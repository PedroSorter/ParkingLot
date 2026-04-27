using ParkingLot.Core.Common;
using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Api.Utils.Responses
{
    public static class OperationsResponses
    {
        public static ParkingLotResponse ToParkingLotResponse(ParkingLotUnit parkingLot) => new(
        parkingLot.Id,
        parkingLot.CreatedAt,
        parkingLot.Spots.Select(spot => new ParkingSpotResponse(
            spot.Id,
            spot.SpotNumber,
            spot.Size,
            spot.ParkedVehicle is null
                ? null
                : new ParkedVehicleResponse(
                    spot.ParkedVehicle.LicensePlate,
                    spot.ParkedVehicle.Type))).ToArray());

        public static ParkingOperationResponse ToOperationResponse(ParkingOperationResult result) => new(
            result.Succeeded, result.Message, result.SpotNumbers);
    }
}
