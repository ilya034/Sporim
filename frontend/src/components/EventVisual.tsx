import type { EventCategory } from '../types'
import { categoryLabels } from '../utils/formatters'

interface EventVisualProps {
  category: EventCategory
  compact?: boolean
}

const categorySymbols: Record<EventCategory, string> = {
  esports: 'GG',
  pop_culture: 'VI',
  streamers: 'LIVE',
  macro: '%',
  student_ugc: 'УРФУ',
}

export function EventVisual({ category, compact = false }: EventVisualProps) {
  return (
    <div className={`event-visual event-visual-${category}${compact ? ' compact' : ''}`}>
      <span className="event-visual-category">{categoryLabels[category]}</span>
      <span className="event-visual-symbol">{categorySymbols[category]}</span>
      <span className="visual-orbit visual-orbit-one" aria-hidden="true" />
      <span className="visual-orbit visual-orbit-two" aria-hidden="true" />
    </div>
  )
}
