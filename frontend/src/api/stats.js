import apiClient from './client'

// Totals and the team record are computed by the backend; every role can read them.
export const getPlayerTotals = () => apiClient.get('/stats/players').then((res) => res.data)

export const getTeamStats = () => apiClient.get('/stats/team').then((res) => res.data)

// Per-match rows (goals/assists). A row existing means the player played in
// that match. Only Admins can create/update/delete them.
export const getPlayerStats = ({ playerId, eventId } = {}) =>
  apiClient.get('/playerstats', { params: { playerId, eventId } }).then((res) => res.data)

export const createPlayerStat = (stat) =>
  apiClient.post('/playerstats', stat).then((res) => res.data)

export const updatePlayerStat = (id, stat) => apiClient.put(`/playerstats/${id}`, stat)

export const deletePlayerStat = (id) => apiClient.delete(`/playerstats/${id}`)
