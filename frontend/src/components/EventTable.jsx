// Result of a played match from the club's point of view, or an em dash if
// there's no score yet (or it's a training session).
function resultLabel(event) {
  if (event.goalsFor == null || event.goalsAgainst == null) return '—'
  const score = `${event.goalsFor}-${event.goalsAgainst}`
  if (event.goalsFor > event.goalsAgainst) return `W ${score}`
  if (event.goalsFor < event.goalsAgainst) return `L ${score}`
  return `D ${score}`
}

// Read-only table of schedule entries, used by the schedule page. `renderActions`
// lets the admin page add Edit/Delete buttons without duplicating the table.
function EventTable({ events, emptyText, renderActions }) {
  return (
    <table>
      <thead>
        <tr>
          <th>Date</th>
          <th>Type</th>
          <th>Opponent</th>
          <th>Location</th>
          <th>Result</th>
          {renderActions && <th></th>}
        </tr>
      </thead>
      <tbody>
        {events.map((e) => (
          <tr key={e.id}>
            <td>{e.date}</td>
            <td>{e.type}</td>
            <td>{e.opponent ?? '—'}</td>
            <td>{e.location}</td>
            <td>{e.type === 'Match' ? resultLabel(e) : '—'}</td>
            {renderActions && <td>{renderActions(e)}</td>}
          </tr>
        ))}
        {events.length === 0 && (
          <tr>
            <td colSpan={renderActions ? 6 : 5}>{emptyText}</td>
          </tr>
        )}
      </tbody>
    </table>
  )
}

export default EventTable
