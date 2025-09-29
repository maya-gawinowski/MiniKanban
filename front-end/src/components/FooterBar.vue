<template>
  <footer class="footer-bar">
    <small>© {{ new Date().getFullYear() }}</small>
  </footer>
</template>

<style>
.footer-bar {
    height: 100%;
    width: 100%;
    align-items: center;
    background: var(--purple-dark);
}
small {
    color: var(--purple-light);
}
</style>