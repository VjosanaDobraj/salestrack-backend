import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Trash2, ExternalLink } from 'lucide-react';
import { adminApi } from '../api';
import type { Course } from '../api';

export const AdminCourses = () => {
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ title: '', description: '', thumbnailUrl: '' });
  const [saving, setSaving] = useState(false);

  const load = () => adminApi.getCourses().then(setCourses).catch(console.error).finally(() => setLoading(false));
  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    try {
      await adminApi.createCourse(form);
      setForm({ title: '', description: '', thumbnailUrl: '' });
      setShowCreate(false);
      load();
    } catch { /* handle error */ }
    finally { setSaving(false); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this course and all its lessons?')) return;
    await adminApi.deleteCourse(id);
    setCourses(prev => prev.filter(c => c.id !== id));
  };

  if (loading) return <div className="text-gray-400">Loading courses…</div>;

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Courses</h1>
          <p className="text-gray-500 text-sm mt-1">{courses.length} course{courses.length !== 1 ? 's' : ''}</p>
        </div>
        <button
          onClick={() => setShowCreate(!showCreate)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 transition-colors"
        >
          <Plus size={16} /> New Course
        </button>
      </div>

      {/* Create Form */}
      {showCreate && (
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h3 className="font-semibold text-gray-800 mb-4">Create New Course</h3>
          <form onSubmit={handleCreate} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
              <input
                value={form.title}
                onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="e.g. Cold Calling Techniques"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea
                value={form.description}
                onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows={3}
                placeholder="What will agents learn?"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Thumbnail URL</label>
              <input
                value={form.thumbnailUrl}
                onChange={e => setForm(f => ({ ...f, thumbnailUrl: e.target.value }))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="https://..."
              />
            </div>
            <div className="flex gap-3">
              <button type="submit" disabled={saving} className="px-5 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 disabled:opacity-60">
                {saving ? 'Creating…' : 'Create Course'}
              </button>
              <button type="button" onClick={() => setShowCreate(false)} className="px-5 py-2 border border-gray-300 text-gray-700 text-sm rounded-lg hover:bg-gray-50">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Course Grid */}
      {courses.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <BookOpen size={40} className="mx-auto mb-3 opacity-40" />
          <p>No courses yet. Create your first course.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {courses.map((course) => (
            <div key={course.id} className="bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-md transition-shadow">
              {course.thumbnailUrl ? (
                <img src={course.thumbnailUrl} alt={course.title} className="w-full h-36 object-cover" />
              ) : (
                <div className="h-36 bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center">
                  <BookOpen size={36} className="text-white opacity-60" />
                </div>
              )}
              <div className="p-5">
                <h3 className="font-semibold text-gray-900 mb-1">{course.title}</h3>
                {course.description && <p className="text-sm text-gray-500 line-clamp-2 mb-3">{course.description}</p>}
                <div className="flex items-center justify-between">
                  <span className="text-xs text-gray-400">{course.lessonCount} lesson{course.lessonCount !== 1 ? 's' : ''}</span>
                  <div className="flex gap-2">
                    <Link
                      to={`/admin/courses/${course.id}`}
                      className="flex items-center gap-1 px-3 py-1.5 bg-blue-50 text-blue-700 text-xs font-semibold rounded-lg hover:bg-blue-100 transition-colors"
                    >
                      <ExternalLink size={12} /> Builder
                    </Link>
                    <button
                      onClick={() => handleDelete(course.id)}
                      className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
function BookOpen({ size, className }: { size: number; className?: string }) {
  return (
    <svg className={className} width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
      <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z" /><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z" />
    </svg>
  );
}
