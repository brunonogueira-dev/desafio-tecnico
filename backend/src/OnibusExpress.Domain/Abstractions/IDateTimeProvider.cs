namespace OnibusExpress.Domain.Abstractions;

/// <summary>
/// Fonte de tempo do domínio. Todo acesso a "agora" passa por aqui — nunca
/// DateTimeOffset.UtcNow direto — para que regras dependentes de tempo
/// (viagem já partida, prazo de cancelamento) sejam testáveis com relógio fixo.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
