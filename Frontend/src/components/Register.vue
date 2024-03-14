<script setup>
import { ref } from 'vue';
import AuthService from '../services/AuthService';

const username = ref('');
const password = ref('');
const role = ref('user'); // Default role
const successMessage = ref('');
const errorMessage = ref('');

async function handleRegister() {
    try {
        await AuthService.register(username.value, password.value, role.value);
        successMessage.value = 'Registration successful!';
        errorMessage.value = '';
    } catch (error) {
        errorMessage.value = 'Registration failed. ' + error.message;
        successMessage.value = '';
    }
}
</script>

<template>
    <div>
        <h1>Register</h1>
        <div class="d-flex">
            <router-link to="/login">Login</router-link> -
            <router-link to="/register">Register</router-link>
        </div>
        <input v-model="username" placeholder="Username">
        <input v-model="password" type="password" placeholder="Password">
        <select v-model="role">
            <option value="user">User</option>
            <option value="admin">Admin</option>
        </select>
        <button @click="handleRegister">Register</button>
        <p v-if="successMessage">{{ successMessage }}</p>
        <p v-if="errorMessage">{{ errorMessage }}</p>
    </div>
</template>
