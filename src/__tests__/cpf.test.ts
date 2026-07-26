import { describe, expect, it } from 'vitest';
import { cpfValido, formatarCpf, somenteDigitos } from '@/lib/cpf';

describe('cpfValido', () => {
  it('aceita CPF válido com e sem máscara', () => {
    expect(cpfValido('529.982.247-25')).toBe(true);
    expect(cpfValido('52998224725')).toBe(true);
  });

  it('rejeita dígito verificador inválido', () => {
    expect(cpfValido('529.982.247-24')).toBe(false);
  });

  it('rejeita sequências repetidas', () => {
    expect(cpfValido('111.111.111-11')).toBe(false);
    expect(cpfValido('00000000000')).toBe(false);
  });

  it('rejeita tamanho errado', () => {
    expect(cpfValido('123')).toBe(false);
    expect(cpfValido('5299822472')).toBe(false);
  });
});

describe('formatarCpf', () => {
  it('aplica a máscara progressivamente', () => {
    expect(formatarCpf('52998224725')).toBe('529.982.247-25');
    expect(formatarCpf('529982')).toBe('529.982');
  });

  it('ignora caracteres não numéricos e limita a 11 dígitos', () => {
    expect(somenteDigitos('529.982.247-25')).toBe('52998224725');
    expect(formatarCpf('5299822472599')).toBe('529.982.247-25');
  });
});
