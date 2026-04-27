import { z } from 'zod'
import { requestJson } from '../../../shared/api/http'

export const spotSizeSchema = z.enum(['Small', 'Regular', 'Large'])
export const vehicleTypeSchema = z.enum(['Motorcycle', 'Car', 'Van'])

export const parkedVehicleSchema = z.object({
  licensePlate: z.string(),
  type: vehicleTypeSchema,
})

export const parkingSpotSchema = z.object({
  id: z.string(),
  spotNumber: z.number(),
  size: spotSizeSchema,
  parkedVehicle: parkedVehicleSchema.nullable(),
})

export const parkingLotSchema = z.object({
  id: z.string(),
  createdAt: z.string(),
  spots: z.array(parkingSpotSchema),
})

export const parkingLotsSchema = z.array(parkingLotSchema)

export const parkingLotStatusSchema = z.object({
  parkingLotId: z.string(),
  totalSpots: z.number(),
  isFull: z.boolean(),
  isEmpty: z.boolean(),
  remainingSpotNumbers: z.array(z.number()),
  areAllRequestedSizeSpotsTaken: z.boolean(),
  spotsTakenByVans: z.number(),
})

export const operationSchema = z.object({
  succeeded: z.boolean(),
  message: z.string(),
  spotNumbers: z.array(z.number()),
})

export type SpotSize = z.infer<typeof spotSizeSchema>
export type VehicleType = z.infer<typeof vehicleTypeSchema>
export type ParkingLot = z.infer<typeof parkingLotSchema>
export type ParkingLotStatus = z.infer<typeof parkingLotStatusSchema>
export type ParkingOperation = z.infer<typeof operationSchema>

export type SpotCounts = {
  smallSpots: number
  regularSpots: number
  largeSpots: number
}

export type ParkVehicleRequest = {
  licensePlate: string
  vehicleType: VehicleType
}

export const createParkingLot = async (payload: SpotCounts) => {
  const data = await requestJson<unknown>('/parking-lots', {
    method: 'POST',
    body: JSON.stringify(payload),
  })

  return parkingLotSchema.parse(data)
}

export const listParkingLots = async () => {
  const data = await requestJson<unknown>('/parking-lots')
  return parkingLotsSchema.parse(data)
}

export const getParkingLot = async (id: string) => {
  const data = await requestJson<unknown>(`/parking-lots/${id}`)
  return parkingLotSchema.parse(data)
}

export const getParkingLotStatus = async (id: string, spotSize: SpotSize) => {
  const data = await requestJson<unknown>(`/parking-lots/${id}/status?spotSize=${spotSize}`)
  return parkingLotStatusSchema.parse(data)
}

export const updateParkingSpots = async (id: string, payload: SpotCounts) => {
  const data = await requestJson<unknown>(`/parking-lots/${id}/spots`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })

  return operationSchema.parse(data)
}

export const parkVehicle = async (id: string, payload: ParkVehicleRequest) => {
  const data = await requestJson<unknown>(`/parking-lots/${id}/park`, {
    method: 'POST',
    body: JSON.stringify(payload),
  })

  return operationSchema.parse(data)
}

export const vacateVehicle = async (id: string, licensePlate: string) => {
  const data = await requestJson<unknown>(`/parking-lots/${id}/vacate`, {
    method: 'POST',
    body: JSON.stringify({ licensePlate }),
  })

  return operationSchema.parse(data)
}

export const deleteParkingLot = async (id: string) => {
  await requestJson<void>(`/parking-lots/${id}`, {
    method: 'DELETE',
  })
}
