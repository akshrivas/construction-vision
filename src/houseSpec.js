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
  return `${spec.floors} floor${spec.floors === 1 ? '' : 's'} · ${spec.bedrooms} bed · ${spec.parking} car`
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
