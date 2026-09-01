import styles from './Skeleton.module.css'

interface Props {
  width?: string | number
  height?: string | number
  radius?: string
  className?: string
}

export default function Skeleton({ width = '100%', height = '1em', radius, className = '' }: Props) {
  return (
    <span
      className={`${styles.skeleton} ${className}`}
      style={{ width, height, borderRadius: radius }}
      aria-hidden="true"
    />
  )
}
