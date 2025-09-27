import { defineStore } from 'pinia'
import axios from 'axios'

export type Board = {id: string, name: string, ownerId: string}
export type Column = {id: string, name: string, order: number}
export type Card = {id: string, title: string, order: number}

export const useBoard = defineStore('board', {
    state: () => ({
        boards: [] as Board[],
        columnsByBoard: {} as Record<string, Column[]>,
        cardsByColumn: {} as Record<string, Card[]>,
        loading: false as boolean,
        error: '' as string | null,
        activeBoardId: '' as string | null,
    }),
    getters: {
        columnsOf: (s) => (boardId: string) => s.columnsByBoard[boardId] ?? [],
        cardsOf: (s) => (columnId: string) => s.cardsByColumn[columnId] ?? [],
        activeBoard(state): Board | undefined {
            return state.boards.find(b => b.id === state.activeBoardId)
    }
    },
    actions: {
        async loadBoards() {
            this.loading = true,
            this.error = ''
            try {
                const {data} = await axios.get<Board[]>('/api/kanban/boards')
                this.boards = data
                if (!this.activeBoardId && this.boards.length) {
                    const first = this.boards[0]
                    if (first) this.activeBoardId = first.id
                } 
            } catch(e: any) {
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to load boards'
                throw e
            } finally {
                this.loading = false
            }
        },
        async loadColumns(boardId: string) {
            const {data} = await axios.get<Column[]>(`/api/kanban/boards/${boardId}/columns`)
            this.columnsByBoard[boardId] = [...data].sort((a,b) => a.order - b.order)
        },
        async loadCards(columnId: string) {
            const {data} = await axios.get<Card[]>(`/api/kanban/columns/${columnId}/cards`)
            this.cardsByColumn[columnId] = [...data].sort((a,b) => a.order - b.order)
        },
        async loadAll() {
            this.loading = true
            try {
                await this.loadBoards()
                await Promise.all(this.boards.map(b => this.loadColumns(b.id)))
                const allCols = this.boards.flatMap(b => this.columnsOf(b.id))
                await Promise.all(allCols.map(c => this.loadCards(c.id)))
            } catch (e: any) {
                this.error = e?.response?.data?.message || e?.message || 'Failed to load kanban data'
            } finally {
                this.loading = false
            }
        },
        async createBoard(name: string) {
            this.loading = true;
            this.error = '';
            try {
                // Your API returns a single BoardDto
                const { data } = await axios.post<Board>('/api/kanban/boards', { name });

                // Optimistic add + focus it
                this.boards.unshift(data);
                this.activeBoardId = data.id;

                // Load its default columns created server-side
                await this.loadColumns(data.id);

                // And cards for each (likely empty initially)
                const cols = this.columnsOf(data.id);
                await Promise.all(cols.map(c => this.loadCards(c.id)));

                return data;
            } catch (e: any) {
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to create board';
                throw e;
            } finally {
                this.loading = false;
            }
        },
        async addCard(columnId: string) {
            this.loading = true;
            this.error = '';
            try {
                const { data } = await axios.post<Card>(`/api/kanban/columns/${columnId}/cards`, {
                title: "title",
                description: "description"
                });

                // Ensure array exists, then append (keeps order stable)
                const list = (this.cardsByColumn[columnId] ??= []);
                list.push(data);
            } catch (err: any) {
                this.error = err?.response?.data?.message || err?.response?.statusText || 'Failed to create card';
                throw err;
            } finally {
                this.loading = false;
            }
        },
        async deleteBoard(boardId: string) {
            this.loading = true;
            this.error = '';
            try {
                const {data} = await axios.delete(`/api/kanban/boards/${boardId}`);

                this.boards = this.boards.filter(b => b.id !== boardId)

                delete this.columnsByBoard[boardId];
                for (const c of (this.columnsByBoard[boardId] ?? [])) {
                    delete this.cardsByColumn[c.id]
                }
                if (this.activeBoardId === boardId) {
                    this.activeBoardId = this.boards[0]?.id ?? null
                }
            } catch(e: any) {
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to delete board';
                throw e;
            } finally {
                this.loading = false;
            }
        },
        async deleteCard(cardId: string, columnId: string) {
            this.loading = true;
            this.error = '';

            // optimistic remove from local state
            const list = this.cardsByColumn[columnId] ?? []
            const idx = list.findIndex(c => c.id === cardId)
            const removed = idx !== -1 ? list.splice(idx, 1)[0] : null

            try {
                await axios.delete(`/api/kanban/cards/${cardId}`)
                // success: nothing more to do
            } catch (e: any) {
                // rollback if server failed
                if (removed) list.splice(idx, 0, removed)
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to delete card'
                throw e
            } finally {
                this.loading = false;
            }
        },
        async updateCardTitle(cardId: string, columnId: string, title: string) {
            this.loading = true;
            this.error = '';

            // optimistic update
            const list = this.cardsByColumn[columnId] ?? [];
            const idx = list.findIndex(c => c.id === cardId);
            if (idx < 0) return;

            const current = list[idx]!;
            const prev = { ...current };

            list[idx]!.title = title;

            try {
                await axios.put(`/api/kanban/cards/${cardId}`, { title: title }); // or PATCH if you prefer
                console.log("HEHO")
                return list[idx];
            } catch (e: any) {
                // rollback on failure
                list[idx] = prev;
                this.error = e?.response?.data?.message || e?.response?.statusText || 'Failed to update card title'
                throw e
            } finally {
                this.loading = false;
            }
        }
    },
})