<script setup>
import { computed } from 'vue'

const props = defineProps({
  branding: {
    type: Object,
    required: true,
  },
})

const showImage = computed(
  () => props.branding.type === 'image' && props.branding.imageUrl,
)
</script>

<template>
  <div class="box-branding">
    <img
      v-if="showImage"
      class="box-branding__image"
      :src="branding.imageUrl"
      :alt="branding.imageAlt ?? ''"
    />
    <h1 v-else class="box-branding__text">{{ branding.text }}</h1>
  </div>
</template>

<style scoped>
.box-branding {
  margin: 0;
}

.box-branding__text {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.box-branding__image {
  display: block;
  height: 2rem;
  width: auto;
}
</style>
