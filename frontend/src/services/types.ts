// Tipos que espelham o contrato da API (mantidos em sincronia com o backend).

export interface Rota {
  id: string;
  origem: string;
  destino: string;
  duracaoMinutos: number;
}

export interface ViagemResumo {
  id: string;
  origem: string;
  destino: string;
  dataHoraPartida: string;
  duracaoMinutos: number;
  precoBase: number;
  totalAssentos: number;
  vagasDisponiveis: number;
}

export interface Assento {
  numero: number;
  ocupado: boolean;
}

export interface ViagemDetalhe {
  id: string;
  origem: string;
  destino: string;
  dataHoraPartida: string;
  duracaoMinutos: number;
  precoBase: number;
  totalAssentos: number;
  vagasDisponiveis: number;
  assentos: Assento[];
}

export interface PassageiroInput {
  nome: string;
  cpf: string;
  email: string;
  dataNascimento: string;
}

export interface CriarReservaInput {
  viagemId: string;
  numeroAssento: number;
  passageiro: PassageiroInput;
}

export interface ReservaViagem {
  id: string;
  origem: string;
  destino: string;
  dataHoraPartida: string;
  duracaoMinutos: number;
  precoBase: number;
}

export interface PassageiroResumo {
  nome: string;
  cpfFormatado: string;
  email: string;
}

export interface Reserva {
  codigo: string;
  status: string;
  numeroAssento: number;
  viagem: ReservaViagem;
  passageiro: PassageiroResumo;
}

/** Erro de negócio já traduzido do ProblemDetails para uso na UI. */
export interface ApiError {
  status: number;
  title: string;
  detail: string;
  errors?: Record<string, string[]>;
}
