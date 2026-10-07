import { useEffect, useRef, useState } from 'react';
import type { FormEvent } from 'react';

type User = { id: string; avatar: string; name: string; email: string };

export default function App() {
  const [users, setUsers] = useState<User[]>([]);
  const [busy, setBusy] = useState(true);
  const [error, setError] = useState('');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [source, setSource] = useState('');
  const lastRead = useRef<string | null>(null);

  async function loadUsers() {
    const response = await fetch('/api/users');
    if (!response.ok) throw new Error('Could not load people. Check the API and try Refresh.');
    const read = response.headers.get('X-Database-Read');
    setSource(!lastRead.current ? 'Response loaded' : read && read === lastRead.current ? 'Redis reused the response' : 'Read from SQL Server');
    lastRead.current = read;
    setUsers(await response.json());
  }

  async function refresh() {
    setBusy(true);
    setError('');
    try { await loadUsers(); }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Could not refresh. Try again.'); }
    finally { setBusy(false); }
  }

  useEffect(() => { void refresh(); }, []);

  async function addPerson(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError('');
    try {
      const response = await fetch('/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: crypto.randomUUID(), avatar: '', name: name.trim(), email: email.trim() }),
      });
      if (!response.ok) throw new Error('Could not save this person. Check the details and try again.');
      await loadUsers();
      setName('');
      setEmail('');
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Could not save. Try again.');
    } finally { setBusy(false); }
  }

  return <>
    <header><a href="https://tarkan.dev">tarkan.dev</a><span>Modernize with Aspire</span></header>
    <main>
      <section className="introduction">
        <h1>One app.<br />All connected.</h1>
        <p>Add a person. Refresh the list. The data lives in SQL Server; Redis remembers the response.</p>
        <form onSubmit={addPerson}>
          <label htmlFor="name">Name</label>
          <input id="name" value={name} onChange={event => setName(event.target.value)} required maxLength={120} autoComplete="name" />
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={email} onChange={event => setEmail(event.target.value)} required maxLength={200} autoComplete="email" />
          <button disabled={busy || !name.trim() || !email.trim()}>{busy ? 'Working…' : 'Add person'}</button>
        </form>
      </section>
      <section className="people" aria-labelledby="people-heading">
        <div className="list-heading"><h2 id="people-heading">People</h2><button className="secondary" onClick={refresh} disabled={busy}>Refresh</button></div>
        {error && <p role="alert" className="error">{error}</p>}
        <p role="status" className="source">{busy ? 'Loading…' : source}</p>
        {users.length ? <ul>{users.map(user => <li key={user.id}><span className="initial" aria-hidden="true">{user.name.charAt(0)}</span><div><h3>{user.name}</h3><p>{user.email}</p></div></li>)}</ul>
          : !busy && !error && <p className="empty">No people yet. Add the first person.</p>}
      </section>
    </main>
    <footer>React → .NET → SQL Server + Redis</footer>
  </>;
}
