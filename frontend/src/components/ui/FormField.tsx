import { ReactNode, useId, cloneElement, isValidElement } from 'react'
import styles from './FormField.module.css'

interface Props {
  label: string
  children: ReactNode
  error?: string
}

export default function FormField({ label, children, error }: Props) {
  const generatedId = useId()
  const child = isValidElement<{ id?: string }>(children) ? children : null
  const id = child?.props.id ?? generatedId
  const content = child ? cloneElement(child, { id }) : children

  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={id}>{label}</label>
      {content}
      {error && <span className={styles.error}>{error}</span>}
    </div>
  )
}
