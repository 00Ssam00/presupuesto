import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import ToastHost from '../ui/ToastHost'
import styles from './Layout.module.css'

export default function Layout() {
  return (
    <div className={styles.layout}>
      <Sidebar />
      <main className={styles.main}>
        <div className={styles.content}>
          <Outlet />
        </div>
      </main>
      <ToastHost />
    </div>
  )
}
