<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import { useAuth } from '../../store/auth';

const router = useRouter();
const auth = useAuth();

const email = ref('')
const password = ref('')
const errorMsg = ref('')
const loading = ref(false)

function validate(): string | null {
    if (!email.value) return 'Email is required'
    if (password.value.length < 6) return 'Password must be at least 6 characters'
    return null
}

async function submitForm() {
    errorMsg.value = ''
    const v = validate()
    if (v) { errorMsg.value = v; return}
    
    loading.value = true
    try {
        const res = await axios.post('/api/auth/login', { email: email.value, password: password.value})

        if (res.status >= 200 && res.status < 300) {
            const token = res.data.access_token ?? res.data.accessToken
            if (!token) throw new Error('No token in response')
            auth.setToken(token)
            await router.push('/kanban') 
        }
        else {
            errorMsg.value = `Unexpected status: ${res.status}`
        }
    } catch (err: any) {
        const data = err?.response?.data
        if (Array.isArray(data)) {
            errorMsg.value = data.map((e: any) => e.description ?? e.code ?? String(e)).join('\n')
        } else {
            errorMsg.value = data?.title || data?.message || err?.response?.statusText || 'Login failed'
        }
        console.error(err)
    } finally {
        loading.value = false
    }
}
</script>

<template>
    <div class="form-container">
        <form class="login-form form" @submit.prevent="submitForm" novalidate>
            <h2 class="form-title">Log in</h2>
                <input class="form-input" v-model="email" type="email" placeholder="Email" required />
                <input class="form-input" v-model="password" type="password" required minlength="6" placeholder="Password"/>
            <button class="form-button" type="submit">Sign in</button>
            <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
            <p>Don't have an account yet ? </p> 
            <a class="form-link" href="/signin">Sign up</a>
        </form>
    </div>
</template>