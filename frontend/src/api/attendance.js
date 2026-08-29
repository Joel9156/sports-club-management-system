import apiClient from './client'

// AttendanceController only has GET and POST right now - no PUT/DELETE, so an
// already-recorded session can't be edited from the frontend yet.

export const getAttendance = ({ date, playerId } = {}) =>
  apiClient.get('/attendance', { params: { date, playerId } }).then((res) => res.data)

export const recordAttendance = (record) =>
  apiClient.post('/attendance', record).then((res) => res.data)
