import { createApp } from 'vue'
import { applyFavicon } from '@box-pack/web-basics'
import './style.css'
import App from './App.vue'
import { brandingConfig } from './branding.js'
import { router } from '@box-pack/web-navigation-bob'

document.title = brandingConfig.title
applyFavicon()

const app = createApp(App)
app.use(router)
app.mount('#app')
