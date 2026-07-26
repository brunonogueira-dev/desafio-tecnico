# OniBus Express — Backend

API REST para venda de passagens rodoviárias. .NET 8 + ASP.NET Core + EF Core + PostgreSQL.

> Repositório do backend. O frontend vive em um repositório separado (`../frontend`).
> Este README é preenchido incrementalmente ao longo do desenvolvimento.

## Stack

_(preenchido na entrega, com justificativa de cada escolha)_

## Como rodar

### Com Docker (um comando)

```bash
cp .env.example .env
docker compose up --build
```

### Sem Docker

_(passo a passo — dotnet ef database update, dotnet run)_

## Variáveis de ambiente

Veja [`.env.example`](.env.example).

## Arquitetura

_(decisões: índice único parcial, IDateTimeProvider, assentos derivados, Result, sem MediatR/AutoMapper)_

## Endpoints

_(tabela + link para o Swagger)_

## Regras de negócio

_(as 5 regras e onde cada uma é testada)_

## Testes

_(como rodar; integração usa Testcontainers e exige Docker)_

## O que ficou de fora / melhorias futuras

_(honestidade sobre escopo)_
