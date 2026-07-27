import type { ReactElement } from 'react';
import { render } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { BuscaPage } from '@/pages/BuscaPage';
import { AssentosPage } from '@/pages/AssentosPage';
import { PassageiroPage } from '@/pages/PassageiroPage';
import { SucessoPage } from '@/pages/SucessoPage';
import { ConsultaPage } from '@/pages/ConsultaPage';

type Entrada = string | { pathname: string; state?: unknown };

export function renderApp(initialEntries: Entrada[] = ['/']) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<BuscaPage />} />
            <Route path="/viagens/:id/assentos" element={<AssentosPage />} />
            <Route path="/checkout" element={<PassageiroPage />} />
            <Route path="/sucesso" element={<SucessoPage />} />
            <Route path="/consulta" element={<ConsultaPage />} />
          </Route>
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

// Renderiza um componente isolado apenas com o QueryClient (sem rotas).
export function renderWithQuery(ui: ReactElement) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}
