import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { router } from './router'
import axios from 'axios'
import { useAuth } from '../store/auth'
import { createPinia } from 'pinia'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

axios.interceptors.request.use((config) => {
    const auth = useAuth()
    if (auth.token) {
        config.headers = config.headers ?? {}
        config.headers.Authorization = `Bearer ${auth.token}`
    }
    return config
})

app.mount('#app')
