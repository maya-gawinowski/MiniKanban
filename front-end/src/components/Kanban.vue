<script setup lang="ts">
import { ref } from 'vue'
import draggable from 'vuedraggable'
import { v4 as uuid } from 'uuid'

type Card = { id: string; title: string }
type Column = { id: string; name: string; cards: Card[] }

// TO FIX => retrieve data from api
const columns = ref<Column[]>([
  { id: 'todo', name: 'To Do', cards: [{ id: 'c1', title: 'Set up project' },{ id: 'c2', title: 'Design database' }]},
  { id: 'doing', name: 'Doing', cards: [{ id: 'c3', title: 'Build auth flow' }]},
  { id: 'done', name: 'Done', cards: [{ id: 'c4', title: 'Create repo' }]}
])

function addCard(col: Column) {
    col.cards.push({id: uuid(), title: "new card"})
}

function onDragEnd() {
  // TO FIX => post modification to api
  console.log('New order:', columns.value)
}
</script>

<template>
    <h2 class="">My mini Kanban</h2>
    <div class="board">
        <div v-for="col in columns" :key="col.id" class="column">
            <header class="column-header">
            <h3>{{ col.name }}</h3>
            <button class="add" @click="addCard(col)">＋</button>
            </header>

            <draggable
                v-model="col.cards"
                item-key="id"
                group="kanban"          
                @end="onDragEnd"
                class="card-list"
                ghost-class="ghost"
                drag-class="dragging"
            >
                <template #item="{ element }">
                    <div class="card">
                    {{ element.title }}
                    </div>
                </template>
                <template #footer>
                    <div class="drop-helper">Drop here</div>
                </template>
            </draggable>
        </div>
    </div>
</template>

<style scoped>
.board {
  display: grid;
  grid-template-columns: repeat(3, minmax(260px, 1fr));
  gap: 16px;
  padding: 16px;
  align-items: start;
}

.column {
  background: lightgray;
  border: 1px solid lightgray;
  border-radius: 10px;
  min-height: 220px;
  display: flex;
  flex-direction: column;
}

.column-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px 12px; border-bottom: 1px solid #e3e6eb;
}

.add {
  border: 0; background: grey; border-radius: 6px; padding: 2px 8px; cursor: pointer;
  color: #fff
}

.card-list {
  padding: 10px;
  display: flex; flex-direction: column; gap: 8px;
  min-height: 160px;
}

.card {
  background: #fff; border: 1px solid #e3e6eb; border-radius: 8px;
  padding: 10px 12px; box-shadow: 0 1px 2px rgba(0,0,0,.04);
}

.dragging { opacity: .7; }
.ghost { background: #dfeaff; }
.drop-helper {
  opacity: .5; text-align: center; padding: 8px 0;
}
</style>
