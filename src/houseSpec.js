const STORAGE_KEY = 'cv_house_specs'

export function createHouseSpec(form) {
  const width = Number(form.width)
  const length = Number(form.length)
  const floors = Number(form.floors)
  const bedrooms = Number(form.bedrooms)
  const parking = Number(form.parking)
  const familySize = Number(form.familySize)

  const rooms = ['living', 'kitchen']
  for (let i = 0; i < bedrooms; i += 1) rooms.push('bedroom')
  if (floors > 1) rooms.push('stairs')

  return {
    id: crypto.randomUUID(),
    plot: { width, length },
    floors,
    bedrooms,
    parking,
    familySize,
    orientation: form.orientation || null,
    rooms,
    balcony: Boolean(form.balcony),
    createdAt: new Date().toISOString(),
    status: 'exploring',
  }
}

export function isFormValid(form) {
  const width = Number(form.width)
  const length = Number(form.length)
  return (
    width > 0 &&
    length > 0 &&
    Number(form.floors) >= 1 &&
    Number(form.bedrooms) >= 1 &&
    Number(form.parking) >= 0
  )
}

export function plotLabel(spec) {
  return `${spec.plot.width} × ${spec.plot.length} ft`
}

export function summaryLine(spec) {
  const parking = spec.parking === 0 ? 'no parking' : `${spec.parking} car${spec.parking === 1 ? '' : 's'}`
  return `${spec.floors} floor${spec.floors === 1 ? '' : 's'} · ${spec.bedrooms} bed · ${parking}`
}

const FLOOR_LABELS = ['Ground floor', 'First floor', 'Second floor', 'Third floor']

export function layoutHouse(spec) {
  const floorCount = Number(spec.floors) || 1
  const floors = Array.from({ length: floorCount }, (_, level) => ({
    label: FLOOR_LABELS[level] || `Floor ${level + 1}`,
    rooms: [],
  }))

  if (spec.parking > 0) {
    floors[0].rooms.push({
      kind: 'parking',
      wide: true,
      label: 'Parking',
      note: spec.parking === 1 ? '1 car' : `${spec.parking} cars`,
    })
  }

  floors[0].rooms.push({ kind: 'living', wide: true, label: 'Living', note: '' })
  floors[0].rooms.push({ kind: 'kitchen', wide: false, label: 'Kitchen', note: '' })

  const counts = bedroomCounts(Number(spec.bedrooms) || 0, floorCount)
  let bedroomIndex = 0
  counts.forEach((count, level) => {
    for (let i = 0; i < count; i += 1) {
      bedroomIndex += 1
      floors[level].rooms.push({
        kind: 'bedroom',
        wide: false,
        label: spec.bedrooms === 1 ? 'Bedroom' : `Bedroom ${bedroomIndex}`,
        note: '',
      })
    }
  })

  if (floorCount > 1) {
    floors.forEach((floor) => {
      floor.rooms.push({ kind: 'stairs', wide: false, label: 'Stairs', note: '' })
    })
  }

  if (spec.balcony) {
    floors[floorCount - 1].rooms.push({
      kind: 'balcony',
      wide: true,
      label: 'Balcony',
      note: '',
    })
  }

  floors.forEach((floor) => fillLastGap(floor.rooms))
  return floors
}

function fillLastGap(rooms) {
  let runStart = 0
  const closeRun = (end) => {
    const count = end - runStart
    if (count % 2 === 1) rooms[end - 1].wide = true
  }

  rooms.forEach((room, index) => {
    if (!room.wide) return
    closeRun(index)
    runStart = index + 1
  })
  closeRun(rooms.length)
}

function bedroomCounts(bedrooms, floorCount) {
  const counts = Array(floorCount).fill(0)
  if (floorCount <= 1) {
    counts[0] = bedrooms
    return counts
  }

  const base = Math.floor(bedrooms / floorCount)
  let extra = bedrooms % floorCount
  for (let i = 0; i < floorCount; i += 1) counts[i] = base
  for (let i = floorCount - 1; i >= 0 && extra > 0; i -= 1) {
    counts[i] += 1
    extra -= 1
  }
  return counts
}

export function loadSpecs() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : []
  } catch {
    return []
  }
}

export function saveSpec(spec) {
  const all = loadSpecs().filter((item) => item.id !== spec.id)
  all.unshift(spec)
  localStorage.setItem(STORAGE_KEY, JSON.stringify(all))
  return all
}

export const defaultForm = {
  width: '30',
  length: '50',
  floors: 2,
  bedrooms: 3,
  parking: 1,
  familySize: 4,
  orientation: 'east',
  balcony: true,
}

export const orientations = [
  { value: '', label: 'Skip' },
  { value: 'north', label: 'North' },
  { value: 'south', label: 'South' },
  { value: 'east', label: 'East' },
  { value: 'west', label: 'West' },
]
