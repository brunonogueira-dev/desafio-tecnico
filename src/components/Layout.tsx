import { Link, Outlet, useLocation } from 'react-router-dom';

export function Layout() {
  const { pathname } = useLocation();

  return (
    <div className="app">
      <header className="app-header">
        <Link to="/" className="brand" aria-label="OniBus Express, ir para a busca">
          <span className="brand-mark" aria-hidden="true">🚌</span>
          <span className="brand-name">OniBus Express</span>
        </Link>
        <nav>
          <Link
            to="/consulta"
            className={pathname.startsWith('/consulta') ? 'nav-link is-active' : 'nav-link'}
          >
            Minha reserva
          </Link>
        </nav>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
      <footer className="app-footer">
        <p>OniBus Express — desafio técnico. Passagens rodoviárias.</p>
      </footer>
    </div>
  );
}
