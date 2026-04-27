using ParkingLot.Core.Entities;
namespace ParkingLot.Core;

public interface IParkingLotRepository
{
    Task AddAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingLotUnit>> ListAsync(CancellationToken cancellationToken = default);
    Task<ParkingLotUnit?> GetByIdAsync(Guid parkingLotId, CancellationToken cancellationToken = default);
    Task SaveAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid parkingLotId, CancellationToken cancellationToken = default);
}
