import { ref, computed } from "vue";

// State to track user data
const user = ref(null);

// Initialize user state from localStorage
const loadUser = () => {
  const userData = localStorage.getItem("user");
  if (userData) {
    user.value = JSON.parse(userData);
  }
};

// Computed property to check if user is logged in
const isLoggedIn = computed(() => !!user.value);

// Computed property to check user's role
const isUserAdmin = computed(() => user.value?.role === "Admin");

export function useAuth() {
  loadUser();
  return { isLoggedIn, isUserAdmin, user };
}
