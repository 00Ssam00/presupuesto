import { Pool, types } from 'pg'
import dotenv from 'dotenv'
import path from 'path'

dotenv.config({ path: path.resolve(__dirname, '../../../.env') })

// OID 1082 = DATE. Por defecto pg-types lo parsea a un objeto Date en hora local,
// lo que desplaza el día al serializar a JSON (ISO en UTC). Se deja como string
// 'YYYY-MM-DD' tal cual la devuelve Postgres para evitar ese corrimiento.
types.setTypeParser(1082, (val) => val)

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false },
})

export default pool
