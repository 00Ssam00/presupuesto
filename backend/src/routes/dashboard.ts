import { Router } from 'express'
import { getDashboard } from '../controllers/dashboardController'

const router = Router()
router.get('/:periodId', getDashboard)
export default router
