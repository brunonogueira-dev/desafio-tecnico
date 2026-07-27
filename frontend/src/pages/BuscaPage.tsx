import { useMemo, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRotas, useViagens } from '@/services/queries';
import { useCompra } from '@/store/compra';
import { ErrorState, EmptyState, ViagensSkeleton } from '@/components/States';
import { formatarBRL, formatarData, formatarHora, formatarDuracao } from '@/lib/format';
import type { ViagemResumo } from '@/services/types';

interface Criterio {
  origem: string;
  destino: string;
  data: string;
  pagina: number;
}

export function BuscaPage() {
  const navigate = useNavigate();
  const selecionarViagem = useCompra((s) => s.selecionarViagem);
  const rotasQuery = useRotas();

  const hoje = new Date().toISOString().slice(0, 10);
  const [form, setForm] = useState({ origem: '', destino: '', data: hoje });
  const [criterio, setCriterio] = useState<Criterio>({ origem: '', destino: '', data: hoje, pagina: 1 });

  const origens = useMemo(
    () => [...new Set((rotasQuery.data ?? []).map((r) => r.origem))].sort(),
    [rotasQuery.data],
  );
  const destinos = useMemo(
    () =>
      [...new Set((rotasQuery.data ?? []).filter((r) => r.origem === form.origem).map((r) => r.destino))].sort(),
    [rotasQuery.data, form.origem],
  );

  const viagensQuery = useViagens({
    origem: criterio.origem || undefined,
    destino: criterio.destino || undefined,
    data: criterio.data,
    pagina: criterio.pagina,
  });

  const submeter = (e: FormEvent) => {
    e.preventDefault();
    setCriterio({ origem: form.origem, destino: form.destino, data: form.data || hoje, pagina: 1 });
  };

  const irParaPagina = (pagina: number) => setCriterio((c) => ({ ...c, pagina }));

  const escolher = (viagem: ViagemResumo) => {
    selecionarViagem(viagem);
    navigate(`/viagens/${viagem.id}/assentos`);
  };

  const pagina = viagensQuery.data;

  return (
    <section className="page">
      <h1>Para onde você vai?</h1>
      <p className="subtitle">
        Estas são as viagens de {formatarData(criterio.data)}. Filtre por origem e destino se quiser.
      </p>

      <form className="card busca-form" onSubmit={submeter}>
        <div className="field">
          <label htmlFor="origem">Origem</label>
          <select
            id="origem"
            value={form.origem}
            onChange={(e) => setForm({ ...form, origem: e.target.value, destino: '' })}
          >
            <option value="">Todas</option>
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
          >
            <option value="">Todos</option>
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
          />
        </div>

        <button type="submit" className="btn btn-primary">Buscar viagens</button>
      </form>

      {rotasQuery.isError && (
        <ErrorState mensagem="Não foi possível carregar as rotas." onRetry={() => rotasQuery.refetch()} />
      )}

      <div className="resultados">
        {viagensQuery.isLoading && <ViagensSkeleton />}

        {viagensQuery.isError && (
          <ErrorState
            mensagem="Não foi possível buscar as viagens. Tente novamente."
            onRetry={() => viagensQuery.refetch()}
          />
        )}

        {pagina && pagina.itens.length === 0 && (
          <EmptyState mensagem="Nenhuma viagem encontrada para esta data." />
        )}

        {pagina && pagina.itens.length > 0 && (
          <>
            <p className="resultados-total">{pagina.total} viagem(ns) encontrada(s)</p>
            <ul className="viagem-list">
              {pagina.itens.map((v) => (
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

            {pagina.totalPaginas > 1 && (
              <nav className="paginacao" aria-label="Paginação">
                <button
                  type="button"
                  className="btn"
                  onClick={() => irParaPagina(criterio.pagina - 1)}
                  disabled={criterio.pagina <= 1}
                >
                  Anterior
                </button>
                <span aria-live="polite">Página {pagina.pagina} de {pagina.totalPaginas}</span>
                <button
                  type="button"
                  className="btn"
                  onClick={() => irParaPagina(criterio.pagina + 1)}
                  disabled={criterio.pagina >= pagina.totalPaginas}
                >
                  Próxima
                </button>
              </nav>
            )}
          </>
        )}
      </div>
    </section>
  );
}
