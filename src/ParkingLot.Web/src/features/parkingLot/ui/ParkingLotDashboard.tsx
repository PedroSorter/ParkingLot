import { useMemo, useState } from 'react'
import type {
  ParkingLot,
  ParkingLotStatus,
  SpotCounts,
  SpotSize,
  VehicleType,
} from '../api/parkingLotApi'
import React from 'react'

type ParkingLotDashboardProps = {
  parkingLot: ParkingLot | null
  parkingLots: ParkingLot[]
  status: ParkingLotStatus | null
  isLoading: boolean
  message: string | null
  onCreate: (counts: SpotCounts) => void
  onUpdateSpots: (counts: SpotCounts) => void
  onParkVehicle: (licensePlate: string, vehicleType: VehicleType) => void
  onVacateVehicle: (licensePlate: string) => void
  onDelete: () => void
  onDeleteParkingLot: (parkingLotId: string) => void
  onRefreshStatus: () => void
  onSelectParkingLot: (parkingLotId: string) => void
  onStatusSizeChange: (spotSize: SpotSize) => void
}

type SubmitEvent = {
  preventDefault: () => void
}

const spotSizes: SpotSize[] = ['Small', 'Regular', 'Large']
const vehicleTypes: VehicleType[] = ['Motorcycle', 'Car', 'Van']

export const ParkingLotDashboard = ({
  parkingLot,
  parkingLots,
  status,
  isLoading,
  message,
  onCreate,
  onUpdateSpots,
  onParkVehicle,
  onVacateVehicle,
  onDelete,
  onDeleteParkingLot,
  onRefreshStatus,
  onSelectParkingLot,
  onStatusSizeChange,
}: ParkingLotDashboardProps) => {
  const [counts, setCounts] = useState<SpotCounts>({
    smallSpots: 4,
    regularSpots: 8,
    largeSpots: 2,
  })
  const [vehicleType, setVehicleType] = useState<VehicleType>('Car')
  const [parkPlate, setParkPlate] = useState('')
  const [vacatePlate, setVacatePlate] = useState('')

  const spotsBySize = useMemo(() => {
    const grouped = new Map<SpotSize, number>()

    for (const spot of parkingLot?.spots ?? []) {
      grouped.set(spot.size, (grouped.get(spot.size) ?? 0) + 1)
    }

    return grouped
  }, [parkingLot])

  const updateCount = (name: keyof SpotCounts, value: string) => {
    setCounts((current) => ({
      ...current,
      [name]: Number(value),
    }))
  }

  const submitCreate = (event: SubmitEvent) => {
    event.preventDefault()
    onCreate(counts)
  }

  const submitUpdate = (event: SubmitEvent) => {
    event.preventDefault()
    onUpdateSpots(counts)
  }

  const submitPark = (event: SubmitEvent) => {
    event.preventDefault()
    onParkVehicle(parkPlate, vehicleType)
    setParkPlate('')
  }

  const submitVacate = (event: SubmitEvent) => {
    event.preventDefault()
    onVacateVehicle(vacatePlate)
    setVacatePlate('')
  }

  return (
    <main className="app-shell">
      <section className="hero-card">
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          <p className="eyebrow">Parking Lot Control</p>
          <h2>Simple parking operations with live API data.</h2>
          <p className="hero-copy">
            Create parking lots, select one from the list, then manage spots, vehicles,
            status, and deletion from one clean workspace.
          </p>
        </div>
        <div className="hero-stat">
          <span>{status?.totalSpots ?? 0}</span>
          <small>Total spots</small>
        </div>
      </section>

      {message ? <div className="notice">{message}</div> : null}

      <section className="panel lot-list-panel">
        <div className="section-title">
          <h2>Parking lots</h2>
          <span>{parkingLots.length} created</span>
        </div>
        <div className="lot-list">
          {parkingLots.map((lot) => (
            <article
              key={lot.id}
              className={parkingLot?.id === lot.id ? 'lot-card selected' : 'lot-card'}
            >
              <button type="button" className="text-button" onClick={() => onSelectParkingLot(lot.id)}>
                <strong>{lot.spots.length} spots</strong>
                <small>{lot.id}</small>
              </button>
              <button
                type="button"
                className="danger compact"
                disabled={isLoading}
                onClick={() => onDeleteParkingLot(lot.id)}
              >
                Delete
              </button>
            </article>
          ))}
          {parkingLots.length === 0 ? (
            <p className="empty-state">No parking lots created yet.</p>
          ) : null}
        </div>
      </section>

      <section className="content-grid">
        <div className="panel">
          <h2>Create parking lot</h2>
          <form className="form-grid" onSubmit={submitCreate}>
            <NumberField
              label="Small"
              value={counts.smallSpots}
              disabled={isLoading}
              onChange={(value) => updateCount('smallSpots', value)}
            />
            <NumberField
              label="Regular"
              value={counts.regularSpots}
              disabled={isLoading}
              onChange={(value) => updateCount('regularSpots', value)}
            />
            <NumberField
              label="Large"
              value={counts.largeSpots}
              disabled={isLoading}
              onChange={(value) => updateCount('largeSpots', value)}
            />
            <button type="submit" disabled={isLoading}>
              Create lot
            </button>
          </form>
          <p className="hint">After creating a lot, select it from the list before adding vehicles or editing spots.</p>
        </div>

        <div className="panel">
          <h2>Edit selected lot</h2>
          {!parkingLot ? <p className="selection-required">Select a parking lot first.</p> : null}
          <form className="form-grid" onSubmit={submitUpdate}>
            <NumberField
              label="Small"
              value={counts.smallSpots}
              onChange={(value) => updateCount('smallSpots', value)}
            />
            <NumberField
              label="Regular"
              value={counts.regularSpots}
              onChange={(value) => updateCount('regularSpots', value)}
            />
            <NumberField
              label="Large"
              value={counts.largeSpots}
              onChange={(value) => updateCount('largeSpots', value)}
            />
            <button type="submit" disabled={!parkingLot || isLoading}>
              Update empty lot
            </button>
          </form>
          <p className="hint">Spot counts can be edited only when no vehicle is parked.</p>
        </div>
      </section>

      <section className="content-grid">
        <div className="panel">
          <h2>Vehicle actions</h2>
          {!parkingLot ? <p className="selection-required">Select a parking lot first.</p> : null}
          <form className="stack" onSubmit={submitPark}>
            <label>
              License plate
              <input
                value={parkPlate}
                disabled={!parkingLot || isLoading}
                onChange={(event) => setParkPlate(event.target.value)}
                placeholder="ABC-123"
                required
              />
            </label>
            <label>
              Vehicle type
              <select
                value={vehicleType}
                disabled={!parkingLot || isLoading}
                onChange={(event) => setVehicleType(event.target.value as VehicleType)}
              >
                {vehicleTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </label>
            <button type="submit" disabled={!parkingLot || isLoading}>
              Park vehicle
            </button>
          </form>

          <form className="inline-form" onSubmit={submitVacate}>
            <input
              value={vacatePlate}
              disabled={!parkingLot || isLoading}
              onChange={(event) => setVacatePlate(event.target.value)}
              placeholder="Plate to vacate"
              required
            />
            <button type="submit" disabled={!parkingLot || isLoading}>
              Vacate
            </button>
          </form>
        </div>

        <div className="panel">
          <div className="section-title">
            <h2>Status</h2>
            <div className="status-actions">          
              <button type="button" disabled={!parkingLot || isLoading} onClick={onRefreshStatus}>
                Refresh status
              </button>
            </div>
          </div>
          {!parkingLot ? <p className="selection-required">Select a parking lot first.</p> : null}

          <div className="status-grid">
            <Metric label="Full" value={status?.isFull ? 'Yes' : 'No'} />
            <Metric label="Empty" value={status?.isEmpty ? 'Yes' : 'No'} />
            <Metric label="Van spots" value={status?.spotsTakenByVans ?? 0} />
            <Metric label="Remaining Spot Numbers" value={status?.remainingSpotNumbers.length ?? 0} />
          </div>

          <div className="size-pills">
            {spotSizes.map((size) => (
              <span key={size}>
                {size}: {spotsBySize.get(size) ?? 0}
              </span>
            ))}
          </div>
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginTop: '20px' }}> 
            <select disabled={!parkingLot || isLoading} onChange={(event) => onStatusSizeChange(event.target.value as SpotSize)}>
                {spotSizes.map((size) => (
                  <option key={size} value={size}>
                    {size}
                  </option>
                ))}
            </select>
            <h3>Are All Requested Size Spots Take?</h3>
            <span>{status?.areAllRequestedSizeSpotsTaken ? 'Yes' : 'No'}  </span> 
          </div>      
        </div>

        <div className="panel danger-panel">
          <h2>Lot management</h2>
          <p className="hint">
            Deleting a parking lot removes every spot and vehicle assignment for that lot.
          </p>
          <button type="button" className="danger" disabled={!parkingLot || isLoading} onClick={onDelete}>
            Delete parking lot
          </button>
        </div>
      </section>

      <section className="panel">
        <div className="section-title">
          <h2>Spots</h2>
          <span>{parkingLot?.id ?? 'No lot created yet'}</span>
        </div>
        <div className="spot-grid" aria-label="Parking spots">
          {(parkingLot?.spots ?? []).map((spot) => (
            <article key={spot.id} className={spot.parkedVehicle ? 'spot occupied' : 'spot'}>
              <strong>#{spot.spotNumber}</strong>
              <span>{spot.size}</span>
              <small>{spot.parkedVehicle?.licensePlate ?? 'Available'}</small>
            </article>
          ))}
          {!parkingLot ? <p className="empty-state">Create a parking lot to see its spots.</p> : null}
        </div>
      </section>
    </main>
  )
}

const NumberField = ({
  label,
  value,
  disabled = false,
  onChange,
}: {
  label: string
  value: number
  disabled?: boolean
  onChange: (value: string) => void
}) => {
  return (
    <label>
      {label}
      <input
        min={0}
        required
        type="number"
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

const Metric = ({ label, value }: { label: string; value: string | number }) => {
  return (
    <div className="metric">
      <span>{value}</span>
      <small>{label}</small>
    </div>
  )
}
