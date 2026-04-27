import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterAll, afterEach, beforeAll, expect, test } from 'vitest'
import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { ParkingLotPage } from './ParkingLotPage'
import React from 'react'

const parkingLot = {
  id: 'lot-1',
  createdAt: '2026-04-27T00:00:00Z',
  spots: [
    { id: 'spot-1', spotNumber: 1, size: 'Small', parkedVehicle: null },
    { id: 'spot-2', spotNumber: 2, size: 'Regular', parkedVehicle: null },
    { id: 'spot-3', spotNumber: 3, size: 'Large', parkedVehicle: null },
  ],
}

let parkingLots = [parkingLot]

const server = setupServer(
  http.get('*/api/parking-lots', () => HttpResponse.json(parkingLots)),
  http.post('*/api/parking-lots', () => {
    parkingLots = [parkingLot]
    return HttpResponse.json(parkingLot, { status: 201 })
  }),
  http.delete('*/api/parking-lots/:id', () => {
    parkingLots = []
    return new HttpResponse(null, { status: 204 })
  }),
  http.get('*/api/parking-lots/:id', () => HttpResponse.json(parkingLot)),
  http.get('*/api/parking-lots/:id/status', () =>
    HttpResponse.json({
      parkingLotId: 'lot-1',
      totalSpots: 3,
      isFull: false,
      isEmpty: true,
      remainingSpotNumbers: [1, 2, 3],
      areAllRequestedSizeSpotsTaken: false,
      spotsTakenByVans: 0,
    }),
  ),
  http.post('*/api/parking-lots/:id/vacate', () =>
    HttpResponse.json(
      {
        succeeded: false,
        message: 'Vehicle TEST is not parked in this lot.',
        spotNumbers: [],
      },
      { status: 404 },
    ),
  ),
)

beforeAll(() => server.listen())
afterEach(() => {
  parkingLots = [parkingLot]
  server.resetHandlers()
})
afterAll(() => server.close())

test('renders parking lots list and refreshes selected lot status by button', async () => {
  renderParkingLotPage()

  expect(await screen.findByText('1 created')).toBeInTheDocument()
  expect(screen.getByRole('button', { name: /park vehicle/i })).toBeDisabled()
  expect(screen.getByRole('button', { name: /update empty lot/i })).toBeDisabled()

  await userEvent.click(screen.getByRole('button', { name: /3 spots/i }))

  expect(screen.getByRole('button', { name: /park vehicle/i })).toBeEnabled()
  expect(screen.getByRole('button', { name: /update empty lot/i })).toBeEnabled()
  expect(await screen.findByText('#1')).toBeInTheDocument()
  expect(await screen.findByText('#2')).toBeInTheDocument()
  expect(await screen.findByText('#3')).toBeInTheDocument()

  await userEvent.click(screen.getByRole('button', { name: /refresh status/i }))

  expect(await screen.findByText('Total spots')).toBeInTheDocument()
})

test('deletes a parking lot from the list', async () => {
  renderParkingLotPage()

  expect(await screen.findByText('1 created')).toBeInTheDocument()

  await userEvent.click(screen.getByRole('button', { name: /^delete$/i }))

  expect(await screen.findByText('Parking lot deleted.')).toBeInTheDocument()
  expect(await screen.findByText('0 created')).toBeInTheDocument()
})

test('shows the operation message from json error responses', async () => {
  renderParkingLotPage()

  await userEvent.click(await screen.findByRole('button', { name: /3 spots/i }))
  await userEvent.type(screen.getByPlaceholderText(/plate to vacate/i), 'TEST')
  await userEvent.click(screen.getByRole('button', { name: /vacate/i }))

  expect(await screen.findByText('Vehicle TEST is not parked in this lot.')).toBeInTheDocument()
})

const renderParkingLotPage = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  render(
    <QueryClientProvider client={queryClient}>
      <ParkingLotPage />
    </QueryClientProvider>,
  )
}
