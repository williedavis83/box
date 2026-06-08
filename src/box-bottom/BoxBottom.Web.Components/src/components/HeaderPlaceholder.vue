<template>
  <div class="box-header-placeholder">
    <slot>
      <span class="box-header-placeholder__text">Placeholder</span>
    </slot>
  </div>
</template>

<style scoped>
.box-header-placeholder {
  flex-shrink: 0;
  padding: 0.5rem 0.75rem;
  color: #667085;
  font-size: 0.875rem;
}

.box-header-placeholder__text {
  opacity: 0.7;
}
</style>
