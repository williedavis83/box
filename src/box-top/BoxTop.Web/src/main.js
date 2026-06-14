import { createApp } from 'vue'
import { applyFavicon, applyWebTitle } from '@box-pack/web-basics'
import './style.css'
import App from './App.vue'
import { router } from '@box-pack/web-navigation'

applyWebTitle()
applyFavicon()

const app = createApp(App)
app.use(router)
app.mount('#app')
