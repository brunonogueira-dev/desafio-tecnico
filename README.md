# OniBus Express — Frontend

SPA de venda de passagens rodoviárias. React 18 + TypeScript + Vite.

> Repositório do frontend. A API vive em um repositório separado (`../backend`).

## Stack

- **React 18 + TypeScript (strict)** + **Vite**
- **TanStack Query** para estado de servidor (cache de rotas/viagens/reserva)
- **Zustand** para o estado do fluxo de compra (viagem e assento escolhidos)
- **react-hook-form + zod** para o formulário de passageiro
- **Vitest + Testing Library + MSW** para os testes
- **nginx** para servir em produção, com a URL da API injetada em runtime

## Como rodar

### Desenvolvimento

```bash
cp .env.example .env   # ajuste VITE_API_BASE_URL se necessário
npm install
npm run dev
```

A aplicação sobe em `http://localhost:5173` e consome a API em `VITE_API_BASE_URL`
(padrão `http://localhost:8080`).

### Com Docker

A imagem nginx recebe a URL da API em runtime pela variável `API_BASE_URL`:

```bash
docker build -t onibus-web .
docker run -p 3000:80 -e API_BASE_URL=http://localhost:8080 onibus-web
```

O jeito recomendado é subir tudo junto pelo `docker-compose` do backend (veja o
README do backend), que já orquestra banco, API e frontend.

## Telas

1. **Busca** — origem, destino e data; lista de viagens com preço, duração e vagas.
2. **Assentos** — mapa em layout de ônibus com estados livre/ocupado/selecionado.
3. **Passageiro** — formulário validado (CPF com dígito verificador) e resumo.
4. **Consulta** — busca por código, detalhes e cancelamento.

## Testes

```bash
npm test
```
