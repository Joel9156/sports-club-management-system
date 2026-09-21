import apiClient from './client'

// Reading and marking as read are scoped to the caller's own notifications
// regardless of role. Some are created by the backend (e.g. when attendance is
// recorded); Coaches and Admins can also send one to a group (sendNotification).

export const getNotifications = () => apiClient.get('/notifications').then((res) => res.data)

export const markNotificationRead = (id) => apiClient.post(`/notifications/${id}/read`)

// audience: 'Players' | 'Volunteers' | 'Everyone'
export const sendNotification = (message, audience) =>
  apiClient.post('/notifications/send', { message, audience }).then((res) => res.data)
