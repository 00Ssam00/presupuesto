import { Router } from 'express'
import {
  listPeriods, getCurrentPeriod, getPeriod,
  createPeriod, updatePeriod, deletePeriod
} from '../controllers/periodsController'

const router = Router()
router.get('/current', getCurrentPeriod)
router.get('/',        listPeriods)
router.get('/:id',     getPeriod)
router.post('/',       createPeriod)
router.put('/:id',     updatePeriod)
router.delete('/:id',  deletePeriod)
export default router
