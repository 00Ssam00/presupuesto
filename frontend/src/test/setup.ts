import '@testing-library/jest-dom/vitest'

// Node 22+ registra un `globalThis.localStorage` propio (deshabilitado sin el
// flag --localstorage-file) que pisa el localStorage funcional que expone
// jsdom, dejando `localStorage.setItem` roto en cualquier componente que lo
// use (p. ej. Header, que persiste el tema). Se reemplaza por una
// implementación en memoria para que los tests de componentes/páginas no
// fallen por esta colisión de entornos.
class MemoryStorage implements Storage {
  private store = new Map<string, string>()
  get length() { return this.store.size }
  clear() { this.store.clear() }
  getItem(key: string) { return this.store.has(key) ? this.store.get(key)! : null }
  key(index: number) { return Array.from(this.store.keys())[index] ?? null }
  removeItem(key: string) { this.store.delete(key) }
  setItem(key: string, value: string) { this.store.set(key, String(value)) }
}

const memoryStorage = new MemoryStorage()
Object.defineProperty(globalThis, 'localStorage', { value: memoryStorage, configurable: true })
if (typeof window !== 'undefined') {
  Object.defineProperty(window, 'localStorage', { value: memoryStorage, configurable: true })
}
