import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Navigate, useNavigate } from 'react-router-dom';
import { useCompra } from '@/store/compra';
import { useCriarReserva } from '@/services/queries';
import { ApiRequestError } from '@/services/api';
import { cpfValido, formatarCpf, somenteDigitos } from '@/lib/cpf';
import { formatarBRL, formatarData, formatarHora } from '@/lib/format';

const schema = z.object({
  nome: z.string().trim().min(3, 'Informe o nome completo.'),
  cpf: z.string().refine((v) => cpfValido(v), 'CPF inválido. Confira os dígitos.'),
  email: z.string().trim().email('E-mail inválido.'),
  dataNascimento: z
    .string()
    .min(1, 'Informe a data de nascimento.')
    .refine((v) => new Date(v) < new Date(), 'A data de nascimento deve estar no passado.'),
});

type FormData = z.infer<typeof schema>;

export function PassageiroPage() {
  const navigate = useNavigate();
  const viagem = useCompra((s) => s.viagem);
  const assento = useCompra((s) => s.assento);
  const criarReserva = useCriarReserva();

  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { nome: '', cpf: '', email: '', dataNascimento: '' },
  });

  // Guarda de rota: precisa de viagem e assento escolhidos.
  if (!viagem || !assento) {
    return <Navigate to="/" replace />;
  }

  const assentoIndisponivel = criarReserva.error instanceof ApiRequestError && criarReserva.error.status === 409;
  const outroErro =
    criarReserva.error instanceof ApiRequestError && criarReserva.error.status !== 409
      ? criarReserva.error.detail
      : null;

  const enviar = handleSubmit((dados) => {
    criarReserva.mutate(
      {
        viagemId: viagem.id,
        numeroAssento: assento,
        passageiro: {
          nome: dados.nome.trim(),
          cpf: somenteDigitos(dados.cpf),
          email: dados.email.trim(),
          dataNascimento: dados.dataNascimento,
        },
      },
      {
        onSuccess: (reserva) => navigate('/sucesso', { state: { reserva } }),
      },
    );
  });

  return (
    <section className="page">
      <button type="button" className="link-voltar" onClick={() => navigate(-1)}>
        ← Voltar para os assentos
      </button>

      <h1>Dados do passageiro</h1>

      <div className="checkout">
        <form className="card form" onSubmit={enviar} noValidate>
          <div className="field">
            <label htmlFor="nome">Nome completo</label>
            <input id="nome" type="text" aria-invalid={!!errors.nome} {...register('nome')} />
            {errors.nome && <span className="field-error" role="alert">{errors.nome.message}</span>}
          </div>

          <div className="field">
            <label htmlFor="cpf">CPF</label>
            <Controller
              control={control}
              name="cpf"
              render={({ field }) => (
                <input
                  id="cpf"
                  type="text"
                  inputMode="numeric"
                  placeholder="000.000.000-00"
                  aria-invalid={!!errors.cpf}
                  value={field.value}
                  onChange={(e) => field.onChange(formatarCpf(e.target.value))}
                  onBlur={field.onBlur}
                />
              )}
            />
            {errors.cpf && <span className="field-error" role="alert">{errors.cpf.message}</span>}
          </div>

          <div className="field">
            <label htmlFor="email">E-mail</label>
            <input id="email" type="email" aria-invalid={!!errors.email} {...register('email')} />
            {errors.email && <span className="field-error" role="alert">{errors.email.message}</span>}
          </div>

          <div className="field">
            <label htmlFor="dataNascimento">Data de nascimento</label>
            <input
              id="dataNascimento"
              type="date"
              aria-invalid={!!errors.dataNascimento}
              {...register('dataNascimento')}
            />
            {errors.dataNascimento && (
              <span className="field-error" role="alert">{errors.dataNascimento.message}</span>
            )}
          </div>

          {assentoIndisponivel && (
            <div className="banner banner-error" role="alert">
              Este assento acabou de ser reservado por outra pessoa. Volte e escolha outro assento.
              <button type="button" className="btn btn-secondary" onClick={() => navigate(-1)}>
                Escolher outro assento
              </button>
            </div>
          )}
          {outroErro && (
            <div className="banner banner-error" role="alert">{outroErro}</div>
          )}

          <button type="submit" className="btn btn-primary" disabled={criarReserva.isPending}>
            {criarReserva.isPending ? 'Confirmando…' : 'Confirmar reserva'}
          </button>
        </form>

        <aside className="card resumo" aria-label="Resumo da compra">
          <h2>Resumo</h2>
          <div className="viagem-rota">
            <strong>{viagem.origem}</strong>
            <span aria-hidden="true">→</span>
            <strong>{viagem.destino}</strong>
          </div>
          <dl>
            <div><dt>Partida</dt><dd>{formatarData(viagem.dataHoraPartida)} · {formatarHora(viagem.dataHoraPartida)}</dd></div>
            <div><dt>Assento</dt><dd>{assento}</dd></div>
            <div><dt>Total</dt><dd className="preco">{formatarBRL(viagem.precoBase)}</dd></div>
          </dl>
        </aside>
      </div>
    </section>
  );
}
