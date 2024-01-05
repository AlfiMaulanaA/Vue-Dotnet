<template>
  <div>
    <!-- Display error message -->
    <div v-if="errorMessage" class="alert alert-danger">
      {{ errorMessage }}
    </div>

    <!-- User List -->
    <div class="mb-3">
      <button class="btn btn-primary" @click="fetchUsers">Refresh User List</button>
      <ul class="list-group mt-3">
        <li class="list-group-item" v-for="user in users" :key="user.id">
          <div v-if="editingUserId === user.id">
            <!-- Edit Form -->
            <input type="text" v-model="editableUser.username" placeholder="Username">
            <input type="text" v-model="editableUser.phone" placeholder="Phone">
            <button class="btn btn-success btn-sm" @click="updateUser(user.id)">Save</button>
            <button class="btn btn-secondary btn-sm" @click="cancelEdit">Cancel</button>
          </div>
          <div v-else>
            {{ user.username }} - {{ user.phone }}
            <button class="btn btn-warning btn-sm float-end" @click="startEdit(user)">Edit</button>
            <button class="btn btn-danger btn-sm float-end" @click="() => deleteUser(user.id)">Delete</button>
          </div>
        </li>
      </ul>
    </div>

    <!-- Add User Form -->
    <div>
      <input type="text" v-model="newUser.username" placeholder="Username">
      <input type="password" v-model="newUser.password" placeholder="Password">
      <input type="text" v-model="newUser.phone" placeholder="Phone">
      <button class="btn btn-success" @click="createUser">Add User</button>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import axios from 'axios';

const users = ref([]);
const newUser = ref({ username: '', password: '', phone: '' });
const editableUser = ref({});
const editingUserId = ref(null);
const errorMessage = ref('');
const apiUrl = 'http://localhost:5258/users';

const fetchUsers = async () => {
  try {
    const response = await axios.get(apiUrl);
    users.value = response.data;
  } catch (error) {
    errorMessage.value = 'Failed to fetch users: ' + error.message;
  }
};

const createUser = async () => {
  try {
    const response = await axios.post(apiUrl, newUser.value);
    newUser.value = { username: '', password: '', phone: '' };
    fetchUsers();
  } catch (error) {
    errorMessage.value = 'Failed to create user: ' + error.message;
  }
};

const deleteUser = async (id) => {
  try {
    await axios.delete(`${apiUrl}/${id}`);
    fetchUsers();
  } catch (error) {
    errorMessage.value = 'Failed to delete user: ' + error.message;
  }
};

const updateUser = async (id) => {
  try {
    await axios.put(`${apiUrl}/${id}`, editableUser.value);
    cancelEdit();
    fetchUsers();
  } catch (error) {
    errorMessage.value = 'Failed to update user: ' + error.message;
  }
};


const startEdit = (user) => {
  editingUserId.value = user.id;
  editableUser.value = { ...user };
};

const cancelEdit = () => {
  editingUserId.value = null;
  editableUser.value = {};
};

onMounted(fetchUsers);
</script>

<style>
/* Add custom styles here */
</style>
