import { beforeEach, describe, expect, it } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderApp } from '@/test/utils';
import { useCompra } from '@/store/compra';
import { viagemResumoMock } from '@/test/handlers';

describe('AssentosPage — mapa de assentos', () => {
  beforeEach(() => {
    useCompra.getState().selecionarViagem(viagemResumoMock);
  });

  async function montar() {
    renderApp([`/viagens/${viagemResumoMock.id}/assentos`]);
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /Assento 1, livre/ })).toBeInTheDocument(),
    );
  }

  it('seleciona ao clicar em assento livre e habilita prosseguir', async () => {
    const user = userEvent.setup();
    await montar();

    expect(screen.getByRole('button', { name: 'Prosseguir' })).toBeDisabled();

    await user.click(screen.getByRole('button', { name: /Assento 1, livre/ }));

    expect(screen.getByRole('button', { name: /Assento 1, selecionado/ })).toHaveAttribute('aria-pressed', 'true');
    expect(screen.getByRole('button', { name: 'Prosseguir' })).toBeEnabled();
  });

  it('não seleciona assento ocupado', async () => {
    await montar();

    const ocupado = screen.getByRole('button', { name: /Assento 2, ocupado/ });
    expect(ocupado).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Prosseguir' })).toBeDisabled();
  });

  it('troca a seleção ao escolher outro assento livre', async () => {
    const user = userEvent.setup();
    await montar();

    await user.click(screen.getByRole('button', { name: /Assento 1, livre/ }));
    await user.click(screen.getByRole('button', { name: /Assento 3, livre/ }));

    expect(useCompra.getState().assento).toBe(3);
    expect(screen.getByRole('button', { name: /Assento 1, livre/ })).toHaveAttribute('aria-pressed', 'false');
  });
});
