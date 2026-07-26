# OniBus Express — Backend

API REST para venda de passagens rodoviárias, construída em **.NET 8 / ASP.NET Core** com **EF Core** e **PostgreSQL**, seguindo arquitetura em camadas (Domain, Application, Infrastructure, Api).

> Este é o repositório do **backend**. O frontend vive em um repositório separado (`../frontend`).
> O `docker-compose.yml` deste repositório orquestra os três serviços (banco, API e frontend)
> para a demonstração integrada — o frontend é construído a partir da pasta irmã `../frontend`.

---

## Stack e por quê

| Escolha | Motivo |
|---|---|
| **.NET 8 + ASP.NET Core Web API** | Requisito do desafio; runtime LTS. |
| **PostgreSQL** | Suporta **índice único parcial**, peça central da garantia de concorrência de assento (SQLite não suporta). |
| **EF Core** | Produtividade com controle: conversores de value object, índices parciais via `HasFilter`, migrations. |
| **Arquitetura em 4 camadas** | Domain sem dependências; regras de negócio isoladas e testáveis. |
| **Result em vez de exceção** | Erro de negócio é valor previsível; exceção fica reservada para falha inesperada. |
| **xUnit + FluentAssertions + Testcontainers** | Testes de unidade rápidos e de integração contra um Postgres real. |
| **Serilog** | Log estruturado. |
| Sem **MediatR / AutoMapper / repositório genérico** | Escala de MVP não justifica a abstração; menos indireção, avaliação de qualidade mais limpa. |

---

## Como rodar

### Com Docker (um comando)

Pré-requisito: Docker em execução. Para subir **os três serviços** (banco + API + frontend),
tenha o repositório `frontend` como pasta irmã (`../frontend`) e rode a partir de `backend/`:

```bash
cp .env.example .env
docker compose up --build
```

- API + Swagger: <http://localhost:8080/> (Swagger UI na raiz)
- Frontend: <http://localhost:3000>
- Health check: <http://localhost:8080/health>

Em `Development` (padrão do compose) a API aplica **migrations e seed automaticamente** no startup.

> Para subir só banco + API, rode `docker compose up --build postgres api`.

### Sem Docker

Pré-requisitos: .NET 8 SDK e um PostgreSQL acessível.

```bash
# 1. Suba um Postgres (ou use o do compose)
docker compose up -d postgres

# 2. Configure a connection string (não versionada)
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=onibus_express;Username=onibus;Password=onibus_dev_pwd"

# 3. Aplique as migrations
dotnet ef database update --project src/OnibusExpress.Infrastructure --startup-project src/OnibusExpress.Api

# 4. Rode a API (em Development ela também aplica migrations + seed)
dotnet run --project src/OnibusExpress.Api
```

---

## Variáveis de ambiente

Documentadas em [`.env.example`](.env.example). O `.env` real fica fora do versionamento.
A connection string **nunca** é gravada em `appsettings` versionado — vem de
`ConnectionStrings__Postgres` (montada pelo compose a partir das variáveis do Postgres).

---

## Decisões de arquitetura

- **Índice único parcial em vez de checagem em código.**
  A unicidade de assento é garantida pelo banco: índice único em
  `(ViagemId, NumeroAssento) WHERE Status = 'Confirmada'`
  ([`ReservaConfiguration`](src/OnibusExpress.Infrastructure/Persistence/Configurations/ReservaConfiguration.cs)).
  A checagem em código existe só para a mensagem amigável; sob concorrência, o banco é a fonte da verdade e a
  violação (`23505`) é traduzida em **409** no
  [`ExceptionHandlingMiddleware`](src/OnibusExpress.Api/Middleware/ExceptionHandlingMiddleware.cs).
  Reservas canceladas saem do índice (filtro parcial), liberando o assento.

- **`IDateTimeProvider` injetado.**
  Nenhuma regra lê `DateTime.UtcNow` direto. As regras dependentes de tempo (viagem já partiu, prazo de 2h) usam
  [`IDateTimeProvider`](src/OnibusExpress.Domain/Abstractions/IDateTimeProvider.cs); os testes injetam um relógio fixo.

- **Assentos derivados, não contador.**
  Vagas = `TotalAssentos − reservas confirmadas`. Não existe coluna contador mutável, eliminando uma classe inteira
  de bug de consistência.

- **`Result<T>` em vez de exceção para erro de negócio.**
  A Application retorna [`Result`](src/OnibusExpress.Application/Common/Result.cs) com `ErrorCode` tipado; a Api
  traduz para status HTTP e **ProblemDetails** (RFC 7807).

- **Value Objects com invariante no construtor.**
  [`Cpf`](src/OnibusExpress.Domain/ValueObjects/Cpf.cs) (dígito verificador) e
  [`CodigoReserva`](src/OnibusExpress.Domain/ValueObjects/CodigoReserva.cs) (formato `AAA-99999`, alfabeto sem
  caracteres ambíguos, RNG criptográfico) são imutáveis e impossíveis de existir inválidos.

- **Por que NÃO usei MediatR/AutoMapper/repositório genérico:** ver tabela de stack acima.

---

## Endpoints

| Método | Rota | Descrição | Sucesso | Erros |
|---|---|---|---|---|
| GET | `/rotas` | Lista as rotas | 200 | — |
| GET | `/viagens?origem=&destino=&data=` | Busca viagens | 200 | 400 |
| GET | `/viagens/{id}` | Detalhe + mapa de assentos | 200 | 404 |
| POST | `/reservas` | Cria reserva | 201 (+ Location) | 400, 404, 409 |
| GET | `/reservas/{codigo}` | Consulta reserva | 200 | 404 |
| DELETE | `/reservas/{codigo}` | Cancela reserva | 204 | 404, 409 |

Documentação interativa: **Swagger UI** na raiz (`/`), com exemplos de request/response.

---

## Regras de negócio e onde estão testadas

| Regra | Onde vive | Teste |
|---|---|---|
| Assento confirmado não pode ser reservado de novo | índice parcial + `CriarReservaHandler` | `AssentoConcorrenciaTests` (unit: `CriarReservaHandlerTests`) |
| Não reservar viagem já partida | `Viagem.JaPartiu` | `FluxoCompletoTests`, `ViagemTests` |
| CPF validado por formato e dígito verificador | `Cpf` (VO) | `CpfTests`, `ReservaCpfTests` |
| Código de reserva único e legível `AAA-99999` | `CodigoReserva` (VO) | `CodigoReservaTests`, `CodigoReservaUnicoTests` |
| Cancelamento só até 2h antes | `Reserva.PodeSerCancelada` | `ReservaTests`, `CancelamentoTests` |

O **teste de concorrência** (`AssentoConcorrenciaTests`) dispara 10 requisições paralelas no mesmo assento e verifica
que exatamente uma retorna 201 e nove retornam 409 — a prova de que o índice único parcial resolve a corrida.

---

## Testes

```bash
# Unidade (Domain + Application) — rápido, sem banco
dotnet test tests/OnibusExpress.UnitTests

# Integração (endpoints + regras 3.4) — sobe um Postgres via Testcontainers (exige Docker)
dotnet test tests/OnibusExpress.IntegrationTests

# Tudo
dotnet test
```

Os testes de integração usam **Testcontainers**, portanto exigem **Docker em execução**.

---

## O que ficou de fora / melhorias com mais tempo

- Autenticação/autorização (o desafio é MVP público).
- Paginação em `/viagens`.
- Idempotência no `POST /reservas` (chave de idempotência).
- Expiração de reserva pendente com fluxo de pagamento.
- Observabilidade (OpenTelemetry) e CI (GitHub Actions).
- Busca por data considerando fuso local do cliente (hoje a comparação é por dia UTC).
