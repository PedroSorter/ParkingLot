using Npgsql;
using ParkingLot.Core;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Enums;

namespace ParkingLot.Infrastructure.PostgreSQL;

public sealed class PostgresParkingLotRepository(NpgsqlDataSource dataSource) : IParkingLotRepository
{
    public async Task AddAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var command = CreateCommand(
                connection,
                transaction,
                """
                insert into parking_lots (id, created_at)
                values (@id, @created_at);
                """))
            {
                command.Parameters.AddWithValue("id", parkingLot.Id);
                command.Parameters.AddWithValue("created_at", parkingLot.CreatedAt);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await PersistSpotsAsync(connection, transaction, parkingLot, cancellationToken);
            await PersistVehicleAssignmentsAsync(connection, transaction, parkingLot, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ParkingLotUnit?> GetByIdAsync(
        Guid parkingLotId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        DateTimeOffset? createdAt = null;
        await using (var command = CreateCommand(
            connection,
            transaction: null,
            "select created_at from parking_lots where id = @id;"))
        {
            command.Parameters.AddWithValue("id", parkingLotId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            var createdAtUtc = DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc);
            createdAt = new DateTimeOffset(createdAtUtc);
        }

        var spots = new List<ParkingSpot>();
        await using (var command = CreateCommand(
            connection,
            transaction: null,
            """
            select
                s.id,
                s.spot_number,
                s.size,
                v.license_plate,
                v.vehicle_type
            from parking_spots s
            left join vehicle_spots vs on vs.spot_id = s.id
            left join parked_vehicles v on v.id = vs.vehicle_id
            where s.parking_lot_id = @parking_lot_id
            order by s.spot_number;
            """))
        {
            command.Parameters.AddWithValue("parking_lot_id", parkingLotId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var parkedVehicle = reader.IsDBNull(3)
                    ? null
                    : new ParkedVehicle(
                        reader.GetString(3),
                        Enum.Parse<VehicleType>(reader.GetString(4), ignoreCase: true));

                spots.Add(ParkingSpot.Rehydrate(
                    reader.GetGuid(0),
                    reader.GetInt32(1),
                    Enum.Parse<SpotSize>(reader.GetString(2), ignoreCase: true),
                    parkedVehicle));
            }
        }

        return ParkingLotUnit.Rehydrate(parkingLotId, createdAt.Value, spots);
    }

    public async Task<IReadOnlyList<ParkingLotUnit>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var parkingLotIds = new List<Guid>();

        await using (var command = CreateCommand(
            connection,
            transaction: null,
            "select id from parking_lots order by created_at desc;"))
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                parkingLotIds.Add(reader.GetGuid(0));
            }
        }

        var parkingLots = new List<ParkingLotUnit>();

        foreach (var parkingLotId in parkingLotIds)
        {
            var parkingLot = await GetByIdAsync(parkingLotId, cancellationToken);
            if (parkingLot is not null)
            {
                parkingLots.Add(parkingLot);
            }
        }

        return parkingLots;
    }

    public async Task SaveAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var command = CreateCommand(
                connection,
                transaction,
                "delete from parked_vehicles where parking_lot_id = @parking_lot_id;"))
            {
                command.Parameters.AddWithValue("parking_lot_id", parkingLot.Id);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var command = CreateCommand(
                connection,
                transaction,
                "delete from parking_spots where parking_lot_id = @parking_lot_id;"))
            {
                command.Parameters.AddWithValue("parking_lot_id", parkingLot.Id);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await PersistSpotsAsync(connection, transaction, parkingLot, cancellationToken);
            await PersistVehicleAssignmentsAsync(connection, transaction, parkingLot, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid parkingLotId, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(
            connection,
            transaction: null,
            "delete from parking_lots where id = @id;");

        command.Parameters.AddWithValue("id", parkingLotId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task PersistSpotsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        ParkingLotUnit parkingLot,
        CancellationToken cancellationToken)
    {
        foreach (var spot in parkingLot.Spots)
        {
            await using var command = CreateCommand(
                connection,
                transaction,
                """
                insert into parking_spots (id, parking_lot_id, spot_number, size)
                values (@id, @parking_lot_id, @spot_number, @size);
                """);

            command.Parameters.AddWithValue("id", spot.Id);
            command.Parameters.AddWithValue("parking_lot_id", parkingLot.Id);
            command.Parameters.AddWithValue("spot_number", spot.SpotNumber);
            command.Parameters.AddWithValue("size", spot.Size.ToString());

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task PersistVehicleAssignmentsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        ParkingLotUnit parkingLot,
        CancellationToken cancellationToken)
    {
        var parkedVehicles = parkingLot.Spots
            .Where(spot => spot.ParkedVehicle is not null)
            .GroupBy(spot => new
            {
                spot.ParkedVehicle!.LicensePlate,
                spot.ParkedVehicle.Type
            });

        foreach (var vehicleGroup in parkedVehicles)
        {
            var vehicleId = Guid.NewGuid();

            await using (var command = CreateCommand(
                connection,
                transaction,
                """
                insert into parked_vehicles (id, parking_lot_id, license_plate, vehicle_type, parked_at)
                values (@id, @parking_lot_id, @license_plate, @vehicle_type, @parked_at);
                """))
            {
                command.Parameters.AddWithValue("id", vehicleId);
                command.Parameters.AddWithValue("parking_lot_id", parkingLot.Id);
                command.Parameters.AddWithValue("license_plate", vehicleGroup.Key.LicensePlate);
                command.Parameters.AddWithValue("vehicle_type", vehicleGroup.Key.Type.ToString());
                command.Parameters.AddWithValue("parked_at", DateTimeOffset.UtcNow);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var spot in vehicleGroup)
            {
                await using var command = CreateCommand(
                    connection,
                    transaction,
                    """
                    insert into vehicle_spots (vehicle_id, spot_id)
                    values (@vehicle_id, @spot_id);
                    """);

                command.Parameters.AddWithValue("vehicle_id", vehicleId);
                command.Parameters.AddWithValue("spot_id", spot.Id);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    private static NpgsqlCommand CreateCommand(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string commandText)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.Transaction = transaction;
        return command;
    }
}
