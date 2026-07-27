import { create } from 'zustand';
import type { ViagemResumo } from '@/services/types';

interface CompraState {
  viagem: ViagemResumo | null;
  assento: number | null;
  selecionarViagem: (viagem: ViagemResumo) => void;
  selecionarAssento: (assento: number | null) => void;
  limpar: () => void;
}

/** Estado do fluxo de compra (viagem e assento escolhidos). Não é cache de API. */
export const useCompra = create<CompraState>((set) => ({
  viagem: null,
  assento: null,
  selecionarViagem: (viagem) => set({ viagem, assento: null }),
  selecionarAssento: (assento) => set({ assento }),
  limpar: () => set({ viagem: null, assento: null }),
}));
