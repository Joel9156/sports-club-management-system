import apiClient from './client'

// Notifications are read-only from the client's perspective - they're
// created internally by the backend (e.g. when attendance is recorded), not
// submitted via POST here. Every endpoint is scoped to the caller's own
// notifications regardless of role.

export const getNotifications = () => apiClient.get('/notifications').then((res) => res.data)

export const markNotificationRead = (id) => apiClient.post(`/notifications/${id}/read`)
