import { API_BASE_URL } from './config';
import type {
  ApiError,
  CriarReservaInput,
  Reserva,
  Rota,
  ViagemDetalhe,
  ViagemResumo,
} from './types';

/** Mensagem amigável em português, orientada à ação, a partir de um ApiError. */
export class ApiRequestError extends Error {
  readonly status: number;
  readonly detail: string;
  readonly errors?: Record<string, string[]>;

  constructor(apiError: ApiError) {
    super(apiError.detail || apiError.title);
    this.name = 'ApiRequestError';
    this.status = apiError.status;
    this.detail = apiError.detail || apiError.title;
    this.errors = apiError.errors;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let resposta: Response;
  try {
    resposta = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...init?.headers,
      },
    });
  } catch {
    throw new ApiRequestError({
      status: 0,
      title: 'Falha de conexão',
      detail: 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.',
    });
  }

  if (resposta.status === 204) {
    return undefined as T;
  }

  const texto = await resposta.text();
  const corpo = texto ? JSON.parse(texto) : undefined;

  if (!resposta.ok) {
    throw new ApiRequestError({
      status: resposta.status,
      title: corpo?.title ?? 'Erro',
      detail: corpo?.detail ?? 'Ocorreu um erro ao processar a solicitação.',
      errors: corpo?.errors,
    });
  }

  return corpo as T;
}

export const api = {
  listarRotas: () => request<Rota[]>('/rotas'),

  buscarViagens: (origem: string, destino: string, data: string) =>
    request<ViagemResumo[]>(
      `/viagens?origem=${encodeURIComponent(origem)}&destino=${encodeURIComponent(destino)}&data=${data}`,
    ),

  obterViagem: (id: string) => request<ViagemDetalhe>(`/viagens/${id}`),

  criarReserva: (input: CriarReservaInput) =>
    request<Reserva>('/reservas', { method: 'POST', body: JSON.stringify(input) }),

  consultarReserva: (codigo: string) => request<Reserva>(`/reservas/${encodeURIComponent(codigo)}`),

  cancelarReserva: (codigo: string) =>
    request<void>(`/reservas/${encodeURIComponent(codigo)}`, { method: 'DELETE' }),
};
