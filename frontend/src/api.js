const BASE = '/api';

async function request(method, path, body = null) {
  const url = `${BASE}${path}`;
  const opts = {
    method,
    headers: { 'Content-Type': 'application/json' },
  };
  if (body) opts.body = JSON.stringify(body);

  const start = performance.now();
  const res = await fetch(url, opts);
  const elapsed = Math.round(performance.now() - start);

  let data = null;
  const text = await res.text();
  try { data = JSON.parse(text); } catch { data = text; }

  return {
    ok: res.ok,
    status: res.status,
    method,
    url: path,
    elapsed,
    data,
  };
}

const api = {
  // ── Req 1: CRUD ──────────────────────────────
  getUsers: () => request('GET', '/users'),
  createUser: (u) => request('POST', '/users', u),
  getUser: (id) => request('GET', `/users/${encodeURIComponent(id)}`),
  updateUser: (id, u) => request('PUT', `/users/${encodeURIComponent(id)}`, u),
  deleteUser: (id) => request('DELETE', `/users/${encodeURIComponent(id)}`),

  getTickets: (params = '') => request('GET', `/tickets${params}`),
  createTicket: (t) => request('POST', '/tickets', t),
  getTicket: (id) => request('GET', `/tickets/${encodeURIComponent(id)}`),
  updateTicket: (id, t) => request('PUT', `/tickets/${encodeURIComponent(id)}`, t),
  deleteTicket: (id) => request('DELETE', `/tickets/${encodeURIComponent(id)}`),

  getArticles: (params = '') => request('GET', `/articles${params}`),
  createArticle: (a) => request('POST', '/articles', a),
  updateArticle: (id, a) => request('PUT', `/articles/${encodeURIComponent(id)}`, a),
  deleteArticle: (id) => request('DELETE', `/articles/${encodeURIComponent(id)}`),

  // ── Req 3: Dynamic query → auto-index ────────
  getUsersByRole: (role) => request('GET', `/users?role=${role}`),
  getTicketsFiltered: (status, priority) => {
    const p = new URLSearchParams();
    if (status) p.set('status', status);
    if (priority) p.set('priority', priority);
    return request('GET', `/tickets?${p.toString()}`);
  },

  // ── Req 4: Static index + SLA report ─────────
  getSlaReport: (violatedOnly = false) =>
    request('GET', `/tickets/sla-report${violatedOnly ? '?violatedOnly=true' : ''}`),

  // ── Req 5: Paging ────────────────────────────
  getTicketsPaged: (page, pageSize) =>
    request('GET', `/tickets?page=${page}&pageSize=${pageSize}`),

  // ── Req 6: Full Text Search ──────────────────
  searchArticles: (q) => request('GET', `/articles/search?q=${encodeURIComponent(q)}`),

  // ── Req 8: Map-Reduce ────────────────────────
  getOperatorPerformance: () => request('GET', '/analytics/operators'),

  // ── Req 10: Vector Search ────────────────────
  vectorSearch: (embedding, maxResults = 5) =>
    request('POST', '/articles/vector-search', { embedding, maxResults }),

  // ── Req 11: PatchByQuery ─────────────────────
  closeStaleTickets: (staleDays = 30) =>
    request('POST', `/maintenance/close-stale-tickets?staleDays=${staleDays}`),
};

export default api;
