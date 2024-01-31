<template>
  <div>
    <!-- Display error message -->
    <div v-if="errorMessage" class="alert alert-danger">
      {{ errorMessage }}
    </div>

    <!-- User List -->
    <div class="mb-3">
      <!-- call funtion for fetch user -->
      <button class="btn btn-primary" @click="fetchUsers">Refresh User List</button>

      <ul class="list-group mt-3">
        <!-- call data array user -->
        <li class="list-group-item" v-for="user in users" :key="user.id">

          <!-- Condition edit user -->
          <div v-if="editingUserId === user.id">
            <!-- Edit Form -->
            <!-- mapping edited user -->
            <input type="text" v-model="editableUser.username" placeholder="Username">
            <input type="text" v-model="editableUser.phone" placeholder="Phone">

            <!-- Handle save edited function user -->
            <button class="btn btn-success btn-sm" @click="updateUser(user.id)">Save</button>
            <!-- Cancel edited  -->
            <button class="btn btn-secondary btn-sm" @click="cancelEdit">Cancel</button>
          </div>
          <!-- Condition if not edited -->
          <div v-else>
            <!-- Mapping data users -->
            {{ user.username }} - {{ user.phone }}

            <!-- Call condition for edited or not -->
            <button class="btn btn-warning btn-sm float-end" @click="startEdit(user)">Edit</button>

            <!-- call function delete user -->
            <button class="btn btn-danger btn-sm float-end" @click="() => deleteUser(user.id)">Delete</button>
          </div>
        </li>
      </ul>
    </div>

    <!-- Add User Form -->
    <div>
      <!-- Form Input data user -->
      <input type="text" v-model="newUser.username" placeholder="Username" class="form-control" required>
      <input type="password" v-model="newUser.password" placeholder="Password" class="form-control" required>
      <input type="text" v-model="newUser.phone" placeholder="Phone" class="form-control">
      <!-- Function for save users -->
      <button class="btn btn-success" @click="createUser">Add User</button>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import axios from 'axios';

// define users (much user), user (one user)
const users = ref([]);

//define for new user
const newUser = ref({ username: '', password: '', phone: '' });

// define for edit user
const editableUser = ref({});

//define edit user by selected id is null
const editingUserId = ref(null);

// error messege
const errorMessage = ref('');

//Call api url from backend
const apiUrl = 'http://localhost:5258/users';


//Function fetch get data users
const fetchUsers = async () => {
  try {
    const response = await axios.get(apiUrl);
    users.value = response.data; //Condition if success fetch data user
  } catch (error) {
    errorMessage.value = 'Failed to fetch users: ' + error.message; //Condtion if error fetch data user
  }
};

//Function for created user
const createUser = async () => {
  try {
    const response = await axios.post(apiUrl, newUser.value);
    newUser.value = { username: '', password: '', phone: '' };
    fetchUsers();
  } catch (error) {
    errorMessage.value = 'Failed to create user: ' + error.message;
  }
};


//delete user
const deleteUser = async (id) => {
  try {
    await axios.delete(`${apiUrl}/${id}`);
    fetchUsers();
  } catch (error) {
    errorMessage.value = 'Failed to delete user: ' + error.message;
  }
};

//update user
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

//Define for get all data users
onMounted(() => {
  fetchUsers
});
</script>

<style>
/* Add custom styles here */
</style>
