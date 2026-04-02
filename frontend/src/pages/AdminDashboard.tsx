import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarChart3, Users, TrendingUp, BookOpen, MessageSquare } from 'lucide-react';
import { adminApi } from '../api';

interface DashboardData {
  totalCourses: number;
  totalAgents: number;
  completionRate: number;
  avgQuizScore: number;
  totalComments: number;
  agents: Array<{
    id: string;
    name: string;
    email: string;
    completionRate: number;
    avgQuizScore: number;
    assignedCourses: number;
  }>;
}

export const AdminDashboard = () => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.dashboard().then(setData).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="text-gray-400">Loading dashboard…</div>;
  if (!data) return <div className="text-red-500">Failed to load dashboard data.</div>;

  const stats = [
    { label: 'Total Courses', value: data.totalCourses, icon: <BookOpen size={22} />, color: 'bg-blue-50 text-blue-600' },
    { label: 'Active Agents', value: data.totalAgents, icon: <Users size={22} />, color: 'bg-green-50 text-green-600' },
    { label: 'Completion Rate', value: `${data.completionRate}%`, icon: <TrendingUp size={22} />, color: 'bg-purple-50 text-purple-600' },
    { label: 'Avg Quiz Score', value: `${data.avgQuizScore}%`, icon: <BarChart3 size={22} />, color: 'bg-amber-50 text-amber-600' },
    { label: 'Total Comments', value: data.totalComments, icon: <MessageSquare size={22} />, color: 'bg-pink-50 text-pink-600' },
  ];

  return (
    <div className="space-y-8 max-w-6xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-500 mt-1 text-sm">Training program overview</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        {stats.map((s) => (
          <div key={s.label} className="bg-white border border-gray-200 rounded-xl p-5">
            <div className={`inline-flex p-2 rounded-lg ${s.color} mb-3`}>{s.icon}</div>
            <div className="text-2xl font-bold text-gray-900">{s.value}</div>
            <div className="text-xs text-gray-500 mt-1">{s.label}</div>
          </div>
        ))}
      </div>

      {/* Agent Progress Table */}
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 className="font-semibold text-gray-900">Agent Progress</h2>
          <Link to="/admin/agents" className="text-sm text-blue-600 hover:underline">Manage Agents →</Link>
        </div>
        {data.agents.length === 0 ? (
          <div className="p-8 text-center text-gray-400">No agents yet. <Link to="/admin/agents" className="text-blue-600 hover:underline">Add an agent</Link></div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100">
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Agent</th>
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Courses</th>
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Completion</th>
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Avg Quiz</th>
                <th className="px-6 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {data.agents.map((agent) => (
                <tr key={agent.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <div className="font-medium text-gray-900">{agent.name}</div>
                    <div className="text-gray-400">{agent.email}</div>
                  </td>
                  <td className="px-6 py-4 text-gray-600">{agent.assignedCourses}</td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className="w-24 bg-gray-100 rounded-full h-2">
                        <div className="bg-blue-500 h-2 rounded-full" style={{ width: `${agent.completionRate}%` }} />
                      </div>
                      <span className="text-gray-600">{agent.completionRate}%</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`font-semibold ${agent.avgQuizScore >= 80 ? 'text-green-600' : agent.avgQuizScore > 0 ? 'text-amber-600' : 'text-gray-400'}`}>
                      {agent.avgQuizScore > 0 ? `${agent.avgQuizScore}%` : '—'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <Link to={`/admin/agents/${agent.id}`} className="text-blue-600 hover:underline text-xs">View Detail</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

