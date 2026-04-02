import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Play, CheckCircle2, Clock } from 'lucide-react';
import { agentApi } from '../api';
import type { AgentCourse } from '../api';

export const AgentDashboard = () => {
  const [courses, setCourses] = useState<AgentCourse[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    agentApi.getCourses().then(setCourses).catch(console.error).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="text-gray-400">Loading your courses...</div>;

  const completed = courses.filter(c => c.isCompleted).length;
  const inProgress = courses.filter(c => !c.isCompleted && c.completedLessons > 0).length;
  const overall = courses.length === 0 ? 0 : Math.round(courses.reduce((s, c) => s + c.progressPercent, 0) / courses.length);

  return (
    <div className="space-y-8 max-w-5xl">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">My Learning</h1>
        <p className="text-gray-500 text-sm mt-1">Track your progress and continue training.</p>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <p className="text-sm text-gray-500">In Progress</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{inProgress}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <p className="text-sm text-gray-500">Completed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{completed}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <p className="text-sm text-gray-500">Overall Progress</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{overall}%</p>
        </div>
      </div>

      {courses.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <p>No courses assigned yet. Ask your admin to assign courses.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {courses.map((course) => (
            <div key={course.id} className="bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-md transition-shadow flex flex-col">
              {course.thumbnailUrl ? (
                <img src={course.thumbnailUrl} alt={course.title} className="w-full h-36 object-cover" />
              ) : (
                <div className="h-36 bg-gradient-to-br from-blue-400 to-indigo-600" />
              )}
              <div className="p-5 flex flex-col flex-1">
                <div className="flex items-start justify-between gap-2 mb-2">
                  <h3 className="font-semibold text-gray-900 text-sm">{course.title}</h3>
                  {course.isCompleted ? <CheckCircle2 size={18} className="text-green-500 shrink-0" /> : course.completedLessons > 0 ? <Clock size={18} className="text-blue-400 shrink-0" /> : null}
                </div>
                {course.description && <p className="text-xs text-gray-500 flex-1 mb-3 line-clamp-2">{course.description}</p>}
                <div className="mb-4">
                  <div className="flex justify-between text-xs text-gray-400 mb-1.5">
                    <span>{course.completedLessons}/{course.lessonCount} lessons</span>
                    <span>{course.progressPercent}%</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-2">
                    <div className="bg-blue-500 h-2 rounded-full" style={{ width: course.progressPercent + '%' }} />
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className={'text-xs font-medium px-2.5 py-1 rounded-full ' + (course.isCompleted ? 'bg-green-100 text-green-700' : course.completedLessons > 0 ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500')}>
                    {course.isCompleted ? 'Completed' : course.completedLessons > 0 ? 'In Progress' : 'Not Started'}
                  </span>
                  <Link to={'/agent/course/' + course.id} className="inline-flex items-center gap-1.5 px-4 py-2 bg-blue-600 text-white text-xs font-semibold rounded-lg hover:bg-blue-700">
                    <Play size={12} />
                    {course.progressPercent > 0 ? 'Continue' : 'Start'}
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
