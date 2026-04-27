namespace ParkingLot.Core.Entities
{
    public sealed record ParkingLotStatus(
        Guid ParkingLotId,
        int TotalSpots,
        bool IsFull,
        bool IsEmpty,
        IReadOnlyList<int> RemainingSpotNumbers,
        bool AreAllRequestedSizeSpotsTaken,
        int SpotsTakenByVans);
}
