import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from './api';
import type { CriarReservaInput } from './types';

export function useRotas() {
  return useQuery({
    queryKey: ['rotas'],
    queryFn: () => api.listarRotas(),
    staleTime: 5 * 60 * 1000,
  });
}

export function useViagens(origem: string, destino: string, data: string, habilitado: boolean) {
  return useQuery({
    queryKey: ['viagens', origem, destino, data],
    queryFn: () => api.buscarViagens(origem, destino, data),
    enabled: habilitado,
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
