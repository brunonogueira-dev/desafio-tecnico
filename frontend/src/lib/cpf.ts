/** Remove tudo que não for dígito. */
export function somenteDigitos(valor: string): string {
  return valor.replace(/\D/g, '');
}

/**
 * Valida CPF por tamanho, repetição e ambos os dígitos verificadores.
 * Mesma regra do backend (VO Cpf), para o feedback ser imediato no formulário.
 */
export function cpfValido(entrada: string): boolean {
  const cpf = somenteDigitos(entrada);
  if (cpf.length !== 11) return false;
  if (/^(\d)\1{10}$/.test(cpf)) return false;

  const digito = (ate: number): number => {
    let soma = 0;
    const peso = ate + 1;
    for (let i = 0; i < ate; i++) {
      soma += Number(cpf[i]) * (peso - i);
    }
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };

  return digito(9) === Number(cpf[9]) && digito(10) === Number(cpf[10]);
}

/** Aplica a máscara 000.000.000-00 progressivamente. */
export function formatarCpf(entrada: string): string {
  const cpf = somenteDigitos(entrada).slice(0, 11);
  return cpf
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/^(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4');
}
