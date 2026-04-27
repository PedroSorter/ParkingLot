export { ParkingLotDashboard } from './ui/ParkingLotDashboard'
export {
  useCreateParkingLotMutation,
  useDeleteParkingLotMutation,
  useParkVehicleMutation,
  useParkingLotQuery,
  useParkingLotsQuery,
  useParkingLotStatusQuery,
  useUpdateParkingSpotsMutation,
  useVacateVehicleMutation,
} from './model/parkingLotHooks'
export type { ParkingLot, ParkingLotStatus, SpotSize, VehicleType } from './api/parkingLotApi'
