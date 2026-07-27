# 🚌 DESAFIO TÉCNICO
## Desenvolvedor Full Stack
**.NET • ReactJS • Docker • Testes**

# Projeto: OniBus Express
### Sistema de Venda de Passagens Rodoviárias

---

## 1. Sobre o Desafio

Bem-vindo ao desafio técnico da OniBus Express! Este desafio foi criado para avaliar suas habilidades práticas no desenvolvimento de software, cobrindo desde o design de APIs até a construção de interfaces modernas.

Você pode entregar somente o Backend, somente o Frontend, ou ambos. Entregar as duas partes é considerado um diferencial e será avaliado positivamente na pontuação final.

Não esperamos perfeição — queremos entender como você pensa, organiza o código e resolve problemas. Documente suas decisões e, se não terminar tudo, explique o que faria diferente com mais tempo.

---

## 2. Contexto do Projeto

A OniBus Express é uma empresa de transporte rodoviário que precisa modernizar seu sistema de vendas. Você foi contratado para construir o MVP (Produto Mínimo Viável) do novo sistema, que deve permitir a busca e compra de passagens de ônibus online.

### Entidades Principais

| Entidade | Campos |
|---|---|
| **Rota** | Origem, destino, duração estimada |
| **Viagem** | Rota associada, data/hora de partida, preço base, assentos disponíveis |
| **Passageiro** | Nome, CPF, e-mail, data de nascimento |
| **Reserva/Passagem** | Viagem, passageiro, número do assento, status, código de reserva |

---

## 3. Requisitos do Backend (.NET)

> Entregue esta parte se o foco da sua vaga é **Back-End** ou **Full Stack**.

### 3.1 Tecnologias Obrigatórias

- .NET 8+ (ASP.NET Core Web API)
- Entity Framework Core com banco relacional (PostgreSQL ou SQL Server)
- Docker + docker-compose para subir o ambiente
- Testes automatizados (xUnit ou NUnit)

### 3.2 Funcionalidades Requeridas

Endpoints mínimos esperados:

```
GET    /rotas               — Listar todas as rotas disponíveis
GET    /viagens             — Buscar viagens por origem, destino e data
GET    /viagens/{id}        — Detalhes de uma viagem (assentos livres/ocupados)
POST   /reservas            — Criar reserva (nome, CPF, e-mail, viagem, assento)
GET    /reservas/{codigo}   — Consultar reserva pelo código gerado
DELETE /reservas/{codigo}   — Cancelar reserva
```

### 3.3 Regras de Negócio

- Não deve ser possível reservar um assento já ocupado
- Não deve ser possível reservar passagem para viagem já realizada
- CPF deve ser validado (formato e dígito verificador)
- O código de reserva deve ser único e legível (ex: `ABC-12345`)
- Cancelamento só permitido até **2 horas** antes da partida

### 3.4 Requisitos de Testes

Esperamos ao menos cobertura de testes unitários e/ou de integração para:

- Validação do CPF
- Regra de assento já ocupado
- Regra de cancelamento dentro do prazo
- Geração do código de reserva único

> **Dica:** use um banco em memória (SQLite in-memory ou TestContainers) para os testes de integração. Não é necessário testar cada linha, mas mostre que você sabe onde os testes agregam valor.

### 3.5 Docker

O projeto deve ter um arquivo `docker-compose.yml` que suba toda a aplicação com um único comando:

```bash
docker-compose up --build
```

Inclua: API, banco de dados e, opcionalmente, migration automática ao iniciar.

---

## 4. Requisitos do Frontend (ReactJS)

> Entregue esta parte se o foco da sua vaga é **Front-End** ou **Full Stack**.

### 4.1 Tecnologias Obrigatórias

- React 18+ com TypeScript
- Gerenciador de estado a sua escolha (Context API, Zustand, Redux, etc.)
- Testes com React Testing Library + Jest ou Vitest
- Docker para servir a aplicação (Nginx ou similar)

### 4.2 Telas Requeridas

#### Tela 1 — Busca de Passagens
- Formulário com: Origem, Destino, Data de ida
- Botão de buscar
- Listagem de viagens disponíveis com preço, horário e vagas restantes
- Estado de loading e mensagem quando não há resultados

#### Tela 2 — Seleção de Assento
- Mapa visual dos assentos (livre / ocupado / selecionado)
- Exibir informações da viagem: rota, data, hora, preço
- Botão para prosseguir com o assento selecionado

#### Tela 3 — Dados do Passageiro e Confirmação
- Formulário: Nome completo, CPF, E-mail
- Validação dos campos no frontend
- Resumo da compra antes de confirmar
- Tela de sucesso com código da reserva após confirmação

#### Tela 4 (Bonus) — Consulta de Reserva
- Campo para digitar o código da reserva
- Exibir detalhes ou opção de cancelamento

### 4.3 Requisitos de Testes

Esperamos ao menos:

- Teste do componente de busca (simula preenchimento e busca)
- Teste do mapa de assentos (seleção, bloqueio de assentos ocupados)
- Teste de validação do formulário de passageiro

> **Dica:** prefira testar comportamento do usuário (clicar, preencher, ver resultado) em vez de detalhes de implementação. Mock da API é permitido e recomendado nos testes.

---

## 5. O Que Será Avaliado

| Critério | Pontuação |
|---|---|
| Backend: Funcionalidades implementadas corretamente | 0 – 25 pts |
| Backend: Qualidade do código (estrutura, SOLID, organização) | 0 – 15 pts |
| Backend: Testes automatizados | 0 – 15 pts |
| Backend: Docker funcionando com um comando | 0 – 5 pts |
| Frontend: Telas e fluxo implementados | 0 – 25 pts |
| Frontend: Qualidade dos componentes e UX | 0 – 10 pts |
| Frontend: Testes de componentes | 0 – 10 pts |
| Frontend: Docker para servir a aplicação | 0 – 5 pts |
| **PLUS:** Entrega Backend + Frontend integrados | + 15 pts |
| **PLUS:** README completo e bem escrito | + 5 pts |
| **PLUS:** Swagger/OpenAPI no backend | + 5 pts |
| **PLUS:** Tratamento de erros e feedback visual (frontend) | + 5 pts |

> Candidatos que entregarem apenas uma parte (backend **OU** frontend) serão avaliados somente nos critérios relevantes. Não há penalidade por não entregar as duas partes.

---

## 6. Requisitos do README

O `README.md` deve conter **obrigatoriamente**:

- Como rodar o projeto localmente (com e sem Docker)
- Quais tecnologias e bibliotecas foram usadas e por quê
- Decisões de arquitetura relevantes
- O que foi implementado e o que ficou de fora
- Como rodar os testes

Será considerado um **plus**:

- Screenshots ou gif da aplicação rodando
- Documentação dos endpoints (ou link para o Swagger)
- Pontos de melhoria que você implementaria com mais tempo

---

## 7. Estrutura Sugerida do Projeto

Não é obrigatória, mas recomendamos:

```
onibus-express/
  backend/                          # API .NET
    src/OnibusExpress.Api/
    src/OnibusExpress.Application/
    src/OnibusExpress.Domain/
    src/OnibusExpress.Infrastructure/
    tests/OnibusExpress.Tests/
    Dockerfile
  frontend/                         # App React
    src/components/
    src/pages/
    src/services/
    src/__tests__/
    Dockerfile
  docker-compose.yml
  README.md
```

---

## 8. Entrega

### 8.1 Como Entregar

1. Crie um repositório público no GitHub (ou GitLab)
2. Faça commits regulares — seu histórico de git também será analisado
3. Envie o link do repositório para o e-mail de recrutamento
4. Inclua no e-mail: qual parte(s) entregou e observações relevantes

### 8.2 Prazo

Você tem **5 dias corridos** a partir do recebimento deste documento para entregar o desafio. Se precisar de mais tempo, nos avise com antecedência — somos flexíveis para casos justificados.

### 8.3 O que Não Será Aceito

- Projeto que não roda após seguir o README
- Código copiado de tutoriais sem adaptação ou compreensão
- README ausente ou sem informações mínimas
- Segredos ou credenciais sensíveis commitados no repositório

---

## 9. Dicas Finais

> Você não precisa implementar tudo. Priorize **qualidade sobre quantidade**. Um backend limpo com testes vale mais do que um sistema completo sem estrutura.

- Leia o desafio completo antes de começar a codar
- Comece pelo que você tem mais domínio
- Testes são tão importantes quanto o código de produção
- Se travar em algo, documente e siga em frente
- Commits pequenos e frequentes mostram seu raciocínio
- Use nomes de variáveis, métodos e componentes que se expliquem

---

**Boa sorte! Estamos ansiosos para ver sua solução.**

Qualquer dúvida sobre o desafio, entre em contato pelo e-mail de recrutamento.
