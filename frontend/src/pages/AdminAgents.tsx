import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { UserPlus, Eye } from 'lucide-react';
import { adminApi } from '../api';

interface Agent { id: string; email: string; fullName: string; }

export const AdminAgents = () => {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [groups, setGroups] = useState<{ id: number; name: string; memberCount: number }[]>([]);
  const [loading, setLoading] = useState(true);
  const [showInvite, setShowInvite] = useState(false);
  const [showGroup, setShowGroup] = useState(false);
  const [inviteForm, setInviteForm] = useState({ email: '', fullName: '', password: '' });
  const [groupForm, setGroupForm] = useState({ name: '', agentIds: [] as string[] });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = () => {
    Promise.all([adminApi.getAgents(), adminApi.getGroups()])
      .then(([a, g]) => { setAgents(a); setGroups(g); })
      .catch(console.error)
      .finally(() => setLoading(false));
  };
  useEffect(() => { load(); }, []);

  const handleInvite = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSaving(true);
    try {
      await adminApi.inviteAgent(inviteForm);
      setInviteForm({ email: '', fullName: '', password: '' });
      setShowInvite(false);
      load();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message || axiosErr?.response?.data?.errors?.join(', ') || 'Failed to create agent.';
      setError(msg);
    } finally { setSaving(false); }
  };

  const handleCreateGroup = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await adminApi.createGroup(groupForm);
      setGroupForm({ name: '', agentIds: [] });
      setShowGroup(false);
      load();
    } finally { setSaving(false); }
  };

  if (loading) return <div className="text-gray-400">Loading agents…</div>;

  return (
    <div className="max-w-5xl space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Agent Management</h1>
          <p className="text-gray-500 text-sm mt-1">{agents.length} agent{agents.length !== 1 ? 's' : ''}</p>
        </div>
        <div className="flex gap-2">
          <button onClick={() => setShowGroup(!showGroup)} className="flex items-center gap-2 px-4 py-2 border border-gray-300 text-gray-700 text-sm font-semibold rounded-lg hover:bg-gray-50">
            + New Group
          </button>
          <button onClick={() => setShowInvite(!showInvite)} className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700">
            <UserPlus size={16} /> Add Agent
          </button>
        </div>
      </div>

      {/* Invite Form */}
      {showInvite && (
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h3 className="font-semibold text-gray-800 mb-4">Add New Agent</h3>
          {error && <div className="mb-3 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">{error}</div>}
          <form onSubmit={handleInvite} className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">Full Name</label>
              <input value={inviteForm.fullName} onChange={e => setInviteForm(f => ({ ...f, fullName: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Jane Doe" required />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">Email Address</label>
              <input type="email" value={inviteForm.email} onChange={e => setInviteForm(f => ({ ...f, email: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="jane@company.com" required />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">Temporary Password</label>
              <input type="password" value={inviteForm.password} onChange={e => setInviteForm(f => ({ ...f, password: e.target.value }))} className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="Min. 8 chars" required />
            </div>
            <div className="md:col-span-3 flex gap-2">
              <button type="submit" disabled={saving} className="px-5 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 disabled:opacity-60">
                {saving ? 'Adding…' : 'Add Agent'}
              </button>
              <button type="button" onClick={() => { setShowInvite(false); setError(''); }} className="px-5 py-2 border border-gray-300 text-sm text-gray-600 rounded-lg hover:bg-gray-50">Cancel</button>
            </div>
          </form>
        </div>
      )}

      {/* Create Group Form */}
      {showGroup && (
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h3 className="font-semibold text-gray-800 mb-4">Create Agent Group</h3>
          <form onSubmit={handleCreateGroup} className="space-y-4">
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">Group Name</label>
              <input value={groupForm.name} onChange={e => setGroupForm(f => ({ ...f, name: e.target.value }))} required className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" placeholder="e.g. New Hires" />
            </div>
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">Members</label>
              <div className="grid grid-cols-2 gap-1 max-h-32 overflow-y-auto">
                {agents.map(agent => (
                  <label key={agent.id} className="flex items-center gap-2 text-sm text-gray-700 px-2 py-1 hover:bg-gray-50 rounded cursor-pointer">
                    <input type="checkbox" checked={groupForm.agentIds.includes(agent.id)} onChange={e => setGroupForm(f => ({ ...f, agentIds: e.target.checked ? [...f.agentIds, agent.id] : f.agentIds.filter(i => i !== agent.id) }))} className="rounded" />
                    {agent.fullName || agent.email}
                  </label>
                ))}
              </div>
            </div>
            <div className="flex gap-2">
              <button type="submit" disabled={saving} className="px-5 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 disabled:opacity-60">{saving ? 'Creating…' : 'Create Group'}</button>
              <button type="button" onClick={() => setShowGroup(false)} className="px-5 py-2 border border-gray-300 text-sm text-gray-600 rounded-lg hover:bg-gray-50">Cancel</button>
            </div>
          </form>
        </div>
      )}

      {/* Agents Table */}
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 font-semibold text-gray-800">Agents</div>
        {agents.length === 0 ? (
          <div className="p-8 text-center text-gray-400">No agents yet. Use "Add Agent" to create one.</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100">
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Name</th>
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Email</th>
                <th className="px-6 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {agents.map(agent => (
                <tr key={agent.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50">
                  <td className="px-6 py-4 font-medium text-gray-900">{agent.fullName || '—'}</td>
                  <td className="px-6 py-4 text-gray-500">{agent.email}</td>
                  <td className="px-6 py-4">
                    <Link to={`/admin/agents/${agent.id}`} className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-gray-100 text-gray-700 text-xs font-medium rounded-lg hover:bg-gray-200">
                      <Eye size={12} /> View Progress
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Groups */}
      {groups.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100 font-semibold text-gray-800">Groups</div>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100">
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Group</th>
                <th className="text-left px-6 py-3 text-gray-500 font-medium">Members</th>
              </tr>
            </thead>
            <tbody>
              {groups.map(g => (
                <tr key={g.id} className="border-b border-gray-50 last:border-0">
                  <td className="px-6 py-4 font-medium text-gray-900">{g.name}</td>
                  <td className="px-6 py-4 text-gray-500">{g.memberCount} agent{g.memberCount !== 1 ? 's' : ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
