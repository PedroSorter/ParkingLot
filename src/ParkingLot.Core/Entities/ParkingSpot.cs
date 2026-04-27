using ParkingLot.Core.Enums;

namespace ParkingLot.Core.Entities
{
    public sealed class ParkingSpot
    {
        private ParkingSpot(Guid id, int spotNumber, SpotSize size, ParkedVehicle? parkedVehicle)
        {
            Id = id;
            SpotNumber = spotNumber;
            Size = size;
            ParkedVehicle = parkedVehicle;
        }

        public Guid Id { get; }
        public int SpotNumber { get; }
        public SpotSize Size { get; }
        public ParkedVehicle? ParkedVehicle { get; private set; }
        public bool IsAvailable => ParkedVehicle is null;

        public static ParkingSpot Create(int spotNumber, SpotSize size)
            => new(Guid.NewGuid(), spotNumber, size, parkedVehicle: null);

        public static ParkingSpot Rehydrate(Guid id, int spotNumber, SpotSize size, ParkedVehicle? parkedVehicle)
            => new(id, spotNumber, size, parkedVehicle);

        public void Park(ParkedVehicle vehicle)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException($"Spot {SpotNumber} is already occupied.");
            }

            ParkedVehicle = vehicle;
        }

        public void Vacate()
        {
            ParkedVehicle = null;
        }
    }
}
