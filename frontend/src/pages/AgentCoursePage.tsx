import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ChevronLeft, CheckCircle2 } from 'lucide-react';
import { agentApi } from '../api';

interface CourseDetail {
  id: number;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  lessons: Array<{ id: number; title: string; lessonType: string; sortOrder: number; isCompleted: boolean }>;
}

export const AgentCoursePage = () => {
  const { id } = useParams<{ id: string }>();
  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) agentApi.getCourse(Number(id)).then(setCourse).catch(console.error).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="text-gray-400">Loading course...</div>;
  if (!course) return <div className="text-red-500">Course not found or not assigned to you.</div>;

  const typeIcon: Record<string, string> = { Video: '🎬', Audio: '🎵', PDF: '📄', Text: '📝' };
  const completedCount = course.lessons.filter(l => l.isCompleted).length;
  const progress = course.lessons.length === 0 ? 0 : Math.round(completedCount / course.lessons.length * 100);

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/agent" className="text-gray-400 hover:text-gray-600"><ChevronLeft size={20} /></Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{course.title}</h1>
          {course.description && <p className="text-sm text-gray-500 mt-1">{course.description}</p>}
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <div className="flex justify-between text-sm mb-2">
          <span className="text-gray-500">{completedCount} of {course.lessons.length} lessons completed</span>
          <span className="font-semibold text-gray-900">{progress}%</span>
        </div>
        <div className="w-full bg-gray-100 rounded-full h-2.5">
          <div className="bg-blue-500 h-2.5 rounded-full" style={{ width: `${progress}%` }} />
        </div>
      </div>

      <div className="space-y-2">
        {course.lessons.map((lesson, idx) => (
          <Link
            key={lesson.id}
            to={`/agent/lesson/${lesson.id}`}
            className="flex items-center gap-4 bg-white border border-gray-200 rounded-xl p-4 hover:shadow-sm hover:border-blue-200 transition-all group"
          >
            <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-sm font-semibold text-gray-500 shrink-0 group-hover:bg-blue-50 group-hover:text-blue-700">
              {idx + 1}
            </div>
            <div className="text-xl">{typeIcon[lesson.lessonType] || '📖'}</div>
            <div className="flex-1">
              <div className="font-medium text-gray-900 text-sm">{lesson.title}</div>
              <div className="text-xs text-gray-400">{lesson.lessonType}</div>
            </div>
            {lesson.isCompleted && <CheckCircle2 size={18} className="text-green-500 shrink-0" />}
            <ChevronLeft size={16} className="text-gray-300 rotate-180 group-hover:text-blue-400" />
          </Link>
        ))}
      </div>
    </div>
  );
};
