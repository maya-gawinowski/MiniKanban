<script setup lang="ts">
    import bisou from '../assets/bisou.jpg';
</script>

<template>
    <div class="data">
        <h1>Welcome</h1>
        <h2>To this mini kanban-like app</h2>
        <img :src="bisou" alt="chat" class="chat" width="120" height="120"/>
    </div>
    
</template>

<style>
.chat {
    max-height: 100%;
    height: auto;
    width: 100%;
    display: block;
 }
</style>