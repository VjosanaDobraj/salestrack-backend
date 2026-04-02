import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Plus, Trash2, GripVertical, ChevronLeft } from 'lucide-react';
import { adminApi } from '../api';

interface QuizQuestion { id: number; prompt: string; options: { id: number; text: string; isCorrect: boolean }[]; }
interface Lesson {
  id: number; title: string; lessonType: string; contentUrl?: string;
  textContent?: string; sortOrder: number; passingScorePercent: number; quizQuestions: QuizQuestion[];
}
interface CourseDetail {
  id: number; title: string; description?: string; thumbnailUrl?: string; lessons: Lesson[];
}

const LESSON_TYPES = ['Video', 'Audio', 'PDF', 'Text'];

export const AdminCourseBuilder = () => {
  const { id } = useParams<{ id: string }>();
  const courseId = Number(id);
  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [showAddLesson, setShowAddLesson] = useState(false);
  const [lessonForm, setLessonForm] = useState({ title: '', lessonType: 'Video', contentUrl: '', textContent: '', passingScorePercent: 80 });
  const [quizForms, setQuizForms] = useState<Record<number, { prompt: string; a: string; b: string; c: string; d: string; correct: number }>>({});
  const [agents, setAgents] = useState<{ id: string; email: string; fullName: string }[]>([]);
  const [groups, setGroups] = useState<{ id: number; name: string }[]>([]);
  const [assignForm, setAssignForm] = useState<{ agentIds: string[]; groupIds: number[] }>({ agentIds: [], groupIds: [] });
  const [saving, setSaving] = useState(false);

  const load = () => {
    Promise.all([
      adminApi.getCourse(courseId),
      adminApi.getAgents(),
      adminApi.getGroups()
    ]).then(([c, a, g]) => { setCourse(c); setAgents(a); setGroups(g); }).finally(() => setLoading(false));
  };
  useEffect(() => { load(); }, [courseId]);

  const handleAddLesson = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await adminApi.addLesson(courseId, lessonForm);
      setLessonForm({ title: '', lessonType: 'Video', contentUrl: '', textContent: '', passingScorePercent: 80 });
      setShowAddLesson(false);
      load();
    } finally { setSaving(false); }
  };

  const handleDeleteLesson = async (lessonId: number) => {
    if (!confirm('Delete this lesson?')) return;
    await adminApi.deleteLesson(lessonId);
    load();
  };

  const handleAddQuizQuestion = async (lessonId: number) => {
    const qf = quizForms[lessonId];
    if (!qf || !qf.prompt.trim()) return;
    await adminApi.addQuizQuestion(lessonId, {
      prompt: qf.prompt, optionA: qf.a, optionB: qf.b, optionC: qf.c, optionD: qf.d, correctOption: qf.correct
    });
    setQuizForms(prev => { const n = { ...prev }; delete n[lessonId]; return n; });
    load();
  };

  const handleDeleteQuestion = async (questionId: number) => {
    if (!confirm('Delete this question?')) return;
    await adminApi.deleteQuizQuestion(questionId);
    load();
  };

  const handleAssign = async (e: React.FormEvent) => {
    e.preventDefault();
    await adminApi.assignCourse({ courseId, agentIds: assignForm.agentIds, groupIds: assignForm.groupIds });
    alert('Course assigned successfully!');
  };

  const handleMoveLesson = async (index: number, direction: 'up' | 'down') => {
    if (!course) return;
    const lessons = [...course.lessons];
    const swapIdx = direction === 'up' ? index - 1 : index + 1;
    if (swapIdx < 0 || swapIdx >= lessons.length) return;
    [lessons[index], lessons[swapIdx]] = [lessons[swapIdx], lessons[index]];
    await adminApi.reorderLessons(lessons.map(l => l.id));
    load();
  };

  if (loading) return <div className="text-gray-400">Loading course builder…</div>;
  if (!course) return <div className="text-red-500">Course not found.</div>;

  return (
    <div className="max-w-6xl space-y-6">
      <div className="flex items-center gap-3">
        <Link to="/admin/courses" className="text-gray-400 hover:text-gray-600">
          <ChevronLeft size={20} />
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{course.title}</h1>
          <p className="text-sm text-gray-400">Course Builder — {course.lessons.length} lessons</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Lessons Panel */}
        <div className="lg:col-span-2 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">Lessons</h2>
            <button
              onClick={() => setShowAddLesson(!showAddLesson)}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700"
            >
              <Plus size={14} /> Add Lesson
            </button>
          </div>

          {/* Add Lesson Form */}
          {showAddLesson && (
            <div className="bg-white border border-blue-200 rounded-xl p-5">
              <h3 className="font-semibold text-gray-800 mb-4">Add Lesson</h3>
              <form onSubmit={handleAddLesson} className="space-y-3">
                <div>
                  <label className="text-xs font-medium text-gray-600 block mb-1">Title *</label>
                  <input value={lessonForm.title} onChange={e => setLessonForm(f => ({ ...f, title: e.target.value }))} required className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Lesson title" />
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="text-xs font-medium text-gray-600 block mb-1">Type</label>
                    <select value={lessonForm.lessonType} onChange={e => setLessonForm(f => ({ ...f, lessonType: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                      {LESSON_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                    </select>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-600 block mb-1">Passing Score (%)</label>
                    <input type="number" min={0} max={100} value={lessonForm.passingScorePercent} onChange={e => setLessonForm(f => ({ ...f, passingScorePercent: Number(e.target.value) }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                  </div>
                </div>
                {lessonForm.lessonType !== 'Text' ? (
                  <div>
                    <label className="text-xs font-medium text-gray-600 block mb-1">Content URL (MP4/MP3/PDF link or YouTube)</label>
                    <input value={lessonForm.contentUrl} onChange={e => setLessonForm(f => ({ ...f, contentUrl: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="https://..." />
                  </div>
                ) : (
                  <div>
                    <label className="text-xs font-medium text-gray-600 block mb-1">Text Content</label>
                    <textarea value={lessonForm.textContent} onChange={e => setLessonForm(f => ({ ...f, textContent: e.target.value }))} rows={5} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Lesson content…" />
                  </div>
                )}
                <div className="flex gap-2">
                  <button type="submit" disabled={saving} className="px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 disabled:opacity-60">{saving ? 'Adding…' : 'Add Lesson'}</button>
                  <button type="button" onClick={() => setShowAddLesson(false)} className="px-4 py-2 border border-gray-300 text-sm text-gray-600 rounded-lg hover:bg-gray-50">Cancel</button>
                </div>
              </form>
            </div>
          )}

          {/* Lessons List */}
          {course.lessons.length === 0 ? (
            <div className="text-center py-12 text-gray-400 bg-white border border-dashed border-gray-200 rounded-xl">
              No lessons yet. Add your first lesson.
            </div>
          ) : (
            <div className="space-y-3">
              {course.lessons.map((lesson, idx) => (
                <div key={lesson.id} className="bg-white border border-gray-200 rounded-xl overflow-hidden">
                  {/* Lesson Header */}
                  <div className="flex items-center gap-3 px-4 py-3 border-b border-gray-100">
                    <GripVertical size={16} className="text-gray-300 cursor-move" />
                    <div className="flex-1">
                      <span className="font-medium text-gray-900 text-sm">{lesson.title}</span>
                      <span className="ml-2 px-2 py-0.5 bg-gray-100 text-gray-500 text-xs rounded-full">{lesson.lessonType}</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button disabled={idx === 0} onClick={() => handleMoveLesson(idx, 'up')} className="p-1 hover:bg-gray-100 rounded text-gray-400 disabled:opacity-30 text-xs">▲</button>
                      <button disabled={idx === course.lessons.length - 1} onClick={() => handleMoveLesson(idx, 'down')} className="p-1 hover:bg-gray-100 rounded text-gray-400 disabled:opacity-30 text-xs">▼</button>
                      <button onClick={() => handleDeleteLesson(lesson.id)} className="p-1 hover:bg-red-50 text-gray-400 hover:text-red-500 rounded ml-1">
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </div>

                  {/* Quiz Section */}
                  <div className="p-4 bg-gray-50">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Quiz ({lesson.quizQuestions.length} questions) — Passing: {lesson.passingScorePercent}%</span>
                      <button onClick={() => setQuizForms(f => ({ ...f, [lesson.id]: { prompt: '', a: '', b: '', c: '', d: '', correct: 1 } }))} className="text-xs text-blue-600 hover:underline">+ Add Question</button>
                    </div>

                    {lesson.quizQuestions.map(q => (
                      <div key={q.id} className="flex items-start justify-between mb-2 bg-white border border-gray-200 rounded-lg px-3 py-2">
                        <div className="text-xs text-gray-700">
                          <span className="font-medium">{q.prompt}</span>
                          <div className="mt-1 text-gray-400">{q.options.map((o, i) => <span key={i} className={o.isCorrect ? 'text-green-600 font-semibold mr-2' : 'mr-2'}>{o.isCorrect ? '✓' : ''}{o.text}</span>)}</div>
                        </div>
                        <button onClick={() => handleDeleteQuestion(q.id)} className="ml-2 text-gray-300 hover:text-red-500"><Trash2 size={12} /></button>
                      </div>
                    ))}

                    {quizForms[lesson.id] !== undefined && (
                      <div className="bg-white border border-blue-200 rounded-lg p-3 mt-2 space-y-2">
                        <input placeholder="Question prompt" value={quizForms[lesson.id].prompt} onChange={e => setQuizForms(f => ({ ...f, [lesson.id]: { ...f[lesson.id], prompt: e.target.value } }))} className="w-full px-2 py-1.5 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-blue-400" />
                        <div className="grid grid-cols-2 gap-2">
                          {(['a', 'b', 'c', 'd'] as const).map((opt, i) => (
                            <div key={opt} className="flex items-center gap-1">
                              <input type="radio" name={`correct-${lesson.id}`} value={i + 1} checked={quizForms[lesson.id].correct === i + 1} onChange={() => setQuizForms(f => ({ ...f, [lesson.id]: { ...f[lesson.id], correct: i + 1 } }))} />
                              <input placeholder={`Option ${opt.toUpperCase()}`} value={quizForms[lesson.id][opt]} onChange={e => setQuizForms(f => ({ ...f, [lesson.id]: { ...f[lesson.id], [opt]: e.target.value } }))} className="flex-1 px-2 py-1 border border-gray-200 rounded text-xs focus:outline-none focus:ring-1 focus:ring-blue-400" />
                            </div>
                          ))}
                        </div>
                        <div className="flex gap-2">
                          <button onClick={() => handleAddQuizQuestion(lesson.id)} className="px-3 py-1 bg-blue-600 text-white text-xs font-semibold rounded hover:bg-blue-700">Save Question</button>
                          <button onClick={() => setQuizForms(f => { const n = { ...f }; delete n[lesson.id]; return n; })} className="px-3 py-1 border border-gray-200 text-xs text-gray-500 rounded hover:bg-gray-50">Cancel</button>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Assignment Panel */}
        <div className="space-y-4">
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <h2 className="font-semibold text-gray-800 mb-4">Assign Course</h2>
            <form onSubmit={handleAssign} className="space-y-4">
              <div>
                <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-2">Agents</label>
                <div className="space-y-1 max-h-36 overflow-y-auto">
                  {agents.map(agent => (
                    <label key={agent.id} className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer hover:bg-gray-50 px-2 py-1 rounded">
                      <input
                        type="checkbox"
                        checked={assignForm.agentIds.includes(agent.id)}
                        onChange={e => setAssignForm(f => ({
                          ...f,
                          agentIds: e.target.checked ? [...f.agentIds, agent.id] : f.agentIds.filter(i => i !== agent.id)
                        }))}
                        className="rounded"
                      />
                      <span>{agent.fullName || agent.email}</span>
                    </label>
                  ))}
                  {agents.length === 0 && <p className="text-xs text-gray-400">No agents yet.</p>}
                </div>
              </div>
              <div>
                <label className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-2">Groups</label>
                <div className="space-y-1">
                  {groups.map(group => (
                    <label key={group.id} className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer hover:bg-gray-50 px-2 py-1 rounded">
                      <input
                        type="checkbox"
                        checked={assignForm.groupIds.includes(group.id)}
                        onChange={e => setAssignForm(f => ({
                          ...f,
                          groupIds: e.target.checked ? [...f.groupIds, group.id] : f.groupIds.filter(i => i !== group.id)
                        }))}
                        className="rounded"
                      />
                      <span>{group.name}</span>
                    </label>
                  ))}
                  {groups.length === 0 && <p className="text-xs text-gray-400">No groups yet.</p>}
                </div>
              </div>
              <button type="submit" className="w-full py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700">
                Assign Course
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};
