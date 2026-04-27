namespace ParkingLot.Core.Common
{
    public sealed record ParkingOperationResult(bool Succeeded, string Message, IReadOnlyList<int> SpotNumbers)
    {
        public static ParkingOperationResult Success(string message, IReadOnlyList<int> spotNumbers)
            => new(true, message, spotNumbers);

        public static ParkingOperationResult Failure(string message)
            => new(false, message, Array.Empty<int>());
    }
}
