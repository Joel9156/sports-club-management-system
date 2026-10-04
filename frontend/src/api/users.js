import apiClient from './client'

export const getPendingAccounts = () => apiClient.get('/users/pending').then((res) => res.data)

export const approveAccount = (id) => apiClient.post(`/users/${id}/approve`)

export const rejectAccount = (id) => apiClient.post(`/users/${id}/reject`)
