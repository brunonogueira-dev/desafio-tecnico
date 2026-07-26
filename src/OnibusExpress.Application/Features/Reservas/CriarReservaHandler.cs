using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Application.Features.Reservas;

/// <summary>Cria uma reserva de assento (POST /reservas).</summary>
public sealed class CriarReservaHandler(
    IViagemRepository viagens,
    IReservaRepository reservas,
    IPassageiroRepository passageiros,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
{
    private const int MaxTentativasCodigo = 10;

    public async Task<Result<ReservaDto>> ExecutarAsync(
        CriarReservaRequest request, CancellationToken cancellationToken)
    {
        if (!Cpf.TryCriar(request.Passageiro.Cpf, out var cpf))
        {
            return Error.Validacao("cpf", "CPF inválido.");
        }

        if (string.IsNullOrWhiteSpace(request.Passageiro.Nome))
        {
            return Error.Validacao("nome", "Nome do passageiro é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Passageiro.Email) || !request.Passageiro.Email.Contains('@'))
        {
            return Error.Validacao("email", "E-mail do passageiro é inválido.");
        }

        if (request.Passageiro.DataNascimento >= DateOnly.FromDateTime(clock.UtcNow.UtcDateTime))
        {
            return Error.Validacao("dataNascimento", "A data de nascimento deve estar no passado.");
        }

        var viagem = await viagens.ObterComRotaAsync(request.ViagemId, cancellationToken);
        if (viagem is null)
        {
            return Error.NaoEncontrado("Viagem não encontrada.");
        }

        if (viagem.JaPartiu(clock))
        {
            return Error.ViagemJaPartiu("Não é possível reservar em uma viagem que já partiu.");
        }

        if (!viagem.AssentoDentroDoRange(request.NumeroAssento))
        {
            return Error.Validacao("numeroAssento", $"O assento deve estar entre 1 e {viagem.TotalAssentos}.");
        }

        // Checagem amigável. A garantia real contra corrida é o índice único
        // parcial no banco, cuja violação a Api traduz em 409.
        if (await reservas.ExisteConfirmadaParaAssentoAsync(viagem.Id, request.NumeroAssento, cancellationToken))
        {
            return Error.AssentoIndisponivel($"O assento {request.NumeroAssento} já está reservado.");
        }

        var passageiro = await passageiros.ObterPorCpfAsync(cpf!, cancellationToken);
        if (passageiro is null)
        {
            passageiro = new Passageiro(
                request.Passageiro.Nome, cpf!, request.Passageiro.Email, request.Passageiro.DataNascimento);
            passageiros.Adicionar(passageiro);
        }
        else
        {
            // Mesmo CPF: mantém o cadastro, mas atualiza nome/e-mail se vieram diferentes.
            passageiro.AtualizarDados(request.Passageiro.Nome, request.Passageiro.Email);
        }

        var codigo = await GerarCodigoUnicoAsync(cancellationToken);
        var reserva = new Reserva(viagem.Id, passageiro.Id, request.NumeroAssento, codigo);
        reservas.Adicionar(reserva);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success(ReservaMapper.ParaDto(reserva, viagem, passageiro));
    }

    private async Task<CodigoReserva> GerarCodigoUnicoAsync(CancellationToken cancellationToken)
    {
        for (var tentativa = 0; tentativa < MaxTentativasCodigo; tentativa++)
        {
            var codigo = CodigoReserva.Gerar();
            if (!await reservas.CodigoEmUsoAsync(codigo, cancellationToken))
            {
                return codigo;
            }
        }

        throw new InvalidOperationException("Não foi possível gerar um código de reserva único.");
    }
}
