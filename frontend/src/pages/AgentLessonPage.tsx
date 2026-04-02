import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ChevronLeft, CheckCircle2, Send } from 'lucide-react';
import { agentApi } from '../api';
import { useAuth } from '../context/AuthContext';

interface QuizQuestion {
  id: number;
  prompt: string;
  options: { id: number; text: string }[];
}

interface Comment {
  id: number;
  body: string;
  authorName: string;
  postedAt: string;
}

interface LessonData {
  id: number;
  courseId: number;
  title: string;
  lessonType: string;
  contentUrl?: string;
  textContent?: string;
  passingScorePercent: number;
  isCompleted: boolean;
  lastAttempt: { scorePercent: number; passed: boolean; attemptedAtUtc: string } | null;
  quizQuestions: QuizQuestion[];
  comments: Comment[];
}

type QuizResult = { scorePercent: number; correct: number; total: number; passed: boolean; passingScore: number };

export const AgentLessonPage = () => {
  const { id } = useParams<{ id: string }>();
  const [lesson, setLesson] = useState<LessonData | null>(null);
  const [loading, setLoading] = useState(true);
  const [marking, setMarking] = useState(false);
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [quizResult, setQuizResult] = useState<QuizResult | null>(null);
  const [submittingQuiz, setSubmittingQuiz] = useState(false);
  const [comment, setComment] = useState('');
  const [postingComment, setPostingComment] = useState(false);
  useAuth();

  const load = () => {
    if (id) {
      agentApi.getLesson(Number(id))
        .then(setLesson)
        .catch(console.error)
        .finally(() => setLoading(false));
    }
  };

  useEffect(() => { load(); }, [id]);

  const handleMarkComplete = async () => {
    if (!lesson) return;
    setMarking(true);
    try {
      await agentApi.markComplete(lesson.id);
      setLesson(l => l ? { ...l, isCompleted: true } : l);
    } finally { setMarking(false); }
  };

  const handleSubmitQuiz = async () => {
    if (!lesson) return;
    setSubmittingQuiz(true);
    try {
      const result = await agentApi.submitQuiz(lesson.id, answers);
      setQuizResult(result);
      if (result.passed) setLesson(l => l ? { ...l, isCompleted: true } : l);
    } finally { setSubmittingQuiz(false); }
  };

  const handlePostComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!lesson || !comment.trim()) return;
    setPostingComment(true);
    try {
      const newComment = await agentApi.postComment(lesson.id, comment);
      setLesson(l => l ? { ...l, comments: [...l.comments, newComment] } : l);
      setComment('');
    } finally { setPostingComment(false); }
  };

  if (loading) return <div className="text-gray-400 p-8">Loading lesson...</div>;
  if (!lesson) return <div className="text-red-500 p-8">Lesson not found or not accessible.</div>;

  const allAnswered = lesson.quizQuestions.length > 0 && lesson.quizQuestions.every(q => answers[q.id] !== undefined);

  const renderMediaPlayer = () => {
    switch (lesson.lessonType) {
      case 'Video':
        if (!lesson.contentUrl) return <div className="p-4 text-gray-400 text-sm">No video URL provided.</div>;
        // YouTube embed
        if (lesson.contentUrl.includes('youtube.com') || lesson.contentUrl.includes('youtu.be')) {
          const videoId = lesson.contentUrl.includes('youtu.be')
            ? lesson.contentUrl.split('youtu.be/')[1]?.split('?')[0]
            : new URL(lesson.contentUrl).searchParams.get('v');
          return (
            <div className="aspect-video w-full">
              <iframe
                className="w-full h-full rounded-t-xl"
                src={`https://www.youtube.com/embed/${videoId}`}
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
              />
            </div>
          );
        }
        // Vimeo
        if (lesson.contentUrl.includes('vimeo.com')) {
          const vimeoId = lesson.contentUrl.split('vimeo.com/')[1]?.split('?')[0];
          return (
            <div className="aspect-video w-full">
              <iframe className="w-full h-full rounded-t-xl" src={`https://player.vimeo.com/video/${vimeoId}`} allow="autoplay; fullscreen" allowFullScreen />
            </div>
          );
        }
        // Native MP4
        return (
          <video controls className="w-full rounded-t-xl bg-black" src={lesson.contentUrl}>
            Your browser does not support video.
          </video>
        );

      case 'Audio':
        if (!lesson.contentUrl) return <div className="p-4 text-gray-400 text-sm">No audio file provided.</div>;
        return (
          <div className="p-6 bg-gradient-to-r from-purple-50 to-blue-50 border-b border-gray-100">
            <p className="text-sm font-medium text-gray-600 mb-3">🎵 Audio Lesson</p>
            <audio controls className="w-full" src={lesson.contentUrl}>
              Your browser does not support audio.
            </audio>
          </div>
        );

      case 'PDF':
        if (!lesson.contentUrl) return <div className="p-4 text-gray-400 text-sm">No PDF provided.</div>;
        return (
          <div className="w-full" style={{ height: '520px' }}>
            <iframe
              className="w-full h-full rounded-t-xl border-0"
              src={`${lesson.contentUrl}#toolbar=1`}
              title="PDF Viewer"
            />
          </div>
        );

      case 'Text':
        return (
          <div className="p-6 border-b border-gray-100 whitespace-pre-wrap text-gray-700 text-sm leading-relaxed">
            {lesson.textContent || 'No content provided.'}
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link to={`/agent/course/${lesson.courseId}`} className="text-gray-400 hover:text-gray-600">
          <ChevronLeft size={20} />
        </Link>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h1 className="text-xl font-bold text-gray-900">{lesson.title}</h1>
            {lesson.isCompleted && <CheckCircle2 size={18} className="text-green-500" />}
          </div>
          <p className="text-xs text-gray-400">{lesson.lessonType} Lesson</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-4">
          {/* Media Player */}
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            {renderMediaPlayer()}
            <div className="px-5 py-4 flex items-center justify-between">
              <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${lesson.isCompleted ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                {lesson.isCompleted ? '✓ Completed' : 'Not completed'}
              </span>
              {!lesson.isCompleted && lesson.quizQuestions.length === 0 && (
                <button
                  onClick={handleMarkComplete}
                  disabled={marking}
                  className="px-4 py-2 bg-green-600 text-white text-sm font-semibold rounded-lg hover:bg-green-700 disabled:opacity-60"
                >
                  {marking ? 'Marking…' : 'Mark Complete'}
                </button>
              )}
            </div>
          </div>

          {/* Comments */}
          <div className="bg-white border border-gray-200 rounded-xl p-5">
            <h3 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
              💬 Comments <span className="text-xs font-normal text-gray-400">({lesson.comments.length})</span>
            </h3>

            <div className="space-y-4 mb-5">
              {lesson.comments.length === 0 && (
                <p className="text-sm text-gray-400">No comments yet. Be the first to ask a question!</p>
              )}
              {lesson.comments.map(c => (
                <div key={c.id} className="flex gap-3">
                  <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-700 text-xs font-bold shrink-0">
                    {c.authorName.charAt(0).toUpperCase()}
                  </div>
                  <div>
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-sm font-semibold text-gray-800">{c.authorName}</span>
                      <span className="text-xs text-gray-400">{new Date(c.postedAt).toLocaleDateString()}</span>
                    </div>
                    <p className="text-sm text-gray-700">{c.body}</p>
                  </div>
                </div>
              ))}
            </div>

            <form onSubmit={handlePostComment} className="flex gap-2">
              <input
                value={comment}
                onChange={e => setComment(e.target.value)}
                placeholder="Ask a question or leave feedback…"
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <button
                type="submit"
                disabled={postingComment || !comment.trim()}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-60"
              >
                <Send size={16} />
              </button>
            </form>
          </div>
        </div>

        {/* Quiz Sidebar */}
        {lesson.quizQuestions.length > 0 && (
          <div className="space-y-4">
            <div className="bg-white border border-gray-200 rounded-xl p-5 sticky top-6">
              <h3 className="font-semibold text-gray-800 mb-1">Quiz</h3>
              <p className="text-xs text-gray-400 mb-4">Passing score: {lesson.passingScorePercent}%</p>

              {/* Previous attempt result */}
              {quizResult && (
                <div className={`mb-4 p-3 rounded-lg border text-sm ${quizResult.passed ? 'bg-green-50 border-green-200 text-green-800' : 'bg-red-50 border-red-200 text-red-800'}`}>
                  <div className="font-semibold">{quizResult.passed ? '✓ Passed!' : '✗ Try Again'}</div>
                  <div>{quizResult.correct}/{quizResult.total} correct — {quizResult.scorePercent}%</div>
                  {!quizResult.passed && <div className="text-xs mt-1">Need {quizResult.passingScore}% to pass.</div>}
                </div>
              )}

              {/* Last attempt if no current result */}
              {!quizResult && lesson.lastAttempt && (
                <div className={`mb-4 p-3 rounded-lg border text-sm ${lesson.lastAttempt.passed ? 'bg-green-50 border-green-200 text-green-700' : 'bg-amber-50 border-amber-200 text-amber-700'}`}>
                  Last attempt: {lesson.lastAttempt.scorePercent}% — {lesson.lastAttempt.passed ? 'Passed' : 'Not passed'}
                </div>
              )}

              {/* Questions */}
              <div className="space-y-5">
                {lesson.quizQuestions.map((q, qi) => (
                  <div key={q.id}>
                    <p className="text-sm font-medium text-gray-800 mb-2">{qi + 1}. {q.prompt}</p>
                    <div className="space-y-1.5">
                      {q.options.map(opt => (
                        <label key={opt.id} className={`flex items-center gap-2 text-sm px-3 py-2 rounded-lg cursor-pointer transition-colors ${answers[q.id] === opt.id ? 'bg-blue-50 border border-blue-200 text-blue-800' : 'bg-gray-50 border border-gray-200 text-gray-700 hover:bg-gray-100'}`}>
                          <input
                            type="radio"
                            name={`q-${q.id}`}
                            value={opt.id}
                            checked={answers[q.id] === opt.id}
                            onChange={() => setAnswers(a => ({ ...a, [q.id]: opt.id }))}
                            className="shrink-0"
                          />
                          {opt.text}
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
              </div>

              <button
                onClick={handleSubmitQuiz}
                disabled={!allAnswered || submittingQuiz}
                className="w-full mt-5 py-2.5 bg-blue-600 text-white font-semibold text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {submittingQuiz ? 'Submitting…' : 'Submit Quiz'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
