import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { BuscaPage } from '@/pages/BuscaPage';
import { AssentosPage } from '@/pages/AssentosPage';
import { PassageiroPage } from '@/pages/PassageiroPage';
import { SucessoPage } from '@/pages/SucessoPage';
import { ConsultaPage } from '@/pages/ConsultaPage';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, refetchOnWindowFocus: false },
  },
});

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: '/', element: <BuscaPage /> },
      { path: '/viagens/:id/assentos', element: <AssentosPage /> },
      { path: '/checkout', element: <PassageiroPage /> },
      { path: '/sucesso', element: <SucessoPage /> },
      { path: '/consulta', element: <ConsultaPage /> },
      { path: '*', element: <BuscaPage /> },
    ],
  },
]);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  </StrictMode>,
);
