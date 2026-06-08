<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { isTabEnabled } from '../tabs/tabPredicates.js'
import { splitTabsForOverflow } from '../tabs/tabOverflow.js'
import { useTabRegistry } from '../tabs/useTabRegistry.js'
import LinkTab from './LinkTab.vue'
import MenuTab from './MenuTab.vue'

const DEFAULT_MORE_BUTTON_WIDTH = 72
const DEFAULT_TAB_GAP = 4

const { enabledTabs, context } = useTabRegistry()
const router = useRouter()

const containerRef = ref(null)
const moreMeasureRef = ref(null)
const tabRefs = ref({})
const containerWidth = ref(0)
const tabWidths = ref({})
const moreButtonWidth = ref(DEFAULT_MORE_BUTTON_WIDTH)
const tabGap = ref(DEFAULT_TAB_GAP)
const isMoreOpen = ref(false)

const tabIds = computed(() => enabledTabs.value.map((tab) => tab.id))

const overflowSplit = computed(() =>
  splitTabsForOverflow(
    tabIds.value,
    tabWidths.value,
    containerWidth.value,
    moreButtonWidth.value,
    tabGap.value,
  ),
)

const barTabs = computed(() =>
  enabledTabs.value.filter((tab) => overflowSplit.value.visibleIds.includes(tab.id)),
)

const overflowTabs = computed(() =>
  enabledTabs.value.filter((tab) => overflowSplit.value.overflowIds.includes(tab.id)),
)

const showMore = computed(() => overflowTabs.value.length > 0)

function setTabRef(id, element) {
  if (element) {
    tabRefs.value[id] = element
  } else {
    delete tabRefs.value[id]
  }
}

function readTabGap() {
  const tabs = containerRef.value?.querySelector('.box-tab-menu__visible-tabs')
  if (!tabs) {
    return DEFAULT_TAB_GAP
  }

  const styles = getComputedStyle(tabs)
  const gap = parseFloat(styles.columnGap || styles.gap)
  return Number.isFinite(gap) ? gap : DEFAULT_TAB_GAP
}

function updateMoreButtonWidth() {
  moreButtonWidth.value = moreMeasureRef.value?.offsetWidth ?? DEFAULT_MORE_BUTTON_WIDTH
}

function updateContainerWidth() {
  containerWidth.value = containerRef.value?.clientWidth ?? 0
}

async function measureTabs() {
  await nextTick()

  const widths = {}
  for (const tab of enabledTabs.value) {
    const element = tabRefs.value[tab.id]
    widths[tab.id] = element?.offsetWidth ?? 0
  }

  tabWidths.value = widths
  updateMoreButtonWidth()
  tabGap.value = readTabGap()
}

async function remeasure() {
  updateContainerWidth()
  await measureTabs()
}

let resizeObserver
let remeasureQueued = false

function queueRemeasure() {
  if (remeasureQueued) {
    return
  }

  remeasureQueued = true
  requestAnimationFrame(async () => {
    remeasureQueued = false
    await remeasure()

    await nextTick()
    await measureTabs()
  })
}

onMounted(async () => {
  await remeasure()

  if (containerRef.value) {
    resizeObserver = new ResizeObserver(() => {
      queueRemeasure()
    })
    resizeObserver.observe(containerRef.value)
  }
})

onBeforeUnmount(() => {
  resizeObserver?.disconnect()
})

watch(enabledTabs, () => {
  queueRemeasure()
}, { deep: true })

watch(overflowSplit, () => {
  queueRemeasure()
})

function tabDisabled(tab) {
  return !isTabEnabled(tab, context.value)
}

function openOverflowTab(tab) {
  if (tab.type === 'link') {
    router.push(tab.route)
  }

  isMoreOpen.value = false
}

function toggleMore() {
  isMoreOpen.value = !isMoreOpen.value
}
</script>

<template>
  <nav ref="containerRef" class="box-tab-menu" aria-label="Main navigation">
    <div class="box-tab-menu__measure" aria-hidden="true">
      <button
        v-for="tab in enabledTabs"
        :key="`measure-${tab.id}`"
        :ref="(element) => setTabRef(tab.id, element)"
        type="button"
        class="box-tab-menu__measure-tab"
      >
        {{ tab.label }}
      </button>
      <button
        ref="moreMeasureRef"
        type="button"
        class="box-tab-menu__measure-tab"
      >
        More
      </button>
    </div>

    <div class="box-tab-menu__bar">
      <div class="box-tab-menu__visible-tabs">
        <template v-for="tab in barTabs" :key="tab.id">
          <LinkTab
            v-if="tab.type === 'link'"
            :tab="tab"
            :disabled="tabDisabled(tab)"
          />
          <MenuTab
            v-else
            :tab="tab"
            :disabled="tabDisabled(tab)"
          />
        </template>
      </div>

      <div v-if="showMore" class="box-tab-menu__more">
        <button
          type="button"
          class="box-tab-menu__more-button"
          data-testid="tab-menu-more"
          @click="toggleMore"
        >
          More
        </button>
        <ul v-if="isMoreOpen" class="box-tab-menu__more-list">
          <li v-for="tab in overflowTabs" :key="tab.id">
            <button
              type="button"
              class="box-tab-menu__more-item"
              :data-tab-id="tab.id"
              :disabled="tabDisabled(tab)"
              @click="openOverflowTab(tab)"
            >
              {{ tab.label }}
            </button>
          </li>
        </ul>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.box-tab-menu {
  position: relative;
  flex: 1;
  min-width: 0;
}

.box-tab-menu__measure {
  position: absolute;
  visibility: hidden;
  pointer-events: none;
  display: flex;
  white-space: nowrap;
  height: 0;
  overflow: hidden;
}

.box-tab-menu__measure-tab,
.box-tab-menu__more-button,
.box-tab-menu__more-item {
  border: none;
  background: transparent;
  padding: 0.5rem 0.75rem;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  border-bottom: 2px solid transparent;
  flex-shrink: 0;
}

.box-tab-menu__bar {
  display: flex;
  align-items: stretch;
  gap: 0.25rem;
  min-width: 0;
}

.box-tab-menu__visible-tabs {
  display: flex;
  align-items: stretch;
  gap: 0.25rem;
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.box-tab-menu__more {
  position: relative;
  flex-shrink: 0;
  z-index: 1;
}

.box-tab-menu__more-list {
  position: absolute;
  top: calc(100% + 0.25rem);
  right: 0;
  z-index: 30;
  list-style: none;
  margin: 0;
  padding: 0.25rem 0;
  min-width: 10rem;
  border: 1px solid #e4e7ec;
  border-radius: 0.5rem;
  background: #fff;
  box-shadow: 0 8px 24px rgba(16, 24, 40, 0.12);
}

.box-tab-menu__more-item {
  display: block;
  width: 100%;
  text-align: left;
}

.box-tab-menu__more-item:disabled,
.box-tab-menu__more-button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}
</style>
