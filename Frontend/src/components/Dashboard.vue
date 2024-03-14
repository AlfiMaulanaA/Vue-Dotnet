<script setup>
import { useAuth } from '../services/useAuth';
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';

const { isLoggedIn, isUserAdmin } = useAuth();
const router = useRouter();

onMounted(() => {
    if (!isLoggedIn.value) {
        router.push('/login');
    }
});
</script>

<template>
    <div>
        <div v-if="isUserAdmin">
            This is Admin Content
        </div>
        <div>
            This is User Content
        </div>
    </div>

</template>
