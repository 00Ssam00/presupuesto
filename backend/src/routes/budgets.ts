import { Router } from 'express'
import { getBudgetsByPeriod, upsertBudget, deleteBudget } from '../controllers/budgetsController'

const router = Router()
router.get('/period/:periodId', getBudgetsByPeriod)
router.post('/',                upsertBudget)
router.delete('/:id',           deleteBudget)
export default router
