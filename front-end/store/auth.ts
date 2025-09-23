import { defineStore } from 'pinia'
import axios from 'axios'

export const useAuth = defineStore('auth', {
    state: () => ({
        token: (localStorage.getItem('token') ?? '') as string,
        email: '' as string | null
    }),
    getters: {
        isLoggedIn: (s) => !!s.token
    },
    actions: {
        setToken(t: string) {
            this.token = t
            if (t) localStorage.setItem('token', t)
            else localStorage.removeItem('token')
        },
        async logout() {
            this.setToken('')
            this.email = null
            delete axios.defaults.headers.common.Authorization
        },
        async fetchProfile() {
            if (!this.token) return
            const res = await axios.get('/api/auth/me')
            this.email = res.data.email
        }
    }
})

