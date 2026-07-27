using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OnibusExpress.Domain.Exceptions;

namespace OnibusExpress.Api.Middleware;

/// <summary>
/// Converte toda exceção não tratada em ProblemDetails. Intercepta em especial
/// a violação do índice único parcial de assento, respondendo 409.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private const string IndiceAssento = "IX_reservas_ViagemId_NumeroAssento";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DbUpdateException ex) when (ExtrairViolacaoUnica(ex) is { } pg)
        {
            var ehAssento = pg.ConstraintName == IndiceAssento;
            logger.LogWarning("Violação de unicidade ({Constraint}) tratada como conflito.", pg.ConstraintName);
            await EscreverProblema(
                context,
                StatusCodes.Status409Conflict,
                ehAssento ? "Assento indisponível" : "Conflito de dados",
                ehAssento
                    ? "O assento acabou de ser reservado por outra pessoa. Escolha outro assento."
                    : "Não foi possível concluir a reserva por um conflito simultâneo. Tente novamente.");
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Invariante de domínio violada.");
            await EscreverProblema(
                context,
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}.",
                context.Request.Method, context.Request.Path);
            await EscreverProblema(
                context,
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                environment.IsDevelopment() ? ex.ToString() : "Ocorreu um erro inesperado.");
        }
    }

    private static PostgresException? ExtrairViolacaoUnica(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg ? pg : null;

    private static async Task EscreverProblema(
        HttpContext context, int status, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
