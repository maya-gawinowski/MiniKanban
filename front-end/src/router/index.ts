import { createRouter, createWebHistory } from 'vue-router';

import Welcome from '../components/Welcome.vue';
import Login from '../components/Login.vue';
import Signin from '../components/Signin.vue';
import Kanban from '../components/Kanban.vue';
import { useAuth } from '../../store/auth';

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: '/', component: Welcome},
        { path: '/login', component: Login},
        { path: '/signin', component: Signin},
        { path : "/kanban", component: Kanban, meta: { requiresAuth: true }},
    ]
})

router.beforeEach((to) => {
    const auth = useAuth()
    if (to.meta.requiresAuth && !auth.isLoggedIn) {
        return { path: './login', query: { redirect: to.fullPath }}
    }
})