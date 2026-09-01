import { useEffect, useState } from 'react'
import { TOAST_EVENT, type ToastType } from '../../lib/toast'
import styles from './ToastHost.module.css'

interface ToastItem {
  id: number
  message: string
  type: ToastType
}

let nextId = 1

export default function ToastHost() {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  useEffect(() => {
    function handleToast(e: Event) {
      const { message, type } = (e as CustomEvent<{ message: string; type: ToastType }>).detail
      const id = nextId++
      setToasts(prev => [...prev, { id, message, type }])
      setTimeout(() => {
        setToasts(prev => prev.filter(t => t.id !== id))
      }, 5000)
    }
    window.addEventListener(TOAST_EVENT, handleToast)
    return () => window.removeEventListener(TOAST_EVENT, handleToast)
  }, [])

  function dismiss(id: number) {
    setToasts(prev => prev.filter(t => t.id !== id))
  }

  if (toasts.length === 0) return null

  return (
    <div className={styles.host}>
      {toasts.map(t => (
        <div key={t.id} className={`${styles.toast} ${styles[t.type]}`}>
          <span>{t.message}</span>
          <button className={styles.close} onClick={() => dismiss(t.id)} aria-label="Cerrar">✕</button>
        </div>
      ))}
    </div>
  )
}
