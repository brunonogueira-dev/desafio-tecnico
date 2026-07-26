import { useEffect } from 'react';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { useViagem } from '@/services/queries';
import { useCompra } from '@/store/compra';
import { SeatMap } from '@/components/SeatMap';
import { ErrorState, Spinner } from '@/components/States';
import { formatarBRL, formatarData, formatarHora } from '@/lib/format';

export function AssentosPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const viagem = useCompra((s) => s.viagem);
  const assento = useCompra((s) => s.assento);
  const selecionarAssento = useCompra((s) => s.selecionarAssento);
  const viagemQuery = useViagem(id);

  // Se o assento selecionado deixou de existir/ficou ocupado, limpa a seleção.
  useEffect(() => {
    if (assento && viagemQuery.data) {
      const alvo = viagemQuery.data.assentos.find((a) => a.numero === assento);
      if (!alvo || alvo.ocupado) selecionarAssento(null);
    }
  }, [assento, viagemQuery.data, selecionarAssento]);

  // Guarda de rota: entrar aqui sem viagem no store volta para a busca.
  if (!viagem) {
    return <Navigate to="/" replace />;
  }

  return (
    <section className="page">
      <button type="button" className="link-voltar" onClick={() => navigate(-1)}>
        ← Voltar para a busca
      </button>

      <header className="viagem-header card">
        <div className="viagem-rota">
          <strong>{viagem.origem}</strong>
          <span aria-hidden="true">→</span>
          <strong>{viagem.destino}</strong>
        </div>
        <p>{formatarData(viagem.dataHoraPartida)} · {formatarHora(viagem.dataHoraPartida)}</p>
        <p className="preco">{formatarBRL(viagem.precoBase)}</p>
      </header>

      <h1>Escolha seu assento</h1>

      {viagemQuery.isLoading && <Spinner label="Carregando assentos…" />}
      {viagemQuery.isError && (
        <ErrorState mensagem="Não foi possível carregar os assentos." onRetry={() => viagemQuery.refetch()} />
      )}
      {viagemQuery.isSuccess && (
        <>
          <SeatMap
            assentos={viagemQuery.data.assentos}
            selecionado={assento}
            onSelecionar={selecionarAssento}
          />
          <div className="acao-rodape">
            <span aria-live="polite">
              {assento ? `Assento ${assento} selecionado` : 'Selecione um assento para continuar'}
            </span>
            <button
              type="button"
              className="btn btn-primary"
              disabled={!assento}
              onClick={() => navigate('/checkout')}
            >
              Prosseguir
            </button>
          </div>
        </>
      )}
    </section>
  );
}
