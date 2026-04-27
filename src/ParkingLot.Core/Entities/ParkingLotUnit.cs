using ParkingLot.Core.Common;
using ParkingLot.Core.Enums;

namespace ParkingLot.Core.Entities;

public sealed class ParkingLotUnit
{
    private readonly List<ParkingSpot> _spots;

    private ParkingLotUnit(Guid id, DateTimeOffset createdAt, IEnumerable<ParkingSpot> spots)
    {
        Id = id;
        CreatedAt = createdAt;
        _spots = spots.OrderBy(spot => spot.SpotNumber).ToList();
    }

    public Guid Id { get; }
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<ParkingSpot> Spots => _spots;

    public static ParkingLotUnit Create(int smallSpots, int regularSpots, int largeSpots)
    {
        ValidateSpotCount(smallSpots, nameof(smallSpots));
        ValidateSpotCount(regularSpots, nameof(regularSpots));
        ValidateSpotCount(largeSpots, nameof(largeSpots));

        return new ParkingLotUnit(Guid.NewGuid(), DateTimeOffset.UtcNow, CreateSpots(smallSpots, regularSpots, largeSpots));
    }

    public static ParkingLotUnit Rehydrate(Guid id, DateTimeOffset createdAt, IEnumerable<ParkingSpot> spots)
        => new(id, createdAt, spots);

    public ParkingOperationResult ReplaceSpotLayout(int smallSpots, int regularSpots, int largeSpots)
    {
        ValidateSpotCount(smallSpots, nameof(smallSpots));
        ValidateSpotCount(regularSpots, nameof(regularSpots));
        ValidateSpotCount(largeSpots, nameof(largeSpots));

        if (_spots.Any(spot => !spot.IsAvailable))
        {
            return ParkingOperationResult.Failure("Parking spots can only be edited when no vehicles are parked.");
        }

        var replacement = CreateSpots(smallSpots, regularSpots, largeSpots);
        _spots.Clear();
        _spots.AddRange(replacement);

        return ParkingOperationResult.Success("Parking spots updated.", replacement.Select(spot => spot.SpotNumber).ToArray());
    }

    public ParkingOperationResult ParkVehicle(VehicleType vehicleType, string licensePlate)
    {
        var normalizedLicensePlate = NormalizeLicensePlate(licensePlate);

        if (_spots.Any(spot => string.Equals(
            spot.ParkedVehicle?.LicensePlate,
            normalizedLicensePlate,
            StringComparison.OrdinalIgnoreCase)))
        {
            return ParkingOperationResult.Failure($"Vehicle {normalizedLicensePlate} is already parked.");
        }

        var targetSpots = vehicleType switch
        {
            VehicleType.Motorcycle => FindAvailableSpots(1, SpotSize.Small, SpotSize.Regular, SpotSize.Large),
            VehicleType.Car => FindAvailableSpots(1, SpotSize.Regular, SpotSize.Large),
            VehicleType.Van => FindAvailableSpots(1, SpotSize.Large).Any()
                ? FindAvailableSpots(1, SpotSize.Large)
                : FindConsecutiveRegularSpots(3),
            _ => Array.Empty<ParkingSpot>()
        };

        if (targetSpots.Count == 0)
        {
            return ParkingOperationResult.Failure($"No available spots for vehicle {normalizedLicensePlate}.");
        }

        var vehicle = new ParkedVehicle(normalizedLicensePlate, vehicleType);
        foreach (var spot in targetSpots)
        {
            spot.Park(vehicle);
        }

        var spotNumbers = targetSpots.Select(spot => spot.SpotNumber).ToArray();
        return ParkingOperationResult.Success(
            $"Vehicle {normalizedLicensePlate} parked at spot(s): {string.Join(", ", spotNumbers)}.",
            spotNumbers);
    }

    public ParkingOperationResult VacateVehicle(string licensePlate)
    {
        var normalizedLicensePlate = NormalizeLicensePlate(licensePlate);
        var occupiedSpots = _spots
            .Where(spot => string.Equals(
                spot.ParkedVehicle?.LicensePlate,
                normalizedLicensePlate,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (occupiedSpots.Count == 0)
        {
            return ParkingOperationResult.Failure($"Vehicle {normalizedLicensePlate} is not parked in this lot.");
        }

        foreach (var spot in occupiedSpots)
        {
            spot.Vacate();
        }

        var spotNumbers = occupiedSpots.Select(spot => spot.SpotNumber).ToArray();
        return ParkingOperationResult.Success(
            $"Vehicle {normalizedLicensePlate} vacated spot(s): {string.Join(", ", spotNumbers)}.",
            spotNumbers);
    }

    public ParkingLotStatus GetStatus(SpotSize requestedSpotSize)
        => new(
            Id,
            TotalSpots: _spots.Count,
            IsFull: _spots.All(spot => !spot.IsAvailable),
            IsEmpty: _spots.All(spot => spot.IsAvailable),
            RemainingSpotNumbers: _spots.Where(spot => spot.IsAvailable).Select(spot => spot.SpotNumber).ToArray(),
            AreAllRequestedSizeSpotsTaken: _spots.Any(spot => spot.Size == requestedSpotSize)
                && _spots.Where(spot => spot.Size == requestedSpotSize).All(spot => !spot.IsAvailable),
            SpotsTakenByVans: _spots.Count(spot => spot.ParkedVehicle?.Type == VehicleType.Van));

    private static void ValidateSpotCount(int count, string parameterName)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Spot counts cannot be negative.");
        }
    }

    private static string NormalizeLicensePlate(string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            throw new ArgumentException("License plate is required.", nameof(licensePlate));
        }

        return licensePlate.Trim().ToUpperInvariant();
    }

    private static IReadOnlyList<ParkingSpot> CreateSpots(
        int smallSpots,
        int regularSpots,
        int largeSpots)
    {
        var spots = new List<ParkingSpot>();
        var spotNumber = 1;

        for (var i = 0; i < smallSpots; i++)
        {
            spots.Add(ParkingSpot.Create(spotNumber++, SpotSize.Small));
        }

        for (var i = 0; i < regularSpots; i++)
        {
            spots.Add(ParkingSpot.Create(spotNumber++, SpotSize.Regular));
        }

        for (var i = 0; i < largeSpots; i++)
        {
            spots.Add(ParkingSpot.Create(spotNumber++, SpotSize.Large));
        }

        return spots;
    }

    private IReadOnlyList<ParkingSpot> FindAvailableSpots(int requiredSpots, params SpotSize[] allowedSizes)
        => _spots
            .Where(spot => spot.IsAvailable && allowedSizes.Contains(spot.Size))
            .OrderBy(spot => spot.Size)
            .ThenBy(spot => spot.SpotNumber)
            .Take(requiredSpots)
            .ToArray();

    private IReadOnlyList<ParkingSpot> FindConsecutiveRegularSpots(int requiredSpots)
    {
        var regularSpots = _spots
            .Where(spot => spot.IsAvailable && spot.Size == SpotSize.Regular)
            .OrderBy(spot => spot.SpotNumber)
            .ToList();

        for (var index = 0; index <= regularSpots.Count - requiredSpots; index++)
        {
            var group = regularSpots.GetRange(index, requiredSpots);
            if (group[^1].SpotNumber - group[0].SpotNumber == requiredSpots - 1)
            {
                return group;
            }
        }

        return Array.Empty<ParkingSpot>();
    }
}
