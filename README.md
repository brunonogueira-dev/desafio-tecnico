# 🚌 OniBus Express

Sistema de venda de passagens rodoviárias — backend em **.NET 8** + frontend em
**React 18/TypeScript**, tudo orquestrado com **Docker**.

> Enunciado original do desafio: [`DESAFIO.md`](DESAFIO.md).

---

## Stack e por quê

| Camada | Tecnologia | Motivo |
|--------|-----------|--------|
| Backend | .NET 8 + ASP.NET Core Web API | Plataforma pedida; arquitetura em camadas com regras isoláveis e testáveis |
| Persistência | EF Core + PostgreSQL | ORM maduro; Postgres permite o **índice único parcial** que garante o assento |
| Erros | Result + ProblemDetails (RFC 7807) | Erro de negócio como dado, não exceção; resposta HTTP padronizada |
| Frontend | React 18 + TypeScript + Vite | SPA rápida e tipada |
| Estado | TanStack Query + Zustand | Cache de servidor separado do estado do fluxo de compra |
| Forms | react-hook-form + zod | Validação declarativa e testável (mesmo CPF do backend) |
| Testes | xUnit + FluentAssertions + Testcontainers · Vitest + RTL + MSW | Testa comportamento, incluindo concorrência real de assento |
| Infra | Docker + docker-compose + nginx | Sobe tudo com um comando |

## Como rodar

### Com Docker (um comando)

```bash
cp .env.example .env   # opcional; há defaults de dev
docker compose up --build
```

- Frontend: <http://localhost:3000>
- API (Swagger UI): <http://localhost:8080>
- A API aplica **migrations + seed** no startup (ambiente Development).

### Sem Docker

Backend:
```bash
cd backend
dotnet run --project src/OnibusExpress.Api
```
Frontend:
```bash
cd frontend
npm ci && npm run dev
```

## Estrutura

```
backend/
  src/OnibusExpress.Domain/          # entidades, VOs, regras, exceções
  src/OnibusExpress.Application/      # use cases, DTOs, Result, interfaces
  src/OnibusExpress.Infrastructure/  # EF Core, repositórios, migrations, seeder
  src/OnibusExpress.Api/             # controllers, ProblemDetails, Swagger
  tests/                             # unitários + integração (Testcontainers)
frontend/
  src/{components,pages,services,store,lib}
docker-compose.yml
```

Regra de dependência: **Api → Application → Domain**, **Infrastructure → Application**.
O Domain não referencia nada externo.

## Decisões de arquitetura

- **Índice único parcial** `(ViagemId, NumeroAssento) WHERE Status='Confirmada'`:
  é o **banco** que garante que dois pedidos simultâneos não reservem o mesmo
  assento. A checagem em código só existe para mensagem amigável; a violação
  vira **409**.
- **`IDateTimeProvider`** injetado: nada de `DateTime.Now` solto — regras de
  "viagem já partiu" e "prazo de 2h" são testáveis com relógio fixo.
- **Assentos livres derivados** (`Total − reservas confirmadas`): sem contador
  mutável, elimina inconsistência.
- **`Result<T>`** em vez de exceção para erro de negócio.
- **Sem MediatR/AutoMapper/Repository genérico**: a escala do MVP não justifica.

## Contrato da API

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/rotas` | Lista rotas |
| GET | `/viagens?origem=&destino=&data=` | Busca viagens com vagas |
| GET | `/viagens/{id}` | Detalhe + mapa de assentos |
| POST | `/reservas` | Cria reserva (201 + Location) |
| GET | `/reservas/{codigo}` | Consulta reserva |
| DELETE | `/reservas/{codigo}` | Cancela (soft) |

## Regras de negócio

1. Assento ocupado por reserva confirmada não pode ser reservado.
2. Não reservar viagem já partida.
3. CPF validado por formato e dígito verificador.
4. Código de reserva único e legível (`AAA-99999`, sem caracteres ambíguos).
5. Cancelamento só até 2h antes da partida.

## Testes

```bash
cd backend && dotnet test        # unitários + integração (integração exige Docker)
cd frontend && npm test          # Vitest + RTL + MSW
```

## Variáveis de ambiente

Veja [`.env.example`](.env.example). O `.env` real não é versionado.
