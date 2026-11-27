import type { EventCategory, EventFilters } from '../types'
import { categoryLabels } from '../utils/formatters'
import { Icon } from './Icon'

interface FilterBarProps {
  filters: EventFilters
  onChange: (filters: EventFilters) => void
}

const categories: Array<EventCategory | 'all'> = [
  'all',
  'esports',
  'pop_culture',
  'streamers',
  'macro',
  'student_ugc',
]

export function FilterBar({ filters, onChange }: FilterBarProps) {
  return (
    <section className="filters" aria-label="Фильтры событий">
      <label className="search-field">
        <Icon name="search" size={19} />
        <span className="visually-hidden">Поиск событий</span>
        <input
          type="search"
          value={filters.search}
          placeholder="Найти событие"
          onChange={(event) =>
            onChange({ ...filters, search: event.target.value })
          }
        />
      </label>

      <div className="filter-row">
        <div className="category-list" aria-label="Категория">
          {categories.map((category) => (
            <button
              className={`filter-chip${filters.category === category ? ' active' : ''}`}
              type="button"
              key={category}
              aria-pressed={filters.category === category}
              onClick={() => onChange({ ...filters, category })}
            >
              {category === 'all' ? 'Все темы' : categoryLabels[category]}
            </button>
          ))}
        </div>

        <label className="switch-label">
          <input
            type="checkbox"
            checked={filters.onlyActive}
            onChange={(event) =>
              onChange({ ...filters, onlyActive: event.target.checked })
            }
          />
          <span className="switch" aria-hidden="true" />
          Только активные
        </label>
      </div>
    </section>
  )
}
