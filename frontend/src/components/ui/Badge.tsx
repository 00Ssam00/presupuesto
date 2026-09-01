import { ReactNode } from 'react'
import styles from './Badge.module.css'

type Variant = 'default' | 'success' | 'warning' | 'danger'

interface Props {
  children: ReactNode
  variant?: Variant
  style?: React.CSSProperties
}

export default function Badge({ children, variant = 'default', style }: Props) {
  return (
    <span className={`${styles.badge} ${styles[variant]}`} style={style}>
      {children}
    </span>
  )
}
