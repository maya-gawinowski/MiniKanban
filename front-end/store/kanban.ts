import { defineStore } from 'pinia'
import axios from 'axios'

export type Board = {id: string, name: string, ownerId: string}

export const useBoard = defineStore('board', {
    state: () => ({
        boards: [] as Board[],
        loading: false as boolean,
        error: '' as string | null,
    }),
    actions: {
        async loadBoards() {
            this.loading = true,
            this.error = ''
            try {
                const {data} = await axios.get<Board[]>('/api/kanban/boards')
                this.boards = data
            } catch(e: any) {
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to load boards'
                throw e
            } finally {
                this.loading = false
            }
        },
        async createBoard(name: string) {
            this.loading = true,
            this.error = ''
            try {
                const {data} = await axios.post<Board[]>('/api/kanban/boards', {name})
                await this.loadBoards()
            } catch(e: any) {
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to load boards'
                throw e
            } finally {
                this.loading = false
            }
        }
    },
})