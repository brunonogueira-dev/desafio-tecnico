import { useEffect, useState } from 'react';
import { Link, Navigate, useLocation } from 'react-router-dom';
import { useCompra } from '@/store/compra';
import { formatarBRL, formatarData, formatarHora } from '@/lib/format';
import type { Reserva } from '@/services/types';

export function SucessoPage() {
  const location = useLocation();
  const limpar = useCompra((s) => s.limpar);
  const reserva = (location.state as { reserva?: Reserva } | null)?.reserva;
  const [copiado, setCopiado] = useState(false);

  useEffect(() => {
    if (reserva) limpar();
  }, [reserva, limpar]);

  if (!reserva) {
    return <Navigate to="/" replace />;
  }

  const copiar = async () => {
    try {
      await navigator.clipboard.writeText(reserva.codigo);
      setCopiado(true);
      setTimeout(() => setCopiado(false), 2000);
    } catch {
      setCopiado(false);
    }
  };

  return (
    <section className="page page-centro">
      <div className="card sucesso">
        <span className="sucesso-check" aria-hidden="true">✓</span>
        <h1>Reserva confirmada!</h1>
        <p>Guarde o código abaixo para consultar ou cancelar sua reserva.</p>

        <div className="codigo-destaque">
          <span className="codigo" data-testid="codigo-reserva">{reserva.codigo}</span>
          <button type="button" className="btn btn-secondary" onClick={copiar}>
            {copiado ? 'Copiado!' : 'Copiar'}
          </button>
        </div>

        <dl className="resumo-sucesso">
          <div><dt>Trajeto</dt><dd>{reserva.viagem.origem} → {reserva.viagem.destino}</dd></div>
          <div><dt>Partida</dt><dd>{formatarData(reserva.viagem.dataHoraPartida)} · {formatarHora(reserva.viagem.dataHoraPartida)}</dd></div>
          <div><dt>Assento</dt><dd>{reserva.numeroAssento}</dd></div>
          <div><dt>Passageiro</dt><dd>{reserva.passageiro.nome}</dd></div>
          <div><dt>Valor</dt><dd className="preco">{formatarBRL(reserva.viagem.precoBase)}</dd></div>
        </dl>

        <div className="sucesso-acoes">
          <Link to="/consulta" className="btn btn-secondary">Ver minha reserva</Link>
          <Link to="/" className="btn btn-primary">Nova busca</Link>
        </div>
      </div>
    </section>
  );
}
