using Npgsql;

namespace ParkingLot.Infrastructure.PostgreSQL;

public static class PostgresDatabaseMigrator
{
    private const string MigrationId = "001_create_parking_lot_tables";

    public static async Task ApplyMigrationsAsync(
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await using (var lockCommand = connection.CreateCommand())
        {
            lockCommand.CommandText = "select pg_advisory_lock(hashtext('parking_lot_schema_migrations'));";
            await lockCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            await EnsureMigrationTableAsync(connection, cancellationToken);

            if (await HasMigrationRunAsync(connection, cancellationToken))
            {
                return;
            }

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await using (var schemaCommand = connection.CreateCommand())
                {
                    schemaCommand.Transaction = transaction;
                    schemaCommand.CommandText = CreateSchemaSql;
                    await schemaCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var migrationCommand = connection.CreateCommand())
                {
                    migrationCommand.Transaction = transaction;
                    migrationCommand.CommandText = """
                        insert into schema_migrations (id, applied_at)
                        values (@id, @applied_at);
                        """;
                    migrationCommand.Parameters.AddWithValue("id", MigrationId);
                    migrationCommand.Parameters.AddWithValue("applied_at", DateTimeOffset.UtcNow);
                    await migrationCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        finally
        {
            await using var unlockCommand = connection.CreateCommand();
            unlockCommand.CommandText = "select pg_advisory_unlock(hashtext('parking_lot_schema_migrations'));";
            await unlockCommand.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    private static async Task EnsureMigrationTableAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            create table if not exists schema_migrations (
                id text primary key,
                applied_at timestamp with time zone not null
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> HasMigrationRunAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "select exists(select 1 from schema_migrations where id = @id);";
        command.Parameters.AddWithValue("id", MigrationId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    private const string CreateSchemaSql = """
        create table if not exists parking_lots (
            id uuid primary key,
            created_at timestamp with time zone not null
        );

        create table if not exists parking_spots (
            id uuid primary key,
            parking_lot_id uuid not null references parking_lots(id) on delete cascade,
            spot_number integer not null,
            size text not null check (size in ('Small', 'Regular', 'Large')),
            unique (parking_lot_id, spot_number)
        );

        create table if not exists parked_vehicles (
            id uuid primary key,
            parking_lot_id uuid not null references parking_lots(id) on delete cascade,
            license_plate text not null,
            vehicle_type text not null check (vehicle_type in ('Motorcycle', 'Car', 'Van')),
            parked_at timestamp with time zone not null,
            unique (parking_lot_id, license_plate)
        );

        create table if not exists vehicle_spots (
            vehicle_id uuid not null references parked_vehicles(id) on delete cascade,
            spot_id uuid not null references parking_spots(id) on delete cascade,
            primary key (vehicle_id, spot_id),
            unique (spot_id)
        );

        create index if not exists ix_parking_spots_parking_lot_id
            on parking_spots (parking_lot_id);

        create index if not exists ix_parked_vehicles_parking_lot_id
            on parked_vehicles (parking_lot_id);
        """;
}
