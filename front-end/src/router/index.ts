import { createRouter, createWebHistory } from 'vue-router';

import Welcome from '../components/Welcome.vue';
import Login from '../components/Login.vue';
import Signin from '../components/Signin.vue';
import { useAuth } from '../../store/auth';
import Container from '@/components/Container.vue';

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: '/', component: Welcome},
        { path: '/login', component: Login},
        { path: '/signin', component: Signin},
        { path : "/kanban", component: Container, meta: { requiresAuth: true }},
    ],
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) return savedPosition
        if (to.hash) return { el: to.hash}
        return false
    }
})

router.beforeEach((to) => {
    const auth = useAuth()
    if (to.meta.requiresAuth && !auth.isLoggedIn) {
        return { path: './login', query: { redirect: to.fullPath }}
    }
})