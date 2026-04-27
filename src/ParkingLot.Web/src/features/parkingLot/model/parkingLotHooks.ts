import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createParkingLot,
  deleteParkingLot,
  getParkingLot,
  getParkingLotStatus,
  listParkingLots,
  parkVehicle,
  updateParkingSpots,
  vacateVehicle,
  type ParkVehicleRequest,
  type SpotCounts,
  type SpotSize,
} from '../api/parkingLotApi'

export const useParkingLotsQuery = () => {
  return useQuery({
    queryKey: ['parking-lots'],
    queryFn: listParkingLots,
  })
}

export const useParkingLotQuery = (parkingLotId: string | null) => {
  return useQuery({
    queryKey: ['parking-lot', parkingLotId],
    queryFn: () => getParkingLot(parkingLotId!),
    enabled: Boolean(parkingLotId),
  })
}

export const useParkingLotStatusQuery = (parkingLotId: string | null, spotSize: SpotSize) => {
  return useQuery({
    queryKey: ['parking-lot-status', parkingLotId, spotSize],
    queryFn: () => getParkingLotStatus(parkingLotId!, spotSize),
    enabled: false,
  })
}

export const useCreateParkingLotMutation = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SpotCounts) => createParkingLot(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['parking-lots'] })
    },
  })
}

export const useUpdateParkingSpotsMutation = (parkingLotId: string | null) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: SpotCounts) => updateParkingSpots(parkingLotId!, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['parking-lots'] })
      await queryClient.invalidateQueries({ queryKey: ['parking-lot', parkingLotId] })
    },
  })
}

export const useParkVehicleMutation = (parkingLotId: string | null) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: ParkVehicleRequest) => parkVehicle(parkingLotId!, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['parking-lots'] })
      await queryClient.invalidateQueries({ queryKey: ['parking-lot', parkingLotId] })
    },
  })
}

export const useVacateVehicleMutation = (parkingLotId: string | null) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (licensePlate: string) => vacateVehicle(parkingLotId!, licensePlate),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['parking-lots'] })
      await queryClient.invalidateQueries({ queryKey: ['parking-lot', parkingLotId] })
    },
  })
}

export const useDeleteParkingLotMutation = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (parkingLotId: string) => deleteParkingLot(parkingLotId),
    onSuccess: async (_data, parkingLotId) => {
      await queryClient.invalidateQueries({ queryKey: ['parking-lots'] })
      await queryClient.removeQueries({ queryKey: ['parking-lot', parkingLotId] })
      await queryClient.removeQueries({ queryKey: ['parking-lot-status', parkingLotId] })
    },
  })
}
