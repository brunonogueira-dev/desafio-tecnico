/**
 * URL base da API. Ordem de resolução:
 * 1. window.__ONIBUS_ENV__ (injetado em runtime pelo nginx no container)
 * 2. VITE_API_BASE_URL (build/dev)
 * 3. localhost:8080 (fallback de desenvolvimento)
 */
export const API_BASE_URL: string =
  (typeof window !== 'undefined' && window.__ONIBUS_ENV__?.API_BASE_URL) ||
  import.meta.env.VITE_API_BASE_URL ||
  'http://localhost:8080';
