import apiClient from './client'

// TeamsController only has GET and POST right now - no PUT/DELETE yet.

export const getTeams = () => apiClient.get('/teams').then((res) => res.data)

export const getTeam = (id) => apiClient.get(`/teams/${id}`).then((res) => res.data)

export const createTeam = (team) => apiClient.post('/teams', team).then((res) => res.data)
