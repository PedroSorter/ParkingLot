using Microsoft.AspNetCore.Mvc;
using ParkingLot.Api.Controllers;
using ParkingLot.Core;
using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;

namespace ParkingLot.Api.Tests;

public class ParkingLotsControllerTests
{
    [Fact]
    public async Task Update_spots_returns_ok_when_empty_lot_is_updated()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 1);
        var repository = new InMemoryParkingLotRepository(parkingLot);
        var controller = new ParkingLotsController(repository);

        var response = await controller.UpdateSpots(
            parkingLot.Id,
            new UpdateParkingSpotsRequest(SmallSpots: 2, RegularSpots: 2, LargeSpots: 1),
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var body = Assert.IsType<ParkingOperationResponse>(okResult.Value);
        Assert.True(body.Succeeded);
        Assert.True(repository.WasSaved);
    }

    [Fact]
    public async Task Update_spots_returns_bad_request_when_lot_is_occupied()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 1);
        parkingLot.ParkVehicle(Core.Enums.VehicleType.Car, "CAR-1");
        var repository = new InMemoryParkingLotRepository(parkingLot);
        var controller = new ParkingLotsController(repository);

        var response = await controller.UpdateSpots(
            parkingLot.Id,
            new UpdateParkingSpotsRequest(SmallSpots: 2, RegularSpots: 2, LargeSpots: 1),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.False(repository.WasSaved);
    }

    [Fact]
    public async Task Delete_returns_no_content_when_lot_exists()
    {
        var parkingLot = ParkingLotUnit.Create(smallSpots: 1, regularSpots: 1, largeSpots: 1);
        var repository = new InMemoryParkingLotRepository(parkingLot);
        var controller = new ParkingLotsController(repository);

        var response = await controller.Delete(parkingLot.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
        Assert.True(repository.WasDeleted);
    }

    private sealed class InMemoryParkingLotRepository(ParkingLotUnit? parkingLot) : IParkingLotRepository
    {
        public bool WasSaved { get; private set; }
        public bool WasDeleted { get; private set; }

        public Task AddAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<ParkingLotUnit>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkingLotUnit>>(parkingLot is null ? [] : [parkingLot]);

        public Task<ParkingLotUnit?> GetByIdAsync(Guid parkingLotId, CancellationToken cancellationToken = default)
            => Task.FromResult(parkingLot?.Id == parkingLotId ? parkingLot : null);

        public Task SaveAsync(ParkingLotUnit parkingLot, CancellationToken cancellationToken = default)
        {
            WasSaved = true;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid parkingLotId, CancellationToken cancellationToken = default)
        {
            WasDeleted = true;
            return Task.CompletedTask;
        }
    }
}
