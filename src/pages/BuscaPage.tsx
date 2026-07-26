import { useMemo, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRotas, useViagens } from '@/services/queries';
import { useCompra } from '@/store/compra';
import { ErrorState, EmptyState, ViagensSkeleton } from '@/components/States';
import { formatarBRL, formatarData, formatarHora, formatarDuracao } from '@/lib/format';
import type { ViagemResumo } from '@/services/types';

interface Filtros {
  origem: string;
  destino: string;
  data: string;
}

export function BuscaPage() {
  const navigate = useNavigate();
  const selecionarViagem = useCompra((s) => s.selecionarViagem);
  const rotasQuery = useRotas();
  const [form, setForm] = useState<Filtros>({ origem: '', destino: '', data: '' });
  const [busca, setBusca] = useState<Filtros | null>(null);

  const origens = useMemo(
    () => [...new Set((rotasQuery.data ?? []).map((r) => r.origem))].sort(),
    [rotasQuery.data],
  );
  const destinos = useMemo(
    () =>
      [...new Set((rotasQuery.data ?? []).filter((r) => r.origem === form.origem).map((r) => r.destino))].sort(),
    [rotasQuery.data, form.origem],
  );

  const viagensQuery = useViagens(
    busca?.origem ?? '',
    busca?.destino ?? '',
    busca?.data ?? '',
    busca !== null,
  );

  const submeter = (e: FormEvent) => {
    e.preventDefault();
    if (form.origem && form.destino && form.data) {
      setBusca({ ...form });
    }
  };

  const escolher = (viagem: ViagemResumo) => {
    selecionarViagem(viagem);
    navigate(`/viagens/${viagem.id}/assentos`);
  };

  const hoje = new Date().toISOString().slice(0, 10);

  return (
    <section className="page">
      <h1>Para onde você vai?</h1>
      <p className="subtitle">Escolha origem, destino e data para ver as viagens disponíveis.</p>

      <form className="card busca-form" onSubmit={submeter}>
        <div className="field">
          <label htmlFor="origem">Origem</label>
          <select
            id="origem"
            value={form.origem}
            onChange={(e) => setForm({ ...form, origem: e.target.value, destino: '' })}
            required
          >
            <option value="">Selecione</option>
            {origens.map((o) => (
              <option key={o} value={o}>{o}</option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="destino">Destino</label>
          <select
            id="destino"
            value={form.destino}
            onChange={(e) => setForm({ ...form, destino: e.target.value })}
            disabled={!form.origem}
            required
          >
            <option value="">Selecione</option>
            {destinos.map((d) => (
              <option key={d} value={d}>{d}</option>
            ))}
          </select>
        </div>

        <div className="field">
          <label htmlFor="data">Data de ida</label>
          <input
            id="data"
            type="date"
            min={hoje}
            value={form.data}
            onChange={(e) => setForm({ ...form, data: e.target.value })}
            required
          />
        </div>

        <button type="submit" className="btn btn-primary">Buscar viagens</button>
      </form>

      {rotasQuery.isError && (
        <ErrorState mensagem="Não foi possível carregar as rotas." onRetry={() => rotasQuery.refetch()} />
      )}

      {busca && (
        <div className="resultados">
          {viagensQuery.isLoading && <ViagensSkeleton />}
          {viagensQuery.isError && (
            <ErrorState
              mensagem="Não foi possível buscar as viagens. Tente novamente."
              onRetry={() => viagensQuery.refetch()}
            />
          )}
          {viagensQuery.isSuccess && viagensQuery.data.length === 0 && (
            <EmptyState mensagem="Nenhuma viagem encontrada para esta data." />
          )}
          {viagensQuery.isSuccess && viagensQuery.data.length > 0 && (
            <ul className="viagem-list">
              {viagensQuery.data.map((v) => (
                <li key={v.id}>
                  <article className="card viagem-card">
                    <div className="viagem-rota">
                      <strong>{v.origem}</strong>
                      <span aria-hidden="true">→</span>
                      <strong>{v.destino}</strong>
                    </div>
                    <dl className="viagem-info">
                      <div><dt>Partida</dt><dd>{formatarData(v.dataHoraPartida)} · {formatarHora(v.dataHoraPartida)}</dd></div>
                      <div><dt>Duração</dt><dd>{formatarDuracao(v.duracaoMinutos)}</dd></div>
                      <div><dt>Vagas</dt><dd>{v.vagasDisponiveis} de {v.totalAssentos}</dd></div>
                    </dl>
                    <div className="viagem-acao">
                      <span className="preco">{formatarBRL(v.precoBase)}</span>
                      <button
                        type="button"
                        className="btn btn-primary"
                        onClick={() => escolher(v)}
                        disabled={v.vagasDisponiveis === 0}
                      >
                        {v.vagasDisponiveis === 0 ? 'Esgotado' : 'Selecionar'}
                      </button>
                    </div>
                  </article>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  );
}
