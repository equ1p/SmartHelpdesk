import { useState, useEffect } from 'react';
import api from './api';

function App() {
  const [activeTab, setActiveTab] = useState('tickets');

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="brand">
          🎫 Smart Helpdesk
        </div>
        <nav className="nav-menu">
          <div className={`nav-item ${activeTab === 'tickets' ? 'active' : ''}`} onClick={() => setActiveTab('tickets')}>
            🎫 Tickets
          </div>
          <div className={`nav-item ${activeTab === 'articles' ? 'active' : ''}`} onClick={() => setActiveTab('articles')}>
            📚 Knowledge Base
          </div>
          <div className={`nav-item ${activeTab === 'analytics' ? 'active' : ''}`} onClick={() => setActiveTab('analytics')}>
            📊 Analytics
          </div>
          <div className={`nav-item ${activeTab === 'maintenance' ? 'active' : ''}`} onClick={() => setActiveTab('maintenance')}>
            ⚙️ Maintenance
          </div>
        </nav>
      </aside>

      {/* Main Content */}
      <main className="main-content">
        <header className="header">
          <h2>{activeTab.charAt(0).toUpperCase() + activeTab.slice(1)} Overview</h2>
        </header>
        <div className="page-content">
          {activeTab === 'tickets' && <TicketsPage />}
          {activeTab === 'articles' && <ArticlesPage />}
          {activeTab === 'analytics' && <AnalyticsPage />}
          {activeTab === 'maintenance' && <MaintenancePage />}
        </div>
      </main>
    </div>
  );
}

/* ═══════════════════════════════════════════════════
   Tickets Page
   Covers Req: 1 (CRUD Read), 3 (Dynamic Queries), 4 (SLA Report), 5 (Paging), 7 (Include), 9 (Sorting)
   ═══════════════════════════════════════════════════ */
function TicketsPage() {
  const [tickets, setTickets] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [viewMode, setViewMode] = useState('all'); // all | sla
  
  const [selectedTicket, setSelectedTicket] = useState(null);

  const loadTickets = async () => {
    if (viewMode === 'sla') {
      const res = await api.getSlaReport(true);
      if (res.ok) setTickets(res.data);
    } else {
      let query = `?page=${page}&pageSize=5`;
      if (statusFilter) query += `&status=${statusFilter}`;
      if (priorityFilter) query += `&priority=${priorityFilter}`;
      
      const res = await api.getTickets(query);
      if (res.ok) {
        setTickets(res.data.items || []);
        setTotalPages(res.data.totalPages || 1);
      }
    }
  };

  useEffect(() => { loadTickets(); }, [page, statusFilter, priorityFilter, viewMode]);

  const viewDetails = async (id) => {
    const res = await api.getTicket(id);
    if (res.ok) setSelectedTicket(res.data);
  };

  const StatusBadge = ({ s }) => {
    if (s === 0 || s === 'Open') return <span className="badge warning">Open</span>;
    if (s === 1 || s === 'InProgress') return <span className="badge neutral">In Progress</span>;
    if (s === 2 || s === 'Closed') return <span className="badge success">Closed</span>;
    return <span className="badge">{s}</span>;
  };

  if (selectedTicket) {
    return (
      <div className="card">
        <button className="btn" onClick={() => setSelectedTicket(null)}>← Back to Tickets</button>
        <div style={{ marginTop: 24 }}>
          <h2>{selectedTicket.title}</h2>
          <p style={{ color: 'var(--text-muted)', marginTop: 8 }}>{selectedTicket.description}</p>
          
          <div style={{ display: 'flex', gap: 24, marginTop: 24, background: 'var(--bg-input)', padding: 16, borderRadius: 10 }}>
            <div>
              <div style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>Client (Included)</div>
              <div style={{ fontWeight: 600 }}>{selectedTicket.clientName || selectedTicket.clientId}</div>
            </div>
            {selectedTicket.operatorName && (
              <div>
                <div style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>Operator (Included)</div>
                <div style={{ fontWeight: 600 }}>{selectedTicket.operatorName}</div>
              </div>
            )}
            <div>
              <div style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>Status</div>
              <StatusBadge s={selectedTicket.status} />
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="filters-bar">
        <button className={`btn ${viewMode === 'all' ? 'primary' : ''}`} onClick={() => setViewMode('all')}>All Tickets</button>
        <button className={`btn ${viewMode === 'sla' ? 'danger' : ''}`} onClick={() => setViewMode('sla')}>SLA Violations</button>
        
        {viewMode === 'all' && (
          <>
            <div style={{ flex: 1 }} />
            <select className="input" value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
              <option value="">All Statuses</option>
              <option value="Open">Open</option>
              <option value="InProgress">In Progress</option>
              <option value="Closed">Closed</option>
            </select>
            <select className="input" value={priorityFilter} onChange={e => setPriorityFilter(e.target.value)}>
              <option value="">All Priorities</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </>
        )}
      </div>

      <div className="card" style={{ padding: 0 }}>
        <table className="data-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {tickets.map(t => (
              <tr key={t.id} onClick={() => viewDetails(t.id)} style={{ cursor: 'pointer' }} title="Click to view details">
                <td style={{ fontWeight: 500 }}>{t.title}</td>
                <td><StatusBadge s={t.status} /></td>
                <td>{t.priority === 3 || t.priority === 'Critical' ? <span style={{color: 'var(--error)'}}>Critical</span> : t.priority}</td>
                <td>{new Date(t.createdAt).toLocaleString()}</td>
                <td>
                  <button className="btn" onClick={(e) => { e.stopPropagation(); viewDetails(t.id); }}>View Details</button>
                </td>
              </tr>
            ))}
            {tickets.length === 0 && <tr><td colSpan="5" style={{textAlign:'center'}}>No tickets found.</td></tr>}
          </tbody>
        </table>
      </div>

      {viewMode === 'all' && (
        <div style={{ display: 'flex', gap: 12, justifyContent: 'center', alignItems: 'center' }}>
          <button className="btn" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Prev</button>
          <span>Page {page} of {totalPages}</span>
          <button className="btn" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
        </div>
      )}
    </div>
  );
}

/* ═══════════════════════════════════════════════════
   Articles Page
   Covers Req: 1 (CRUD Read), 6 (FTS), 10 (Vector Search)
   ═══════════════════════════════════════════════════ */
function ArticlesPage() {
  const [articles, setArticles] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedArticle, setSelectedArticle] = useState(null);
  
  const loadArticles = async () => {
    const res = await api.getArticles();
    if (res.ok) setArticles(res.data.items || []);
  };

  useEffect(() => { loadArticles(); }, []);

  const handleFTS = async () => {
    if (!searchQuery) return loadArticles();
    const res = await api.searchArticles(searchQuery);
    if (res.ok) setArticles(res.data.items || []);
  };

  const handleVectorSearch = async () => {
    // Hardcoded vector simulation
    const embedding = [0.12, 0.85, 0.33, 0.67, 0.21, 0.94, 0.45, 0.78];
    const res = await api.vectorSearch(embedding, 3);
    if (res.ok) setArticles(res.data || []);
  };

  if (selectedArticle) {
    return (
      <div className="card">
        <button className="btn" onClick={() => setSelectedArticle(null)}>← Back to Articles</button>
        <div style={{ marginTop: 24 }}>
          <h2>{selectedArticle.title}</h2>
          <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginTop: 12 }}>
            {selectedArticle.tags?.map(t => <span key={t} className="badge neutral">#{t}</span>)}
          </div>
          <div style={{ marginTop: 24, lineHeight: '1.8', whiteSpace: 'pre-wrap', color: 'var(--text-main)' }}>
            {selectedArticle.content}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="filters-bar">
        <input 
          className="input" 
          placeholder="Search articles..." 
          value={searchQuery}
          onChange={e => setSearchQuery(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && handleFTS()}
          style={{ flex: 1 }}
        />
        <button className="btn primary" onClick={handleFTS}>Full Text Search</button>
        <button className="btn" onClick={handleVectorSearch}>🔍 Semantic (Vector) Search</button>
      </div>

      <div className="grid-cards">
        {articles.map(a => (
          <div key={a.id} className="card" style={{ cursor: 'pointer', transition: 'all 0.2s' }} onClick={() => setSelectedArticle(a)}>
            <h3 style={{ marginBottom: 12, fontSize: '1.1rem' }}>{a.title}</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: 16 }}>
              {a.content?.substring(0, 100)}...
            </p>
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
              {a.tags?.map(t => <span key={t} className="badge neutral">#{t}</span>)}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ═══════════════════════════════════════════════════
   Analytics Page
   Covers Req: 8 (Map-Reduce)
   ═══════════════════════════════════════════════════ */
function AnalyticsPage() {
  const [stats, setStats] = useState([]);

  useEffect(() => {
    api.getOperatorPerformance().then(res => {
      if (res.ok) setStats(res.data);
    });
  }, []);

  return (
    <div className="card" style={{ padding: 0 }}>
      <div style={{ padding: 24, borderBottom: '1px solid var(--border-light)' }}>
        <h3 className="card-title">Operator Performance (Map-Reduce)</h3>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
          Real-time aggregated metrics computed automatically by RavenDB background indexes.
        </p>
      </div>
      <table className="data-table">
        <thead>
          <tr>
            <th>Operator ID</th>
            <th>Closed Tickets</th>
            <th>Total Resoluton Time</th>
            <th>Avg Resolution Time</th>
          </tr>
        </thead>
        <tbody>
          {stats.map(s => (
            <tr key={s.operatorId}>
              <td style={{ fontWeight: 600 }}>{s.operatorId}</td>
              <td><span className="badge success">{s.closedCount}</span></td>
              <td>{s.totalResolutionHours.toFixed(1)} hrs</td>
              <td>{s.averageResolutionHours.toFixed(1)} hrs</td>
            </tr>
          ))}
          {stats.length === 0 && <tr><td colSpan="4" style={{textAlign:'center'}}>No analytics available.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}

/* ═══════════════════════════════════════════════════
   Maintenance Page
   Covers Req: 11 (PatchByQuery)
   ═══════════════════════════════════════════════════ */
function MaintenancePage() {
  const [days, setDays] = useState(30);
  const [result, setResult] = useState(null);

  const handlePatch = async () => {
    const res = await api.closeStaleTickets(days);
    if (res.ok) setResult(res.data.message);
  };

  return (
    <div className="card" style={{ maxWidth: 600 }}>
      <h3 className="card-title">Close Stale Tickets (PatchByQuery)</h3>
      <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: 24 }}>
        This executes a server-side RQL operation to update documents without loading them to the client.
      </p>
      
      <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
        <input 
          type="number" 
          className="input" 
          value={days} 
          onChange={e => setDays(e.target.value)} 
          style={{ width: 100 }}
        />
        <span style={{ color: 'var(--text-muted)' }}>days old</span>
        <button className="btn danger" onClick={handlePatch}>Execute Patch</button>
      </div>

      {result && (
        <div style={{ marginTop: 24, padding: 16, background: 'var(--success-bg)', color: 'var(--success)', borderRadius: 8 }}>
          ✓ {result}
        </div>
      )}
    </div>
  );
}

export default App;
