import { useState, type FormEvent } from 'react';
import { useCancelarReserva, useReserva } from '@/services/queries';
import { ApiRequestError } from '@/services/api';
import { ErrorState, Spinner } from '@/components/States';
import { formatarBRL, formatarData, formatarHora } from '@/lib/format';

function formatarCodigo(entrada: string): string {
  const limpo = entrada.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 8);
  const letras = limpo.slice(0, 3);
  const numeros = limpo.slice(3, 8);
  return numeros ? `${letras}-${numeros}` : letras;
}

export function ConsultaPage() {
  const [codigoInput, setCodigoInput] = useState('');
  const [codigoBusca, setCodigoBusca] = useState<string | undefined>(undefined);
  const reservaQuery = useReserva(codigoBusca);
  const cancelar = useCancelarReserva();

  const buscar = (e: FormEvent) => {
    e.preventDefault();
    if (codigoInput.trim()) {
      cancelar.reset();
      setCodigoBusca(codigoInput.trim());
    }
  };

  const cancelarReserva = () => {
    if (!codigoBusca) return;
    if (!window.confirm('Tem certeza que deseja cancelar esta reserva?')) return;
    cancelar.mutate(codigoBusca);
  };

  const naoEncontrada = reservaQuery.error instanceof ApiRequestError && reservaQuery.error.status === 404;
  const reserva = reservaQuery.data;
  const cancelada = reserva?.status === 'Cancelada' || cancelar.isSuccess;
  const erroCancelamento =
    cancelar.error instanceof ApiRequestError ? cancelar.error.detail : null;

  return (
    <section className="page">
      <h1>Minha reserva</h1>
      <p className="subtitle">Informe o código recebido na compra (formato AAA-99999).</p>

      <form className="card consulta-form" onSubmit={buscar}>
        <div className="field">
          <label htmlFor="codigo">Código da reserva</label>
          <input
            id="codigo"
            type="text"
            placeholder="ABC-23456"
            value={codigoInput}
            onChange={(e) => setCodigoInput(formatarCodigo(e.target.value))}
          />
        </div>
        <button type="submit" className="btn btn-primary" disabled={!codigoInput.trim()}>
          Consultar
        </button>
      </form>

      {reservaQuery.isFetching && <Spinner label="Buscando reserva…" />}
      {naoEncontrada && <ErrorState mensagem="Reserva não encontrada. Confira o código informado." />}
      {reservaQuery.isError && !naoEncontrada && (
        <ErrorState mensagem="Não foi possível consultar a reserva." onRetry={() => reservaQuery.refetch()} />
      )}

      {reserva && (
        <article className="card reserva-detalhe">
          <div className="reserva-topo">
            <div className="viagem-rota">
              <strong>{reserva.viagem.origem}</strong>
              <span aria-hidden="true">→</span>
              <strong>{reserva.viagem.destino}</strong>
            </div>
            <span className={cancelada ? 'badge badge-cancelada' : 'badge badge-confirmada'}>
              {cancelada ? 'Cancelada' : 'Confirmada'}
            </span>
          </div>
          <dl>
            <div><dt>Código</dt><dd>{reserva.codigo}</dd></div>
            <div><dt>Partida</dt><dd>{formatarData(reserva.viagem.dataHoraPartida)} · {formatarHora(reserva.viagem.dataHoraPartida)}</dd></div>
            <div><dt>Assento</dt><dd>{reserva.numeroAssento}</dd></div>
            <div><dt>Passageiro</dt><dd>{reserva.passageiro.nome}</dd></div>
            <div><dt>CPF</dt><dd>{reserva.passageiro.cpfFormatado}</dd></div>
            <div><dt>Valor</dt><dd className="preco">{formatarBRL(reserva.viagem.precoBase)}</dd></div>
          </dl>

          {erroCancelamento && (
            <div className="banner banner-error" role="alert">{erroCancelamento}</div>
          )}

          {!cancelada && (
            <button
              type="button"
              className="btn btn-danger"
              onClick={cancelarReserva}
              disabled={cancelar.isPending}
            >
              {cancelar.isPending ? 'Cancelando…' : 'Cancelar reserva'}
            </button>
          )}
          {cancelada && !erroCancelamento && (
            <p className="banner banner-ok">Reserva cancelada. O assento foi liberado.</p>
          )}
        </article>
      )}
    </section>
  );
}
