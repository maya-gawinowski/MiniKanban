<script setup lang="ts">
import { onMounted } from 'vue';
import BoardsList from './BoardsList.vue';
import Kanban from './Kanban.vue';
import { useBoard } from '../../store/kanban';
import { useAuth } from '../../store/auth';

const auth = useAuth() 
const boardStore = useBoard();

onMounted(async () => {
  if (auth.isLoggedIn) {
    await boardStore.loadAll()
  }
})
</script>

<template>
    <div class="container">
        <boards-list />
        <Kanban />
    </div>
</template>

<style scoped>
.container {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: row;
    margin: 0;
    padding: 0;
}
</style>