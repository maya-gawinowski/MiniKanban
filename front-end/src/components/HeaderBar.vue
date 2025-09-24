<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router';
import { useAuth } from '../../store/auth'

defineOptions({ inheritAttrs: false })  // prevent auto inheritance

const auth = useAuth()
const router = useRouter()

function onLogout() {
    auth.logout()
    router.push('/login')
}
</script>

<!-- HeaderBar.vue -->
<template>
  <header class="header-bar" v-bind="$attrs">
    <nav class="main-nav">
        <RouterLink to="/">Home</RouterLink>
        <RouterLink v-if="!auth.isLoggedIn" class="push-right" to="/login">Login</RouterLink>
        <a v-else class="push-right" @click="onLogout">Logout</a>
    </nav>
    <!-- other header content -->
  </header>
</template>

<style>
.header-bar {
    height: 100%;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: lightgrey;
}
.main-nav {
    display: flex;
    gap: 16px;
    width: 100%;
    align-items: center;
}
a.router-link-active {
    text-decoration: underline;
}
a {
    margin: 1rem;
    color: black;
}
a:hover {
    color: white;
}
.push-right {
    margin-left: auto;
}
</style>