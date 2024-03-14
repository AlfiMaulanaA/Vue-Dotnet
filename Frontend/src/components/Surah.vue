<template>
    <div>
        <h1>Surah List</h1>
        <ul>
            <li v-for="surah in surahs" :key="surah.nomor">
                <h2>{{ surah.nama }} - {{ surah.asma }}</h2>
                <p>{{ surah.arti }}</p>
                <audio :src="surah.audio" controls></audio>
                <p v-html="surah.keterangan"></p>
            </li>
        </ul>
    </div>
</template>

<script>
export default {
    name: 'SurahList',
    data() {
        return {
            surahs: []
        }
    },
    mounted() {
        this.fetchSurahs();
    },
    methods: {
        async fetchSurahs() {
            try {
                const response = await fetch('https://api-alquranid.herokuapp.com/surah');
                if (response.ok) {
                    const data = await response.json();
                    this.surahs = data;
                } else {
                    console.error('Failed to fetch surahs');
                }
            } catch (error) {
                console.error('Error fetching surahs:', error);
            }
        }
    }
}
</script>
