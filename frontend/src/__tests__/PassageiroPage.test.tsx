import { beforeEach, describe, expect, it } from 'vitest';
import { fireEvent, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/server';
import { API, reservaMock, viagemResumoMock } from '@/test/handlers';
import { renderApp } from '@/test/utils';
import { useCompra } from '@/store/compra';
import type { CriarReservaInput } from '@/services/types';

describe('PassageiroPage — formulário', () => {
  beforeEach(() => {
    useCompra.getState().selecionarViagem(viagemResumoMock);
    useCompra.getState().selecionarAssento(7);
  });

  async function preencher(cpf: string) {
    const user = userEvent.setup();
    await user.type(screen.getByLabelText('Nome completo'), 'Ana Souza');
    await user.type(screen.getByLabelText('CPF'), cpf);
    await user.type(screen.getByLabelText('E-mail'), 'ana@exemplo.com');
    fireEvent.change(screen.getByLabelText('Data de nascimento'), { target: { value: '1990-05-20' } });
    return user;
  }

  it('mostra erro em cada campo ao enviar vazio', async () => {
    const user = userEvent.setup();
    renderApp(['/checkout']);

    await user.click(screen.getByRole('button', { name: 'Confirmar reserva' }));

    expect(await screen.findByText('Informe o nome completo.')).toBeInTheDocument();
    expect(screen.getByText('CPF inválido. Confira os dígitos.')).toBeInTheDocument();
    expect(screen.getByText('E-mail inválido.')).toBeInTheDocument();
    expect(screen.getByText('Informe a data de nascimento.')).toBeInTheDocument();
  });

  it('rejeita CPF com dígito verificador inválido', async () => {
    renderApp(['/checkout']);
    const user = await preencher('111.111.111-11');

    await user.click(screen.getByRole('button', { name: 'Confirmar reserva' }));

    expect(await screen.findByText('CPF inválido. Confira os dígitos.')).toBeInTheDocument();
  });

  it('envia o payload correto e mostra o código no sucesso', async () => {
    let capturado: CriarReservaInput | null = null;
    server.use(
      http.post(`${API}/reservas`, async ({ request }) => {
        capturado = (await request.json()) as CriarReservaInput;
        return HttpResponse.json(reservaMock, { status: 201 });
      }),
    );

    renderApp(['/checkout']);
    const user = await preencher('529.982.247-25');
    await user.click(screen.getByRole('button', { name: 'Confirmar reserva' }));

    expect(await screen.findByTestId('codigo-reserva')).toHaveTextContent('ABC-23456');
    expect(capturado).toEqual({
      viagemId: 'v1',
      numeroAssento: 7,
      passageiro: {
        nome: 'Ana Souza',
        cpf: '52998224725',
        email: 'ana@exemplo.com',
        dataNascimento: '1990-05-20',
      },
    });
  });

  it('ao receber 409, orienta a escolher outro assento', async () => {
    server.use(
      http.post(`${API}/reservas`, () =>
        HttpResponse.json(
          { title: 'Assento indisponível', status: 409, detail: 'O assento 7 já está reservado.' },
          { status: 409 },
        ),
      ),
    );

    renderApp(['/checkout']);
    const user = await preencher('529.982.247-25');
    await user.click(screen.getByRole('button', { name: 'Confirmar reserva' }));

    expect(await screen.findByText(/escolha outro assento/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Escolher outro assento' })).toBeInTheDocument();
  });
});
