import { useState } from 'react'
import {
  ParkingLotDashboard,
  useCreateParkingLotMutation,
  useDeleteParkingLotMutation,
  useParkVehicleMutation,
  useParkingLotQuery,
  useParkingLotsQuery,
  useParkingLotStatusQuery,
  useUpdateParkingSpotsMutation,
  useVacateVehicleMutation,
  type SpotSize,
  type VehicleType,
} from '../features/parkingLot'
import React from 'react'

export const ParkingLotPage = () => {
  const [parkingLotId, setParkingLotId] = useState<string | null>(null)
  const [statusSize, setStatusSize] = useState<SpotSize>('Small')
  const [message, setMessage] = useState<string | null>(null)

  const parkingLotsQuery = useParkingLotsQuery()
  const parkingLotQuery = useParkingLotQuery(parkingLotId)
  const statusQuery = useParkingLotStatusQuery(parkingLotId, statusSize)
  const createMutation = useCreateParkingLotMutation()
  const updateMutation = useUpdateParkingSpotsMutation(parkingLotId)
  const parkMutation = useParkVehicleMutation(parkingLotId)
  const vacateMutation = useVacateVehicleMutation(parkingLotId)
  const deleteMutation = useDeleteParkingLotMutation()

  const isLoading =
    createMutation.isPending ||
    updateMutation.isPending ||
    parkMutation.isPending ||
    vacateMutation.isPending ||
    deleteMutation.isPending ||
    parkingLotsQuery.isFetching ||
    parkingLotQuery.isFetching ||
    statusQuery.isFetching

  return (
    <ParkingLotDashboard
      parkingLot={parkingLotQuery.data ?? null}
      parkingLots={parkingLotsQuery.data ?? []}
      status={statusQuery.data ?? null}
      isLoading={isLoading}
      message={message}
      onStatusSizeChange={setStatusSize}
      onRefreshStatus={() => {
        if (!parkingLotId) {
          return
        }

        statusQuery.refetch()
      }}
      onSelectParkingLot={(id) => {
        setParkingLotId(id)
        setMessage(null)
      }}
      onDeleteParkingLot={(id) => {
        deleteMutation.mutate(id, {
          onSuccess: () => {
            if (parkingLotId === id) {
              setParkingLotId(null)
            }

            setMessage('Parking lot deleted.')
            window.location.reload()
          },
          onError: (error) => setMessage(error.message),
        })
      }}
      onCreate={(counts) => {
        createMutation.mutate(counts, {
          onSuccess: () => {
            setMessage('Parking lot created. Select it from the list to manage it.')
          },
          onError: (error) => setMessage(error.message),
        })
      }}
      onUpdateSpots={(counts) => {
        updateMutation.mutate(counts, {
          onSuccess: (operation) => setMessage(operation.message),
          onError: (error) => setMessage(error.message),
        })
      }}
      onParkVehicle={(licensePlate: string, vehicleType: VehicleType) => {
        parkMutation.mutate(
          { licensePlate, vehicleType },
          {
            onSuccess: (operation) => setMessage(operation.message),
            onError: (error) => setMessage(error.message),
          },
        )
      }}
      onVacateVehicle={(licensePlate) => {
        vacateMutation.mutate(licensePlate, {
          onSuccess: (operation) => setMessage(operation.message),
          onError: (error) => setMessage(error.message),
        })
      }}
      onDelete={() => {
        if (!parkingLotId) {
          return
        }

        deleteMutation.mutate(parkingLotId, {
          onSuccess: () => {
            setParkingLotId(null)
            setMessage('Parking lot deleted.')
          },
          onError: (error) => setMessage(error.message),
        })
      }}
    />
  )
}
