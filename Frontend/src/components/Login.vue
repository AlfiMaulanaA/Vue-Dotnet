<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import AuthService from '../services/AuthService';

const username = ref('');
const password = ref('');
const router = useRouter();
const errorMessage = ref('');

async function handleLogin() {
  try {
    const response = await AuthService.login(username.value, password.value);
    localStorage.setItem('user', JSON.stringify(response.data));
    router.push('/dashboard'); // Adjust as needed
  } catch (error) {
    errorMessage.value = 'Login failed. Please check your credentials.';
  }
}

const userData = ref([]);

// Function to fetch user data
async function fetchUserData() {
  try {
    const data = await AuthService.getUserData();
    userData.value = data;
  } catch (error) {
    errorMessage.value = "Failed to fetch user data.";
    console.error(error);
  }
}

// Fetch user data when component mounts
onMounted(fetchUserData);
</script>

<template>
  <div>
    <div>
      <h1>Login</h1>
      <div class="d-flex">
        <router-link to="/login">Login</router-link> -
        <router-link to="/register">Register</router-link>
      </div>
      <input v-model="username" placeholder="Username">
      <input v-model="password" type="password" placeholder="Password">
      <button @click="handleLogin">Login</button>
      <p v-if="errorMessage">{{ errorMessage }}</p>
    </div>
    <div>
      <h1>User Data</h1>
      <div v-if="userData">
        <div v-for="user in userData" :key="user.id">
          <p>{{ user.username }}</p>
          <!-- Display other user data as needed -->
        </div>
      </div>
      <p v-if="errorMessage">{{ errorMessage }}</p>
    </div>
  </div>
</template>
