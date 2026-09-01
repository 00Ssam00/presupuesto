export type ToastType = 'error' | 'info'

export const TOAST_EVENT = 'app:toast'

export function showToast(message: string, type: ToastType = 'info') {
  window.dispatchEvent(new CustomEvent<{ message: string; type: ToastType }>(TOAST_EVENT, {
    detail: { message, type },
  }))
}
