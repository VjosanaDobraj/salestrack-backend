import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ChevronLeft, CheckCircle2, Clock } from 'lucide-react';
import { adminApi } from '../api';

interface AgentDetail {
  id: string;
  name: string;
  email: string;
  assignedCourses: number;
  completionRate: number;
  avgQuizScore: number;
  courses: Array<{
    courseId: number;
    title: string;
    progress: number;
    completed: boolean;
    lessonCount: number;
    completedLessons: number;
  }>;
}

export const AdminAgentDetail = () => {
  const { id } = useParams<{ id: string }>();
  const [data, setData] = useState<AgentDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) adminApi.getAgentDetail(id).then(setData).catch(console.error).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="text-gray-400">Loading agent detail…</div>;
  if (!data) return <div className="text-red-500">Agent not found.</div>;

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/admin/agents" className="text-gray-400 hover:text-gray-600"><ChevronLeft size={20} /></Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{data.name}</h1>
          <p className="text-sm text-gray-400">{data.email}</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-5 text-center">
          <div className="text-3xl font-bold text-gray-900">{data.assignedCourses}</div>
          <div className="text-sm text-gray-500 mt-1">Assigned Courses</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5 text-center">
          <div className="text-3xl font-bold text-blue-600">{data.completionRate}%</div>
          <div className="text-sm text-gray-500 mt-1">Completion Rate</div>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5 text-center">
          <div className={`text-3xl font-bold ${data.avgQuizScore >= 80 ? 'text-green-600' : data.avgQuizScore > 0 ? 'text-amber-600' : 'text-gray-400'}`}>
            {data.avgQuizScore > 0 ? `${data.avgQuizScore}%` : '—'}
          </div>
          <div className="text-sm text-gray-500 mt-1">Avg Quiz Score</div>
        </div>
      </div>

      {/* Course Progress */}
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 font-semibold text-gray-800">Course Progress</div>
        {data.courses.length === 0 ? (
          <div className="p-8 text-center text-gray-400">No courses assigned.</div>
        ) : (
          <div className="divide-y divide-gray-50">
            {data.courses.map(course => (
              <div key={course.courseId} className="px-6 py-4">
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    {course.completed
                      ? <CheckCircle2 size={16} className="text-green-500 shrink-0" />
                      : <Clock size={16} className="text-blue-400 shrink-0" />}
                    <span className="font-medium text-gray-900 text-sm">{course.title}</span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-xs text-gray-400">{course.completedLessons}/{course.lessonCount} lessons</span>
                    <span className={`text-sm font-bold ${course.completed ? 'text-green-600' : 'text-gray-700'}`}>{course.progress}%</span>
                  </div>
                </div>
                <div className="w-full bg-gray-100 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full transition-all ${course.completed ? 'bg-green-500' : 'bg-blue-500'}`}
                    style={{ width: `${course.progress}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
