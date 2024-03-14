import { createRouter, createWebHistory } from "vue-router";
import { useAuth } from "../services/useAuth.js";

// Import Views
import Login from "../components/Login.vue";
import Register from "../components/Register.vue";
import Dashboard from "../components/Dashboard.vue";
import Surah from "../components/Surah.vue";

// Routes Configuration
const routes = [
  // Dashboard Routes
  {
    path: "/Login",
    component: Login,
  },
  {
    path: "/Register",
    component: Register,
  },
  {
    path: "/Surah",
    component: Surah,
  },
  {
    path: "/Dashboard",
    component: Dashboard,
    meta: { requiresAuth: true, requiresAdmin: true },
  },
];

// Router Creation
const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const { isLoggedIn, isUserAdmin } = useAuth();

  if (to.meta.requiresAuth && !isLoggedIn.value) {
    next("/login");
  } else if (to.meta.requiresAdmin && !isUserAdmin.value) {
    next("/not-authorized");
  } else {
    next();
  }
});

export default router;
