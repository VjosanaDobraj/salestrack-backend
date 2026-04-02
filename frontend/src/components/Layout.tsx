import { useState } from 'react';
import type { ReactNode } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Menu, X, LogOut, BookOpen, BarChart3, Users, GraduationCap } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

interface LayoutProps { children: ReactNode; }

export const Layout = ({ children }: LayoutProps) => {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const isActive = (path: string) => location.pathname === path || location.pathname.startsWith(path + '/');

  const adminLinks = [
    { label: 'Dashboard', href: '/admin', icon: <BarChart3 size={20} /> },
    { label: 'Courses', href: '/admin/courses', icon: <BookOpen size={20} /> },
    { label: 'Agents', href: '/admin/agents', icon: <Users size={20} /> },
  ];
  const agentLinks = [
    { label: 'My Learning', href: '/agent', icon: <GraduationCap size={20} /> },
  ];

  const navLinks = user?.role === 'Admin' ? adminLinks : agentLinks;

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className={`${sidebarOpen ? 'w-64' : 'w-16'} border-r border-gray-200 bg-white transition-all duration-200 flex flex-col`}>
        <div className="p-4 border-b border-gray-100 flex items-center justify-between">
          {sidebarOpen && (
            <div className="flex items-center gap-2">
              <div className="bg-blue-600 text-white p-1.5 rounded-lg"><BookOpen size={16} /></div>
              <span className="font-bold text-gray-900 text-lg">SalesTrack</span>
            </div>
          )}
          <button onClick={() => setSidebarOpen(!sidebarOpen)} className="p-1 hover:bg-gray-100 rounded-lg text-gray-500">
            {sidebarOpen ? <X size={18} /> : <Menu size={18} />}
          </button>
        </div>

        <nav className="flex-1 p-3 space-y-1">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              to={link.href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                isActive(link.href)
                  ? 'bg-blue-50 text-blue-700 font-semibold'
                  : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
              }`}
            >
              <span className="shrink-0">{link.icon}</span>
              {sidebarOpen && <span>{link.label}</span>}
            </Link>
          ))}
        </nav>

        <div className="p-3 border-t border-gray-100">
          {sidebarOpen && (
            <div className="px-3 py-2 mb-1">
              <p className="text-sm font-semibold text-gray-800 truncate">{user?.fullName || user?.email}</p>
              <p className="text-xs text-gray-400">{user?.role}</p>
            </div>
          )}
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2.5 text-gray-600 hover:bg-gray-100 rounded-lg text-sm transition-colors"
          >
            <span className="shrink-0"><LogOut size={18} /></span>
            {sidebarOpen && <span>Sign out</span>}
          </button>
        </div>
      </aside>

      {/* Main */}
      <main className="flex-1 flex flex-col overflow-hidden">
        <header className="border-b border-gray-200 bg-white px-8 py-4 flex items-center justify-between shrink-0">
          <h2 className="text-lg font-semibold text-gray-800">{
            location.pathname.startsWith('/admin/courses/') ? 'Course Builder' :
            location.pathname === '/admin/courses' ? 'Courses' :
            location.pathname === '/admin/agents' ? 'Agent Management' :
            location.pathname === '/admin' ? 'Admin Dashboard' :
            location.pathname.startsWith('/agent/lesson') ? 'Lesson Player' :
            location.pathname.startsWith('/agent/course') ? 'Course' :
            'My Learning'
          }</h2>
          <span className="text-sm text-gray-400">{user?.email}</span>
        </header>
        <div className="flex-1 overflow-auto p-8">
          {children}
        </div>
      </main>
    </div>
  );
};

