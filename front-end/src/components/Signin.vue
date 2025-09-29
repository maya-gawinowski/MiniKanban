<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';

const router = useRouter();

const email = ref('')
const confirmEmail = ref('')
const password = ref('')
const confirmPassword = ref('')
const errorMsg = ref('')
const loading = ref(false)

function validate(): string | null {
    console.log(email == confirmEmail)
    if (!email.value) return 'Email is required'
    if (email.value != confirmEmail.value) return 'Emails do not match'
    if (password.value.length < 6) return 'Password must be at least 6 characters'
    if (password.value != confirmPassword.value) return 'Passwords do not match'
    return null
}

async function submitForm() {
    errorMsg.value = ''
    const v = validate()
    if (v) { errorMsg.value = v; return}
    
    loading.value = true
    try {
        const res = await axios.post('/api/auth/register', { email: email.value, password: password.value})
        if (res.status >= 200 && res.status < 300) {
            await router.push('/login') 
        }
        else {
            errorMsg.value = `Unexpected status: ${res.status}`
        }
    } catch (err: any) {
        const data = err?.response?.data
        if (Array.isArray(data)) {
            errorMsg.value = data.map((e: any) => e.description ?? e.code ?? String(e)).join('\n')
        } else {
            errorMsg.value = data?.title || data?.message || err?.response?.statusText || 'Registration failed'
        }
        console.error(err)
    } finally {
        loading.value = false
    }
}

</script>

<template>
    <div class="form-container">
        <form class="register-form form" @submit.prevent="submitForm" novalidate>
            <h2 class="form-title">Sign up</h2>
            <label class="form-label">
                Email
                <input class="form-input" v-model="email" type="email" required autocomplete="email"/>
            </label>
            <label class="form-label">
                Confirm email
                <input class="form-input" v-model="confirmEmail" type="email" required autocomplete="email"/>
            </label>
            <label class="form-label">
                Password
                <input class="form-input" v-model="password" type="password" required minlength="6" autocomplete="new-password"/>
            </label>
            <label class="form-label">
                Confirm password
                <input class="form-input" v-model="confirmPassword" type="password" required minlength="6" autocomplete="new-password"/>
            </label>
            <button class="form-button" type="submit">Sign in</button>
            <p v-if="errorMsg" class="error">{{ errorMsg }}</p>
            <p>Have an account already ? </p> <a class="form-link" href="/login">Log in</a>
        </form>
    </div>
</template>

<style>


</style>