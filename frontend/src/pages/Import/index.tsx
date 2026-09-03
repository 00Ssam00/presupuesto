import { useState, useRef } from 'react'
import Header from '../../components/layout/Header'
import Card from '../../components/ui/Card'
import Button from '../../components/ui/Button'
import FormField from '../../components/ui/FormField'
import { importTransactions, getTransactions } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'
import { parseCsvLine, csvEscapeField } from '../../lib/utils'
import type { ImportResponse } from '../../lib/api'
import type { Transaction } from '../../types'
import styles from './Import.module.css'

interface CsvRow {
  monto: string
  tipo: string
  fecha?: string
  categoria?: string
  metodo?: string
  tarjeta?: string
  descripcion?: string
}

export default function ImportExport() {
  const { periods, currentPeriod, loading: refLoading } = useReferenceData()

  const [rows, setRows] = useState<CsvRow[]>([])
  const [parseError, setParseError] = useState('')
  const [targetPeriodId, setTargetPeriodId] = useState('')
  const [importing, setImporting] = useState(false)
  const [importResult, setImportResult] = useState<ImportResponse | null>(null)
  const fileRef = useRef<HTMLInputElement>(null)

  const [exportPeriodId, setExportPeriodId] = useState('')
  const [exporting, setExporting] = useState(false)

  function parseCsv(text: string): CsvRow[] {
    const lines = text.trim().split(/\r?\n/)
    if (lines.length < 2) throw new Error('El archivo está vacío o no tiene datos')
    const headers = parseCsvLine(lines[0]).map(h => h.toLowerCase())

    const requiredCols = ['monto', 'tipo']
    for (const col of requiredCols) {
      if (!headers.includes(col)) throw new Error(`Columna requerida faltante: "${col}"`)
    }

    return lines.slice(1).filter(l => l.trim()).map(line => {
      const values = parseCsvLine(line)
      const row: Record<string, string> = {}
      headers.forEach((h, i) => { row[h] = values[i] || '' })
      return row as unknown as CsvRow
    }).filter(r => r.monto && r.tipo)
  }

  function handleFile(e: React.ChangeEvent<HTMLInputElement>) {
    setParseError('')
    setRows([])
    setImportResult(null)
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = ev => {
      try {
        const parsed = parseCsv(ev.target?.result as string)
        setRows(parsed)
        if (!targetPeriodId) setTargetPeriodId(currentPeriod?.id || periods[0]?.id || '')
      } catch (err) {
        setParseError((err as Error).message)
      }
    }
    reader.readAsText(file)
  }

  async function handleImport() {
    if (!targetPeriodId || rows.length === 0) return
    setImporting(true)
    setImportResult(null)
    try {
      const result = await importTransactions(targetPeriodId, rows)
      setImportResult(result)
      if (result.created > 0) {
        setRows([])
        if (fileRef.current) fileRef.current.value = ''
      }
    } finally {
      setImporting(false)
    }
  }

  function downloadTemplate() {
    const csv = [
      'monto,tipo,fecha,categoria,metodo,tarjeta,descripcion',
      '85000,expense,2026-04-03,Alimentación,Débito,,Mercado semanal',
      '1022000,income,2026-04-01,Ahorro,Transferencia,,Ingreso quincenal',
    ].join('\n')
    const blob = new Blob([csv], { type: 'text/csv' })
    const url  = URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = 'plantilla_transacciones.csv'
    a.click()
    URL.revokeObjectURL(url)
  }

  async function handleExport() {
    setExporting(true)
    try {
      const params = exportPeriodId ? { period_id: exportPeriodId } : undefined
      const transactions = await getTransactions(params)
      const header = ['monto', 'tipo', 'fecha', 'categoria', 'metodo', 'tarjeta', 'descripcion']
      const lines = [header.join(',')]
      transactions.forEach((t: Transaction) => {
        lines.push([
          String(t.amount),
          t.type,
          t.date.slice(0, 10),
          t.category_name || '',
          t.payment_method_name || '',
          t.card_name || '',
          t.description,
        ].map(csvEscapeField).join(','))
      })
      const blob = new Blob([lines.join('\n')], { type: 'text/csv' })
      const url  = URL.createObjectURL(blob)
      const a    = document.createElement('a')
      a.href     = url
      a.download = 'transacciones.csv'
      a.click()
      URL.revokeObjectURL(url)
    } finally {
      setExporting(false)
    }
  }

  const failedRows = importResult?.results.filter(r => r.status === 'error') || []

  return (
    <div>
      <Header title="Importar / Exportar" />

      <div className={styles.grid}>
        <Card>
          <h2 className={styles.title}>Importar transacciones</h2>
          <p className={styles.desc}>
            Sube un archivo CSV con columnas: <code>monto</code>, <code>tipo</code> (requeridos, <code>tipo</code> es
            "income" o "expense") y <code>fecha</code>, <code>categoria</code>, <code>metodo</code>, <code>tarjeta</code>,{' '}
            <code>descripcion</code> (opcionales). <code>categoria</code> y <code>metodo</code> deben coincidir con
            nombres existentes para que la fila se pueda importar.
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

          {parseError && <div className={styles.error}>{parseError}</div>}

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

              <FormField label="Quincena de destino">
                <select value={targetPeriodId} onChange={e => setTargetPeriodId(e.target.value)} disabled={refLoading}>
                  <option value="">Seleccionar...</option>
                  {periods.map(p => <option key={p.id} value={p.id}>{p.label}</option>)}
                </select>
              </FormField>
              <p className={styles.muted}>
                Las fechas de las filas deben caer dentro del rango de la quincena elegida; si una fila no trae fecha, se usa el inicio de la quincena.
              </p>

              <div className={styles.actions} style={{ marginTop: 'var(--space-3)' }}>
                <Button size="sm" onClick={handleImport} disabled={!targetPeriodId || importing}>
                  {importing ? 'Importando...' : `Importar ${rows.length} fila${rows.length === 1 ? '' : 's'}`}
                </Button>
              </div>
            </div>
          )}

          {importResult && (
            <div className={styles.result}>
              <p className={importResult.failed > 0 ? styles.warning : styles.success}>
                {importResult.created} transacción{importResult.created === 1 ? '' : 'es'} importada{importResult.created === 1 ? '' : 's'}
                {importResult.failed > 0 && `, ${importResult.failed} fila${importResult.failed === 1 ? '' : 's'} con errores`}.
              </p>
              {failedRows.length > 0 && (
                <ul className={styles.errorList}>
                  {failedRows.map(r => (
                    <li key={r.row}>Fila {r.row}: {r.error}</li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </Card>

        <Card>
          <h2 className={styles.title}>Exportar datos</h2>
          <p className={styles.desc}>Descarga tus transacciones en formato CSV.</p>
          <FormField label="Quincena">
            <select value={exportPeriodId} onChange={e => setExportPeriodId(e.target.value)} disabled={refLoading}>
              <option value="">Todas las quincenas</option>
              {periods.map(p => <option key={p.id} value={p.id}>{p.label}</option>)}
            </select>
          </FormField>
          <Button variant="secondary" size="sm" onClick={handleExport} disabled={exporting}>
            {exporting ? 'Exportando...' : 'Exportar transacciones'}
          </Button>
        </Card>
      </div>
    </div>
  )
}
