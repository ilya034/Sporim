export function EventCardSkeleton() {
  return (
    <div className="event-card skeleton-card" aria-hidden="true">
      <div className="skeleton skeleton-visual" />
      <div className="event-card-content">
        <div className="skeleton skeleton-label" />
        <div className="skeleton skeleton-heading" />
        <div className="skeleton skeleton-text" />
        <div className="skeleton skeleton-text short" />
        <div className="skeleton skeleton-button" />
      </div>
    </div>
  )
}
