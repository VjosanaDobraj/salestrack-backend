import axios from 'axios';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
});

// ── Auth ──
export const authApi = {
  login: (email: string, password: string) => api.post('/api/auth/login', { email, password }).then(r => r.data),
  logout: () => api.post('/api/auth/logout'),
  me: () => api.get('/api/auth/me').then(r => r.data),
};

// ── Admin ──
export const adminApi = {
  dashboard: () => api.get('/api/admin/dashboard').then(r => r.data),
  getCourses: () => api.get('/api/admin/courses').then(r => r.data),
  getCourse: (id: number) => api.get(`/api/admin/courses/${id}`).then(r => r.data),
  createCourse: (data: { title: string; description?: string; thumbnailUrl?: string }) => api.post('/api/admin/courses', data).then(r => r.data),
  updateCourse: (id: number, data: { title: string; description?: string; thumbnailUrl?: string }) => api.put(`/api/admin/courses/${id}`, data),
  deleteCourse: (id: number) => api.delete(`/api/admin/courses/${id}`),
  addLesson: (courseId: number, data: object) => api.post(`/api/admin/courses/${courseId}/lessons`, data).then(r => r.data),
  updateLesson: (id: number, data: object) => api.put(`/api/admin/lessons/${id}`, data),
  deleteLesson: (id: number) => api.delete(`/api/admin/lessons/${id}`),
  reorderLessons: (orderedIds: number[]) => api.post('/api/admin/lessons/reorder', orderedIds),
  addQuizQuestion: (lessonId: number, data: object) => api.post(`/api/admin/lessons/${lessonId}/quiz`, data).then(r => r.data),
  deleteQuizQuestion: (id: number) => api.delete(`/api/admin/quizquestions/${id}`),
  getAgents: () => api.get('/api/admin/agents').then(r => r.data),
  getAgentDetail: (id: string) => api.get(`/api/admin/agents/${id}`).then(r => r.data),
  inviteAgent: (data: { email: string; fullName: string; password: string }) => api.post('/api/admin/agents/invite', data).then(r => r.data),
  getGroups: () => api.get('/api/admin/groups').then(r => r.data),
  createGroup: (data: { name: string; agentIds: string[] }) => api.post('/api/admin/groups', data).then(r => r.data),
  assignCourse: (data: { courseId: number; agentIds: string[]; groupIds: number[] }) => api.post('/api/admin/assignments', data),
};

// ── Agent ──
export const agentApi = {
  getCourses: () => api.get('/api/agent/courses').then(r => r.data),
  getCourse: (id: number) => api.get(`/api/agent/courses/${id}`).then(r => r.data),
  getLesson: (id: number) => api.get(`/api/agent/lessons/${id}`).then(r => r.data),
  markComplete: (lessonId: number) => api.post(`/api/agent/lessons/${lessonId}/complete`),
  submitQuiz: (lessonId: number, answers: Record<number, number>) => api.post(`/api/agent/lessons/${lessonId}/quiz/submit`, { answers }).then(r => r.data),
  postComment: (lessonId: number, body: string) => api.post(`/api/agent/lessons/${lessonId}/comments`, { body }).then(r => r.data),
};

// Types
export interface User { id: string; email: string; fullName: string; role: 'Admin' | 'Agent'; }
export interface Course { id: number; title: string; description?: string; thumbnailUrl?: string; lessonCount: number; createdAt: string; }
export interface Lesson { id: number; title: string; lessonType: string; contentUrl?: string; textContent?: string; sortOrder: number; passingScorePercent: number; quizQuestions: QuizQuestion[]; }
export interface QuizQuestion { id: number; prompt: string; options: { id: number; text: string; isCorrect?: boolean }[]; }
export interface Comment { id: number; body: string; authorName: string; postedAt: string; }
export interface AgentCourse { id: number; title: string; description?: string; thumbnailUrl?: string; progressPercent: number; isCompleted: boolean; lessonCount: number; completedLessons: number; }

