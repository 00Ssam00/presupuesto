import { Router } from 'express'
import {
  listUpcomingPayments, createUpcomingPayment,
  updateUpcomingPayment, deleteUpcomingPayment
} from '../controllers/upcomingPaymentsController'

const router = Router()
router.get('/',    listUpcomingPayments)
router.post('/',   createUpcomingPayment)
router.put('/:id', updateUpcomingPayment)
router.delete('/:id', deleteUpcomingPayment)
export default router
