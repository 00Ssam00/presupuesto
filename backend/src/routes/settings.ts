import { Router } from 'express'
import { getSettings, updateSetting } from '../controllers/settingsController'

const router = Router()
router.get('/',       getSettings)
router.put('/:key',   updateSetting)
export default router
