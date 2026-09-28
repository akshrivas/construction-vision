import './style.css'
import {
  createHouseSpec,
  defaultForm,
  isFormValid,
  layoutHouse,
  loadSpecs,
  orientations,
  plotLabel,
  saveSpec,
  summaryLine,
} from './houseSpec.js'

const app = document.querySelector('#app')

const state = {
  screen: 'landing',
  form: { ...defaultForm },
  currentSpec: null,
  savedSpecs: loadSpecs(),
}

function setScreen(screen) {
  state.screen = screen
  render()
}

function render() {
  if (state.screen === 'landing') {
    app.innerHTML = landingView()
    bindLanding()
    return
  }
  if (state.screen === 'form') {
    app.innerHTML = formView()
    bindForm()
    return
  }
  if (state.screen === 'home' && state.currentSpec) {
    app.innerHTML = houseView()
    bindHouse()
    return
  }
  app.innerHTML = landingView()
  bindLanding()
}

function landingView() {
  const saved = state.savedSpecs
    .slice(0, 3)
    .map(
      (spec) => `
      <button class="saved-item" data-id="${spec.id}" type="button">
        <strong>${plotLabel(spec)}</strong>
        <span class="muted">${summaryLine(spec)}</span>
      </button>
    `,
    )
    .join('')

  return `
    <main class="screen landing">
      <div class="screen-narrow">
        <p class="brand">Construction Vision</p>
        <h1 class="hero-title">Build your home<br />before you build it.</h1>
        <p class="lead">Enter a few basics. Later you will walk through a possible version of your future home.</p>
        <button class="btn btn-primary" id="create-home" type="button">Create My Home</button>
        ${
          state.savedSpecs.length
            ? `<div class="stack" style="margin-top:28px">
                <p class="muted">Saved designs</p>
                ${saved}
              </div>`
            : ''
        }
      </div>
    </main>
  `
}

function bindLanding() {
  document.querySelector('#create-home')?.addEventListener('click', () => {
    state.form = { ...defaultForm }
    setScreen('form')
  })

  document.querySelectorAll('.saved-item').forEach((button) => {
    button.addEventListener('click', () => {
      const spec = state.savedSpecs.find((item) => item.id === button.dataset.id)
      if (!spec) return
      state.currentSpec = spec
      state.form = {
        width: String(spec.plot.width),
        length: String(spec.plot.length),
        floors: spec.floors,
        bedrooms: spec.bedrooms,
        parking: spec.parking,
        familySize: spec.familySize ?? 4,
        orientation: spec.orientation || '',
        balcony: Boolean(spec.balcony),
      }
      setScreen('home')
    })
  })
}

function formView() {
  const f = state.form
  const chips = orientations
    .map(
      (item) => `
      <button
        class="chip ${(f.orientation || '') === item.value ? 'active' : ''}"
        type="button"
        data-orientation="${item.value}"
      >${item.label}</button>
    `,
    )
    .join('')

  return `
    <main class="screen">
      <div class="screen-narrow stack">
        <button class="back-link" id="back-landing" type="button">← Back</button>
        <h1 class="hero-title" style="font-size:2rem">Tell us the basics</h1>
        <p class="lead">No architecture jargon. Just plot, floors, rooms, and parking.</p>

        <section class="card">
          <span class="label">Plot size (ft)</span>
          <div class="row">
            <label class="field">
              <span class="muted">Width</span>
              <input id="width" inputmode="decimal" value="${f.width}" />
            </label>
            <span>×</span>
            <label class="field">
              <span class="muted">Length</span>
              <input id="length" inputmode="decimal" value="${f.length}" />
            </label>
          </div>
        </section>

        ${stepperCard('Floors', 'floors', f.floors, 1, 4)}
        ${stepperCard('Bedrooms', 'bedrooms', f.bedrooms, 1, 6)}
        ${stepperCard('Parking (cars)', 'parking', f.parking, 0, 3)}
        ${stepperCard('Approx. family size', 'familySize', f.familySize, 1, 12)}

        <section class="card">
          <span class="label">Plot orientation (optional)</span>
          <div class="chip-row">${chips}</div>
        </section>

        <section class="card">
          <div class="toggle-row">
            <div>
              <span class="label" style="margin:0">Balcony</span>
              <strong>${f.balcony ? 'Include balcony' : 'No balcony'}</strong>
            </div>
            <button class="chip ${f.balcony ? 'active' : ''}" id="balcony-toggle" type="button">
              ${f.balcony ? 'Yes' : 'No'}
            </button>
          </div>
        </section>

        <button class="btn btn-dark" id="continue" type="button" ${isFormValid(f) ? '' : 'disabled'}>
          Continue
        </button>
      </div>
    </main>
  `
}

function stepperCard(title, key, value, min, max) {
  return `
    <section class="card">
      <span class="label">${title}</span>
      <div class="stepper">
        <strong>${value}</strong>
        <div class="stepper-actions">
          <button class="chip" type="button" data-step="${key}" data-delta="-1" data-min="${min}" data-max="${max}">−</button>
          <button class="chip" type="button" data-step="${key}" data-delta="1" data-min="${min}" data-max="${max}">+</button>
        </div>
      </div>
    </section>
  `
}

function bindForm() {
  document.querySelector('#back-landing')?.addEventListener('click', () => setScreen('landing'))

  document.querySelector('#width')?.addEventListener('input', (event) => {
    state.form.width = event.target.value
    syncContinue()
  })
  document.querySelector('#length')?.addEventListener('input', (event) => {
    state.form.length = event.target.value
    syncContinue()
  })

  document.querySelectorAll('[data-step]').forEach((button) => {
    button.addEventListener('click', () => {
      const key = button.dataset.step
      const delta = Number(button.dataset.delta)
      const min = Number(button.dataset.min)
      const max = Number(button.dataset.max)
      const next = Math.min(max, Math.max(min, Number(state.form[key]) + delta))
      state.form[key] = next
      render()
    })
  })

  document.querySelectorAll('[data-orientation]').forEach((button) => {
    button.addEventListener('click', () => {
      state.form.orientation = button.dataset.orientation
      render()
    })
  })

  document.querySelector('#balcony-toggle')?.addEventListener('click', () => {
    state.form.balcony = !state.form.balcony
    render()
  })

  document.querySelector('#continue')?.addEventListener('click', () => {
    if (!isFormValid(state.form)) return
    state.currentSpec = createHouseSpec(state.form)
    setScreen('home')
  })
}

function syncContinue() {
  const button = document.querySelector('#continue')
  if (!button) return
  button.disabled = !isFormValid(state.form)
}

function houseView() {
  const spec = state.currentSpec
  const floors = layoutHouse(spec)
    .map(
      (floor) => `
      <section class="floor">
        <h2 class="floor-title">${floor.label}</h2>
        <div class="plot">
          ${floor.rooms
            .map(
              (room) => `
            <div class="room room-${room.kind}${room.wide ? ' room-wide' : ''}">
              <span class="room-label">${room.label}</span>
              ${room.note ? `<span class="room-note">${room.note}</span>` : ''}
            </div>
          `,
            )
            .join('')}
        </div>
      </section>
    `,
    )
    .join('')

  return `
    <main class="screen">
      <div class="screen-narrow stack">
        <button class="back-link" id="back-form" type="button">← Back</button>
        <h1 class="hero-title" style="font-size:2rem">Your home</h1>
        <p class="lead">One version, from the basics you entered.</p>
        <p class="muted">${plotLabel(spec)} · ${summaryLine(spec)}</p>
        ${floors}
        <button class="btn btn-dark" id="save-design" type="button">Save My Design</button>
        <details class="spec-details">
          <summary>House Spec JSON</summary>
          <pre class="json-box">${escapeHtml(JSON.stringify(spec, null, 2))}</pre>
        </details>
      </div>
    </main>
  `
}

function bindHouse() {
  document.querySelector('#back-form')?.addEventListener('click', () => setScreen('form'))

  document.querySelector('#save-design')?.addEventListener('click', () => {
    if (!state.currentSpec) return
    state.savedSpecs = saveSpec(state.currentSpec)
    const button = document.querySelector('#save-design')
    if (button) button.textContent = 'Saved'
  })
}

function escapeHtml(value) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
}

render()
