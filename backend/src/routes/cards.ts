import { Router } from 'express'
import { listCards, createCard, updateCard, deleteCard } from '../controllers/cardsController'

const router = Router()
router.get('/',    listCards)
router.post('/',   createCard)
router.put('/:id', updateCard)
router.delete('/:id', deleteCard)
export default router
