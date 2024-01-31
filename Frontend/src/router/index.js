import { createRouter, createWebHistory } from "vue-router";

// Import Views
import Home from "../Home.vue";

// Routes Configuration
const routes = [
  // Dashboard Routes
  {
    path: "/Dashboard",
    component: Dashboard,
    meta: { middleware: authMiddleware },
  },
  {
    path: "/:pathMatch(.*)*",
    component: () => import("@/views/404.vue"),
  },
];

// Router Creation
const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
