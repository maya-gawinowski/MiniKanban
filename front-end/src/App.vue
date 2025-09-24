<script setup lang="ts">
import HeaderBar from './components/HeaderBar.vue';
import FooterBar from './components/FooterBar.vue';
</script>

<template>
  <div class="layout">
    <header-bar class="fixed-header"/>
    <main class="content">
      <RouterView />
    </main>
    <footer-bar class="footer"/>
  </div>
</template>

<style scoped>
.layout {
  min-height: 100vh;
  width: 100vw;
  display: grid;
  grid-template-rows: 50px 1fr 50px;
}
.header, .footer {
  height: 50px;
  width: 100%;
}
.content {
  padding: 1rem;
  overflow: auto;
}
</style>
