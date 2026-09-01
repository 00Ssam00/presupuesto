import { Router } from 'express'
import periodsRouter from './periods'
import transactionsRouter from './transactions'
import categoriesRouter from './categories'
import budgetsRouter from './budgets'
import goalsRouter from './goals'
import upcomingPaymentsRouter from './upcomingPayments'
import cardsRouter from './cards'
import paymentMethodsRouter from './paymentMethods'
import dashboardRouter from './dashboard'
import settingsRouter from './settings'

const router = Router()

router.use('/periods',           periodsRouter)
router.use('/transactions',      transactionsRouter)
router.use('/categories',        categoriesRouter)
router.use('/budgets',           budgetsRouter)
router.use('/goals',             goalsRouter)
router.use('/upcoming-payments', upcomingPaymentsRouter)
router.use('/cards',             cardsRouter)
router.use('/payment-methods',   paymentMethodsRouter)
router.use('/dashboard',         dashboardRouter)
router.use('/settings',          settingsRouter)

export default router
