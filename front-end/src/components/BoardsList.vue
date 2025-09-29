<script setup lang="ts">
import { useBoard } from '../../store/kanban';
import { onMounted } from 'vue';

const boardStore = useBoard()

function addBoard() {

}

async function selectBoard(boardId: string) {
    await boardStore.selectBoard(boardId);
}
</script>

<template>
    <section class="stack">
        <div class="list-item" @click="addBoard"> <p>Add board</p></div>
        <div class="separator"></div>
        <p class="fw-700 list-subtitle">My boards</p>
        <div class="list-item with-bottom-border" v-for="board in boardStore.boards" :key="board.id" @click="selectBoard(board.id)"><p>{{ board.name }}</p></div>
    </section>
</template>

<style scoped>
.stack {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    background: var(--purple-light-light);
    width: 20%;
    border: 0;
    border-right: 2px solid var(--purple-medium);
}

.list-subtitle {
    margin-left: 10px;
}

.list-item {
    color: var(--purple-dark);
    width: 100%;
    height: 30px;
    cursor: pointer;
 }

.list-item:hover {
    background: white;
}

.list-item p {
    margin: 5px 10px 5px 10px;
}

.with-bottom-border {
    border: 0;
    border-bottom: 2px solid var(--purple-light);
}

.separator {
    height:2px;
    width: 100%;
    background: var(--purple-light);
    margin-top: 30px;
}
</style>