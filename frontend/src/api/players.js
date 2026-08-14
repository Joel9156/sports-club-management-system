import apiClient from './client'

export const getPlayers = () => apiClient.get('/players').then((res) => res.data)

export const getPlayer = (id) => apiClient.get(`/players/${id}`).then((res) => res.data)

export const createPlayer = (player) =>
  apiClient.post('/players', player).then((res) => res.data)

export const updatePlayer = (id, player) => apiClient.put(`/players/${id}`, player)

export const deletePlayer = (id) => apiClient.delete(`/players/${id}`)
