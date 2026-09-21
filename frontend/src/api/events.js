import apiClient from './client'

// The club schedule (matches and training). Every role can read; only
// Admins can create/update/delete.

export const getEvents = ({ type } = {}) =>
  apiClient.get('/events', { params: { type } }).then((res) => res.data)

export const createEvent = (event) => apiClient.post('/events', event).then((res) => res.data)

export const updateEvent = (id, event) => apiClient.put(`/events/${id}`, event)

export const deleteEvent = (id) => apiClient.delete(`/events/${id}`)
