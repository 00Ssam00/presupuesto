import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'

function applyTheme() {
  const saved = localStorage.getItem('theme')
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
  const theme = saved === 'light' || saved === 'dark'
    ? saved
    : (new Date().getHours() >= 20 || new Date().getHours() < 7 ? 'dark' : (prefersDark ? 'dark' : 'light'))
  document.documentElement.setAttribute('data-theme', theme)
}

applyTheme()

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
)
