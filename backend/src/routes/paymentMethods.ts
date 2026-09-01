import { Router } from 'express'
import { listPaymentMethods } from '../controllers/paymentMethodsController'

const router = Router()
router.get('/', listPaymentMethods)
export default router
