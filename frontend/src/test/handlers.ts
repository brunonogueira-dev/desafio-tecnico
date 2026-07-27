import { http, HttpResponse } from 'msw';
import type { Reserva, Rota, ViagemDetalhe, ViagemResumo, ViagensPaginadas } from '@/services/types';

export const API = 'http://localhost:8080';

export const rotasMock: Rota[] = [
  { id: 'r1', origem: 'São Paulo', destino: 'Rio de Janeiro', duracaoMinutos: 360 },
  { id: 'r2', origem: 'São Paulo', destino: 'Curitiba', duracaoMinutos: 360 },
];

export const viagemResumoMock: ViagemResumo = {
  id: 'v1',
  origem: 'São Paulo',
  destino: 'Rio de Janeiro',
  dataHoraPartida: '2026-08-10T12:00:00+00:00',
  duracaoMinutos: 360,
  precoBase: 120,
  totalAssentos: 42,
  vagasDisponiveis: 40,
};

export const viagensPaginadasMock: ViagensPaginadas = {
  itens: [viagemResumoMock],
  pagina: 1,
  tamanho: 10,
  total: 1,
  totalPaginas: 1,
};

export const viagemDetalheMock: ViagemDetalhe = {
  ...viagemResumoMock,
  assentos: Array.from({ length: 42 }, (_, i) => ({
    numero: i + 1,
    ocupado: i + 1 === 2 || i + 1 === 5,
  })),
};

export const reservaMock: Reserva = {
  codigo: 'ABC-23456',
  status: 'Confirmada',
  numeroAssento: 7,
  viagem: {
    id: 'v1',
    origem: 'São Paulo',
    destino: 'Rio de Janeiro',
    dataHoraPartida: '2026-08-10T12:00:00+00:00',
    duracaoMinutos: 360,
    precoBase: 120,
  },
  passageiro: { nome: 'Ana Souza', cpfFormatado: '529.982.247-25', email: 'ana@exemplo.com' },
};

export const handlers = [
  http.get(`${API}/rotas`, () => HttpResponse.json(rotasMock)),
  http.get(`${API}/viagens`, () => HttpResponse.json(viagensPaginadasMock)),
  http.get(`${API}/viagens/:id`, () => HttpResponse.json(viagemDetalheMock)),
  http.post(`${API}/reservas`, () => HttpResponse.json(reservaMock, { status: 201 })),
  http.get(`${API}/reservas/:codigo`, () => HttpResponse.json(reservaMock)),
  http.delete(`${API}/reservas/:codigo`, () => new HttpResponse(null, { status: 204 })),
];
