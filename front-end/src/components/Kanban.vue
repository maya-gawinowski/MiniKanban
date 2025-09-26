<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, nextTick } from 'vue'
import draggable from 'vuedraggable'
import { useBoard } from '../../store/kanban'

const boardStore = useBoard()

const isInputVisible = ref(false)
const newBoardName = ref('')

async function addBoard() {
    if (!newBoardName.value.trim()) return
    
    await boardStore.createBoard(newBoardName.value)
    newBoardName.value = ''
    isInputVisible.value = false
}

async function addCard(columnId: string) {
    const y = window.scrollY
    await boardStore.addCard(columnId)
    await nextTick()
    window.scrollTo({ top: y, behavior: 'auto' })
}

function onDragEnd() {
  // TO FIX => post modification to api
  //console.log('New order:', columns.value)
}

// typed handler (takes the event)
const saveScroll: (e: BeforeUnloadEvent) => void = () => {
  localStorage.setItem('scrollY', String(window.scrollY))
}

onMounted(async () => {
   // load data first (layout may change)
  await boardStore.loadAll()

  // restore after data has rendered
  const y = Number(localStorage.getItem('scrollY') || 0)
  if (y) window.scrollTo({ top: y, behavior: 'auto' })

  // add listener
  window.addEventListener('beforeunload', saveScroll)
})

onBeforeUnmount(() => {
  window.removeEventListener('beforeunload', saveScroll)
})
</script>

<template>
    <h2 class="">My mini Kanban</h2>
    <button class="add" @click="isInputVisible = !isInputVisible">Add a board</button>
    <p v-if="isInputVisible">Choose your new board's name</p>
    <input v-if="isInputVisible" type="text" v-model="newBoardName">
    <button v-if="isInputVisible" class="add" @click="addBoard">Create Board</button>
    
    <div v-if="boardStore.loading">Loading boards...</div>
    <p v-else-if="boardStore.error" class="error">{{ boardStore.error }}</p>

    <div v-else v-for="board in boardStore.boards" :key="board.id">
        <h3>{{  board.name  }}</h3>
        <div class="board">
            <div v-for="col in boardStore.columnsOf(board.id)" :key="col.id" class="column">
                <header class="column-header">
                <h3>{{ col.name }}</h3>
                <button class="add" @click="addCard(col.id)">＋</button>
                </header>

                <draggable
                :list="boardStore.cardsOf(col.id)"
                    item-key="id"
                    group="kanban"          
                    @end="onDragEnd"
                    class="card-list"
                    ghost-class="ghost"
                    drag-class="dragging"
                >
                    <template #item="{ element }">
                        <div class="card">
                            <div>{{ element.title }}</div>
                            <div>{{ element.description }}</div>
                        </div>
                    </template>
                    <template #footer>
                        <div v-if="boardStore.cardsOf(col.id).length === 0" class="drop-helper">
                            Drop cards here
                        </div>
                    </template>
                </draggable>
            </div>
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
  opacity: .6;
  text-align: center;
  padding: 12px 0;
  border: 1px dashed #cfd6e0;
  border-radius: 8px;
}

.card {
  background: #fff; border: 1px solid #e3e6eb; border-radius: 8px;
  padding: 10px 12px; box-shadow: 0 1px 2px rgba(0,0,0,.04);
}

.dragging { opacity: .7; }
.ghost { background: #dfeaff; }
.drop-helper {
    opacity: .6;
    text-align: center;
    padding: 12px 0;
    border: 1px dashed #cfd6e0;
    border-radius: 8px;
}

.error {
    color: #e74c3c;
}
</style>
