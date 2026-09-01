import { useState, useRef } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import styles from './Import.module.css'

interface CsvRow {
  monto: string
  tipo: string
  fecha?: string
  categoria?: string
  metodo?: string
  descripcion?: string
}

export default function ImportExport() {
  const [rows, setRows] = useState<CsvRow[]>([])
  const [error, setError] = useState('')
  const fileRef = useRef<HTMLInputElement>(null)

  function parseCsv(text: string): CsvRow[] {
    const lines = text.trim().split('\n')
    if (lines.length < 2) throw new Error('El archivo está vacío o no tiene datos')
    const headers = lines[0].split(',').map(h => h.trim().toLowerCase())

    const requiredCols = ['monto', 'tipo']
    for (const col of requiredCols) {
      if (!headers.includes(col)) throw new Error(`Columna requerida faltante: "${col}"`)
    }

    return lines.slice(1).map(line => {
      const values = line.split(',').map(v => v.trim())
      const row: Record<string, string> = {}
      headers.forEach((h, i) => { row[h] = values[i] || '' })
      return row as unknown as CsvRow
    }).filter(r => r.monto && r.tipo)
  }

  function handleFile(e: React.ChangeEvent<HTMLInputElement>) {
    setError('')
    setRows([])
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = ev => {
      try {
        const parsed = parseCsv(ev.target?.result as string)
        setRows(parsed)
      } catch (err) {
        setError((err as Error).message)
      }
    }
    reader.readAsText(file)
  }

  function downloadTemplate() {
    const csv = 'monto,tipo,fecha,categoria,metodo,descripcion\n85000,expense,2026-04-03,Alimentación,Débito,Mercado semanal\n1022000,income,2026-04-01,Ahorro,Transferencia,Ingreso quincenal'
    const blob = new Blob([csv], { type: 'text/csv' })
    const url  = URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = 'plantilla_transacciones.csv'
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div>
      <Header title="Importar / Exportar" />

      <div className={styles.grid}>
        <Card>
          <h2 className={styles.title}>Importar transacciones</h2>
          <p className={styles.desc}>
            Sube un archivo CSV con columnas: <code>monto</code>, <code>tipo</code> (requeridos)
            y <code>fecha</code>, <code>categoria</code>, <code>metodo</code>, <code>descripcion</code> (opcionales).
          </p>

          <div className={styles.actions}>
            <Button variant="secondary" size="sm" onClick={downloadTemplate}>
              Descargar plantilla CSV
            </Button>
            <Button size="sm" onClick={() => fileRef.current?.click()}>
              Seleccionar archivo
            </Button>
            <input ref={fileRef} type="file" accept=".csv" onChange={handleFile} style={{ display: 'none' }} />
          </div>

          {error && <div className={styles.error}>{error}</div>}

          {rows.length > 0 && (
            <div className={styles.preview}>
              <div className={styles.previewHeader}>
                {rows.length} filas detectadas — revisión previa:
              </div>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Monto</th>
                    <th>Tipo</th>
                    <th>Fecha</th>
                    <th>Categoría</th>
                    <th>Método</th>
                    <th>Descripción</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.slice(0, 10).map((r, i) => (
                    <tr key={i}>
                      <td className={styles.num}>{r.monto}</td>
                      <td>{r.tipo}</td>
                      <td>{r.fecha || '—'}</td>
                      <td>{r.categoria || '—'}</td>
                      <td>{r.metodo || '—'}</td>
                      <td>{r.descripcion || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {rows.length > 10 && <p className={styles.muted}>... y {rows.length - 10} filas más</p>}
              <p className={styles.note}>
                La importación completa estará disponible en la próxima versión. Por ahora puedes revisar y cargar manualmente.
              </p>
            </div>
          )}
        </Card>

        <Card>
          <h2 className={styles.title}>Exportar datos</h2>
          <p className={styles.desc}>Descarga tus transacciones en formato CSV.</p>
          <Button variant="secondary" size="sm" disabled>
            Exportar transacciones (próximamente)
          </Button>
        </Card>
      </div>
    </div>
  )
}
