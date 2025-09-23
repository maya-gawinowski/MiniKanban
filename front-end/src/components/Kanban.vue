<script setup lang="ts">
import { onMounted } from 'vue';
import { useAuth } from '../../store/auth';

    // make it not accessible if not connected
    const auth = useAuth()

    onMounted(() => {
        if (!auth.email && auth.isLoggedIn) {
            auth.fetchProfile().catch(console.error)
        }
    })
</script>

<template>
    <h1>Kanban</h1>
    <h2>Welcome {{ auth.email }}</h2>
</template>