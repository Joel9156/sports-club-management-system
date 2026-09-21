import StatsTables from '../components/StatsTables'

// Read-only stats for every role. Admins enter the underlying numbers on
// /admin/stats.
function StatsPage() {
  return (
    <div className="page">
      <h1>Stats</h1>
      <StatsTables />
    </div>
  )
}

export default StatsPage
