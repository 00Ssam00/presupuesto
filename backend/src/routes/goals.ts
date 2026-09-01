import { Router } from 'express'
import { listGoals, createGoal, updateGoal, deleteGoal } from '../controllers/goalsController'

const router = Router()
router.get('/',    listGoals)
router.post('/',   createGoal)
router.put('/:id', updateGoal)
router.delete('/:id', deleteGoal)
export default router
