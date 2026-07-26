import { describe, expect, it } from 'vitest';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/server';
import { API } from '@/test/handlers';
import { renderApp } from '@/test/utils';

async function preencherBusca() {
  const user = userEvent.setup();
  await waitFor(() => expect(screen.getByRole('option', { name: 'São Paulo' })).toBeInTheDocument());
  await user.selectOptions(screen.getByLabelText('Origem'), 'São Paulo');
  await user.selectOptions(screen.getByLabelText('Destino'), 'Rio de Janeiro');
  fireEvent.change(screen.getByLabelText('Data de ida'), { target: { value: '2026-08-10' } });
  await user.click(screen.getByRole('button', { name: 'Buscar viagens' }));
}

describe('BuscaPage', () => {
  it('mostra as viagens retornadas após buscar', async () => {
    renderApp(['/']);
    await preencherBusca();

    const lista = await screen.findByRole('list');
    expect(within(lista).getByText('40 de 42')).toBeInTheDocument();
    expect(within(lista).getByText(/R\$\s?120,00/)).toBeInTheDocument();
  });

  it('mostra mensagem quando não há viagens', async () => {
    server.use(http.get(`${API}/viagens`, () => HttpResponse.json([])));
    renderApp(['/']);
    await preencherBusca();

    expect(await screen.findByText('Nenhuma viagem encontrada para esta data.')).toBeInTheDocument();
  });

  it('mostra erro com botão de tentar novamente quando a API falha', async () => {
    server.use(http.get(`${API}/viagens`, () => new HttpResponse(null, { status: 500 })));
    renderApp(['/']);
    await preencherBusca();

    expect(await screen.findByRole('alert')).toHaveTextContent(/não foi possível buscar/i);
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument();
  });
});
