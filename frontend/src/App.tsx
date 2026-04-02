import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClientProvider, QueryClient } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './context/AuthContext';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { AdminDashboard } from './pages/AdminDashboard';
import { AdminCourses } from './pages/AdminCourses';
import { AdminCourseBuilder } from './pages/AdminCourseBuilder';
import { AdminAgents } from './pages/AdminAgents';
import { AdminAgentDetail } from './pages/AdminAgentDetail';
import { AgentDashboard } from './pages/AgentDashboard';
import { AgentCoursePage } from './pages/AgentCoursePage';
import { AgentLessonPage } from './pages/AgentLessonPage';
import './index.css';

const queryClient = new QueryClient();

function AppRoutes() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-gray-400">Loading...</div>
      </div>
    );
  }

  if (!user) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  return (
    <Routes>
      {/* Redirect root based on role */}
      <Route path="/" element={<Navigate to={user.role === 'Admin' ? '/admin' : '/agent'} replace />} />
      <Route path="/login" element={<Navigate to="/" replace />} />

      {/* Admin routes */}
      {user.role === 'Admin' && (
        <>
          <Route path="/admin" element={<Layout><AdminDashboard /></Layout>} />
          <Route path="/admin/courses" element={<Layout><AdminCourses /></Layout>} />
          <Route path="/admin/courses/:id" element={<Layout><AdminCourseBuilder /></Layout>} />
          <Route path="/admin/agents" element={<Layout><AdminAgents /></Layout>} />
          <Route path="/admin/agents/:id" element={<Layout><AdminAgentDetail /></Layout>} />
        </>
      )}

      {/* Agent routes */}
      <Route path="/agent" element={<Layout><AgentDashboard /></Layout>} />
      <Route path="/agent/course/:id" element={<Layout><AgentCoursePage /></Layout>} />
      <Route path="/agent/lesson/:id" element={<Layout><AgentLessonPage /></Layout>} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
