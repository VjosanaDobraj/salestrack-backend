import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { BookOpen } from 'lucide-react';

export const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
      // AuthProvider sets user; we then redirect based on role
      // Navigate to /admin if admin, /agent otherwise — detected in App
      navigate('/');
    } catch {
      setError('Invalid email or password. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center gap-3 mb-4">
            <div className="bg-blue-600 text-white p-3 rounded-xl">
              <BookOpen size={28} />
            </div>
            <h1 className="text-3xl font-bold text-gray-900">SalesTrack</h1>
          </div>
          <p className="text-gray-500">Sales Training Academy</p>
        </div>

        {/* Card */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-200 p-8">
          <h2 className="text-xl font-semibold text-gray-800 mb-6">Sign in to your account</h2>

          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email address</label>
              <input
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                required
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="you@company.com"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                required
                className="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="••••••••"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg text-sm transition-colors disabled:opacity-60 mt-2"
            >
              {loading ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          {/* Demo credentials */}
          <div className="mt-6 pt-5 border-t border-gray-100">
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-3">Demo Credentials</p>
            <div className="space-y-2">
              <button
                type="button"
                onClick={() => { setEmail('admin@salestrack.local'); setPassword('Temp123!'); }}
                className="w-full text-left px-3 py-2 bg-gray-50 hover:bg-gray-100 rounded-lg text-sm transition-colors"
              >
                <span className="font-semibold text-gray-700">Admin</span>
                <span className="text-gray-400 ml-2">admin@salestrack.local / Temp123!</span>
              </button>
              <button
                type="button"
                onClick={() => { setEmail('agent.a@salestrack.local'); setPassword('Temp123!'); }}
                className="w-full text-left px-3 py-2 bg-gray-50 hover:bg-gray-100 rounded-lg text-sm transition-colors"
              >
                <span className="font-semibold text-gray-700">Agent</span>
                <span className="text-gray-400 ml-2">agent.a@salestrack.local / Temp123!</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
