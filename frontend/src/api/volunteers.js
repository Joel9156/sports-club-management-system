import apiClient from './client'

export const getVolunteers = () => apiClient.get('/volunteers').then((res) => res.data)

export const getVolunteer = (id) => apiClient.get(`/volunteers/${id}`).then((res) => res.data)

export const createVolunteer = (volunteer) =>
  apiClient.post('/volunteers', volunteer).then((res) => res.data)

export const updateVolunteer = (id, volunteer) =>
  apiClient.put(`/volunteers/${id}`, volunteer)

export const deleteVolunteer = (id) => apiClient.delete(`/volunteers/${id}`)
