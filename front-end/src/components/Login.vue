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
    <div class="login-page">
        <form class="login-form" @submit.prevent="submitForm" novalidate>
            <h2 class="title">Log in</h2>
            <label>
                Email
                <input v-model="email" type="email" required />
            </label>
            <label>
                Password
                <input v-model="password" type="password" required minlength="6" />
            </label>
            <button type="submit">Sign in</button>
            <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
            <p>Don't have an account yet ? </p> 
            <a href="/signin">Sign up</a>
        </form>
    </div>
</template>

<style scoped>
.login-page {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: calc(100vh-100px);
    background: #f9f9f9;
}
.login-form {
    width: 100%;
    max-width: 320px;
    padding: 2rem;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, .1);
    display: flex;
    flex-direction: column;
    gap: 1rem;
}
.title {
    margin: 0 0 1rem;
    text-align: center;
}
label {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    font-weight: 500;
}
input {
    padding: 0.5rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    font-size: 1rem;
}
button {
    padding: 0.6rem;
    font-size: 1rem;
    background: #4a90e2;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}
button:hover {
    background: #357abd;
}
a, p {
    margin: 0;
    padding: 0;
}
a {
    color: #4a90e2;
}
.error {
    color: #e74c3c;
    text-align: center;
}
</style>