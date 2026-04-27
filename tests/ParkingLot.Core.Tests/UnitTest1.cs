using ParkingLot.Core.Entities;
using ParkingLot.Core.Enums;

namespace ParkingLot.Core.Tests;

public class ParkingLotTests
{
    [Fact]
    public void Motorcycle_can_use_regular_spot_when_small_spots_are_full()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 0);

        parkingLot.ParkVehicle(VehicleType.Motorcycle, "MOT-1");
        var result = parkingLot.ParkVehicle(VehicleType.Motorcycle, "MOT-2");

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { 2 }, result.SpotNumbers);
    }

    [Fact]
    public void Car_cannot_use_small_spot()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 0, largeSpots: 0);

        var result = parkingLot.ParkVehicle(VehicleType.Car, "CAR-1");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Van_prefers_large_spot()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 0, regularSpots: 3, largeSpots: 1);

        var result = parkingLot.ParkVehicle(VehicleType.Van, "VAN-1");

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { 4 }, result.SpotNumbers);
        Assert.Equal(1, parkingLot.GetStatus(SpotSize.Large).SpotsTakenByVans);
    }

    [Fact]
    public void Van_uses_three_consecutive_regular_spots_when_large_spot_is_unavailable()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 0, regularSpots: 3, largeSpots: 0);

        var result = parkingLot.ParkVehicle(VehicleType.Van, "VAN-1");

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { 1, 2, 3 }, result.SpotNumbers);
        Assert.Equal(3, parkingLot.GetStatus(SpotSize.Regular).SpotsTakenByVans);
    }

    [Fact]
    public void Van_fails_without_three_consecutive_regular_spots_or_large_spot()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 0, regularSpots: 3, largeSpots: 0);
        parkingLot.ParkVehicle(VehicleType.Motorcycle, "MOT-1");

        var result = parkingLot.ParkVehicle(VehicleType.Van, "VAN-1");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Vacating_van_frees_all_assigned_regular_spots()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 0, regularSpots: 3, largeSpots: 0);
        parkingLot.ParkVehicle(VehicleType.Van, "VAN-1");

        var result = parkingLot.VacateVehicle("VAN-1");

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { 1, 2, 3 }, result.SpotNumbers);
        Assert.True(parkingLot.GetStatus(SpotSize.Regular).IsEmpty);
    }

    [Fact]
    public void Empty_parking_lot_can_replace_its_spot_layout()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 1);

        var result = parkingLot.ReplaceSpotLayout(smallSpots: 2, regularSpots: 3, largeSpots: 1);

        Assert.True(result.Succeeded);
        Assert.Equal(6, parkingLot.GetStatus(SpotSize.Small).TotalSpots);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, parkingLot.GetStatus(SpotSize.Small).RemainingSpotNumbers);
    }

    [Fact]
    public void Occupied_parking_lot_cannot_replace_its_spot_layout()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 1);
        parkingLot.ParkVehicle(VehicleType.Car, "CAR-1");

        var result = parkingLot.ReplaceSpotLayout(smallSpots: 2, regularSpots: 2, largeSpots: 2);

        Assert.False(result.Succeeded);
        Assert.Equal(3, parkingLot.GetStatus(SpotSize.Small).TotalSpots);
    }
}
