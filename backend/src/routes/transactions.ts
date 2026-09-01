import { Router } from 'express'
import {
  listTransactions, getTransaction,
  createTransaction, updateTransaction, deleteTransaction
} from '../controllers/transactionsController'

const router = Router()
router.get('/',    listTransactions)
router.get('/:id', getTransaction)
router.post('/',   createTransaction)
router.put('/:id', updateTransaction)
router.delete('/:id', deleteTransaction)
export default router
