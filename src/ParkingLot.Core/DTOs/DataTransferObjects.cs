using ParkingLot.Core.Enums;

namespace ParkingLot.Core.DTOs
{
    public sealed record CreateParkingLotRequest(int SmallSpots, int RegularSpots, int LargeSpots);

    public sealed record UpdateParkingSpotsRequest(int SmallSpots, int RegularSpots, int LargeSpots);

    public sealed record ParkVehicleRequest(string LicensePlate, VehicleType VehicleType);

    public sealed record VacateVehicleRequest(string LicensePlate);

    public sealed record ParkingLotResponse(Guid Id, DateTimeOffset CreatedAt, IReadOnlyList<ParkingSpotResponse> Spots);

    public sealed record ParkingSpotResponse(
        Guid Id,
        int SpotNumber,
        SpotSize Size,
        ParkedVehicleResponse? ParkedVehicle);

    public sealed record ParkedVehicleResponse(string LicensePlate, VehicleType Type);

    public sealed record ParkingOperationResponse(bool Succeeded, string Message, IReadOnlyList<int> SpotNumbers);

    public sealed record ErrorResponse(string Message);
}
