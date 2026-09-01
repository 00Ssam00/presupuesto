import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/layout/Layout'
import Dashboard from './pages/Dashboard'
import Transactions from './pages/Transactions'
import Budget from './pages/Budget'
import Goals from './pages/Goals'
import ImportExport from './pages/Import'
import Settings from './pages/Settings'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard"    element={<Dashboard />} />
          <Route path="transactions" element={<Transactions />} />
          <Route path="budget"       element={<Budget />} />
          <Route path="goals"        element={<Goals />} />
          <Route path="import"       element={<ImportExport />} />
          <Route path="settings"     element={<Settings />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
