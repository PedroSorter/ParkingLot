using ParkingLot.Core.Enums;

namespace ParkingLot.Core.Entities
{
    public sealed record ParkedVehicle(string LicensePlate, VehicleType Type);
}
