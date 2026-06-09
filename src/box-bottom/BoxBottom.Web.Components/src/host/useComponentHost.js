import { inject } from 'vue'

export const COMPONENT_HOST_KEY = Symbol('componentHost')

export function useComponentHost() {
  const componentHost = inject(COMPONENT_HOST_KEY)

  if (!componentHost) {
    throw new Error('useComponentHost must be used within a component host provider')
  }

  return componentHost
}
