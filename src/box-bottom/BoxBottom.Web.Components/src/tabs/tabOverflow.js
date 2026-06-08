/**
 * Splits tab ids into visible bar tabs and overflow (More menu) tabs.
 *
 * @param {string[]} tabIds Ordered tab ids left-to-right.
 * @param {Record<string, number>} tabWidths Measured width per tab id.
 * @param {number} containerWidth Available width for tabs.
 * @param {number} moreButtonWidth Width reserved when More menu is shown.
 * @param {number} [tabGap=0] Flex gap between bar items.
 * @returns {{ visibleIds: string[], overflowIds: string[] }}
 */
export function splitTabsForOverflow(
  tabIds,
  tabWidths,
  containerWidth,
  moreButtonWidth,
  tabGap = 0,
) {
  if (tabIds.length === 0 || containerWidth <= 0) {
    return { visibleIds: [], overflowIds: [] }
  }

  const widthFor = (id) => Math.max(tabWidths[id] ?? 0, 0)

  const remainingWidth = (startIndex) => {
    let total = 0

    for (let index = startIndex; index < tabIds.length; index += 1) {
      if (index > startIndex) {
        total += tabGap
      }

      total += widthFor(tabIds[index])
    }

    return total
  }

  let usedWidth = 0
  const visibleIds = []

  for (let index = 0; index < tabIds.length; index += 1) {
    const id = tabIds[index]
    const width = widthFor(id)
    const gapBefore = visibleIds.length > 0 ? tabGap : 0
    const widthWithGap = gapBefore + width
    const remainingCount = tabIds.length - index - 1

    let fits = false

    if (remainingCount === 0) {
      fits = usedWidth + widthWithGap <= containerWidth
    } else {
      const remainingNeeds = remainingWidth(index + 1)
      const totalIfAllVisible = usedWidth + widthWithGap + tabGap + remainingNeeds

      if (totalIfAllVisible <= containerWidth) {
        fits = usedWidth + widthWithGap <= containerWidth
      } else {
        fits = usedWidth + widthWithGap <= containerWidth - moreButtonWidth - tabGap
      }
    }

    if (!fits) {
      break
    }

    visibleIds.push(id)
    usedWidth += widthWithGap
  }

  if (visibleIds.length === tabIds.length) {
    return { visibleIds, overflowIds: [] }
  }

  if (visibleIds.length === 0) {
    return { visibleIds: [], overflowIds: [...tabIds] }
  }

  return {
    visibleIds,
    overflowIds: tabIds.slice(visibleIds.length),
  }
}
