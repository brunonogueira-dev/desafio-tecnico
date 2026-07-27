import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from './api';
import type { BuscaViagensParams, CriarReservaInput } from './types';

export function useRotas() {
  return useQuery({
    queryKey: ['rotas'],
    queryFn: () => api.listarRotas(),
    staleTime: 5 * 60 * 1000,
  });
}

export function useViagens(params: BuscaViagensParams) {
  return useQuery({
    queryKey: ['viagens', params.origem ?? '', params.destino ?? '', params.data, params.pagina ?? 1],
    queryFn: () => api.buscarViagens(params),
    enabled: Boolean(params.data),
    placeholderData: keepPreviousData,
  });
}

export function useViagem(id: string | undefined) {
  return useQuery({
    queryKey: ['viagem', id],
    queryFn: () => api.obterViagem(id!),
    enabled: Boolean(id),
  });
}

export function useCriarReserva() {
  return useMutation({
    mutationFn: (input: CriarReservaInput) => api.criarReserva(input),
  });
}

export function useReserva(codigo: string | undefined) {
  return useQuery({
    queryKey: ['reserva', codigo],
    queryFn: () => api.consultarReserva(codigo!),
    enabled: Boolean(codigo),
    retry: false,
  });
}

export function useCancelarReserva() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (codigo: string) => api.cancelarReserva(codigo),
    onSuccess: (_data, codigo) => {
      queryClient.invalidateQueries({ queryKey: ['reserva', codigo] });
    },
  });
}
