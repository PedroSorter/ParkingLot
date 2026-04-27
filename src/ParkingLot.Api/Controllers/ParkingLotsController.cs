using Microsoft.AspNetCore.Mvc;
using ParkingLot.Api.Utils.Responses;
using ParkingLot.Core;
using ParkingLot.Core.DTOs;
using ParkingLot.Core.Entities;
using ParkingLot.Core.Enums;

namespace ParkingLot.Api.Controllers;

[ApiController]
[Route("parking-lots")]
public sealed class ParkingLotsController(IParkingLotRepository repository) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ParkingLotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParkingLotResponse>> Create(
        [FromBody] CreateParkingLotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var parkingLot = ParkingLotUnit.Create(
                request.SmallSpots,
                request.RegularSpots,
                request.LargeSpots);

            await repository.AddAsync(parkingLot, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = parkingLot.Id },
                OperationsResponses.ToParkingLotResponse(parkingLot));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingLotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParkingLotResponse>>> List(CancellationToken cancellationToken)
    {
        var parkingLots = await repository.ListAsync(cancellationToken);
        return Ok(parkingLots.Select(OperationsResponses.ToParkingLotResponse).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParkingLotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingLotResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        return parkingLot is null
            ? NotFound()
            : Ok(OperationsResponses.ToParkingLotResponse(parkingLot));
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ParkingLotStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingLotStatus>> GetStatus(
        Guid id,
        [FromQuery] SpotSize spotSize,
        CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        return parkingLot is null
            ? NotFound()
            : Ok(parkingLot.GetStatus(spotSize));
    }

    [HttpPut("{id:guid}/spots")]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingOperationResponse>> UpdateSpots(
        Guid id,
        [FromBody] UpdateParkingSpotsRequest request,
        CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        if (parkingLot is null)
        {
            return NotFound();
        }

        var result = parkingLot.ReplaceSpotLayout(
            request.SmallSpots,
            request.RegularSpots,
            request.LargeSpots);

        if (!result.Succeeded)
        {
            return BadRequest(OperationsResponses.ToOperationResponse(result));
        }

        await repository.SaveAsync(parkingLot, cancellationToken);
        return Ok(OperationsResponses.ToOperationResponse(result));
    }

    [HttpPost("{id:guid}/park")]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingOperationResponse>> Park(
        Guid id,
        [FromBody] ParkVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        if (parkingLot is null)
        {
            return NotFound();
        }

        var result = parkingLot.ParkVehicle(request.VehicleType, request.LicensePlate);
        if (!result.Succeeded)
        {
            return BadRequest(OperationsResponses.ToOperationResponse(result));
        }

        await repository.SaveAsync(parkingLot, cancellationToken);
        return Ok(OperationsResponses.ToOperationResponse(result));
    }

    [HttpPost("{id:guid}/vacate")]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ParkingOperationResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingOperationResponse>> Vacate(
        Guid id,
        [FromBody] VacateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        if (parkingLot is null)
        {
            return NotFound();
        }

        var result = parkingLot.VacateVehicle(request.LicensePlate);
        if (!result.Succeeded)
        {
            return NotFound(OperationsResponses.ToOperationResponse(result));
        }

        await repository.SaveAsync(parkingLot, cancellationToken);
        return Ok(OperationsResponses.ToOperationResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await repository.GetByIdAsync(id, cancellationToken);
        if (parkingLot is null)
        {
            return NotFound();
        }

        await repository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
