# 🎓 Sistema Básico de Escola

Este é um ecossistema de back-end robusto e distribuído desenvolvido em **C# e .NET**, focado na gestão escolar e segurança de dados. O sistema foi construído aplicando os princípios mais modernos de engenharia de software para garantir isolamento de escopo, escalabilidade e resiliência através de um desenvolvimento rigorosamente orientado a testes (**TDD**).

🚀 Status do Projeto: Fase de Autorização & Segurança
O núcleo de gestão escolar e o módulo de autenticação estão implementados e integrados.

* **Foco Atual:** Implementação do módulo de **Autorização baseada em Roles/Claims** (Controle de acesso granular para Admin, Professor e Aluno).
* **Próxima etapa:** Integração do Middleware JWT na API Primária e fechamento do fluxo de Matrículas.

---

## 🛠️ Tecnologias, Ferramentas e Práticas

### 🖥️ Ecossistema Core & APIs
* **Linguagem & Framework:** C# e .NET (Minimal APIs / Controllers).
* **Arquitetura:** Clean Architecture / Domain-Driven Design (DDD).
* **Mensageria:** RabbitMQ para comunicação assíncrona entre serviços.

### 🔐 Segurança & Autenticação (Novo!)
* **ASP.NET Core Identity:** Gerenciamento nativo de usuários, hashes de senha e segurança de credenciais.
* **JWT (JSON Web Tokens):** Emissão de tokens dinâmicos assinados com chaves simétricas de criptografia (`HMAC-SHA256`).
* **DotNetEnv:** Isolamento de chaves simétricas e segredos do sistema através de arquivos `.env` centralizados.

### 🧪 Qualidade de Código & Testes Avançados
* **xUnit & FluentAssertions:** Framework principal e asserções expressivas legíveis.
* **Moq:** Isolamento completo de dependências externas (como mocks de produtores RabbitMQ).
* **Bogus:** Geração de massa de dados realista e randômica.
* **Respawn (Foco em Integração):** Reset inteligente e ultra-rápido do estado do banco de dados MySQL (`Testes de Integração`) limpando tabelas entre execuções sem corromper o histórico de migrations.

---

## ✅ Evidência de Qualidade & Cobertura

A suíte de testes do projeto foi expandida para cobrir de forma holística tanto as regras de domínio isoladas quanto os fluxos de persistência reais.

* **Testes Unitários:** Validação de Value Objects (VOs), Entidades e Exceções de Negócio.
* **Testes de Integração:** Fábrica de testes (`WebApplicationFactory`) configurada em memória para injetar variáveis de ambiente isoladas (`JWT_KEY`) e bancos de dados de teste dedicados, garantindo a execução de **mais de 280 cenários de ponta a ponta**.

> ⚠️ **Blindagem de Infraestrutura:** O ambiente de testes de integração possui injeção em memória RAM de credenciais fakes, tornando a esteira de testes 100% segura contra vazamento de segredos e pronta para rodar em ambientes de nuvem (CI/CD).

---

## 🏗️ Destaques da Implementação

* **Autenticação Dinâmica:** Rota de registro e login com persistência direta via Entity Framework no banco `Identity_DB`, gerando crachás de acesso seguros em tempo de execução.
* **Persistência Avançada:** Entity Framework Core configurado com suporte a `Soft Delete` e `IgnoreQueryFilters` para auditoria e restauração de dados deletados.
* **Terminologia Internacional:** Priorização estrita de termos em Inglês para commits (Conventional Commits) e documentação técnica, alinhando o repositório com padrões globais de mercado.

---

## 📈 Road Map

- [x] CRUDs e Domínio de Professores, Disciplinas, Estudantes e Turmas.
- [x] Arquitetura e Configuração do Serviço de Autenticação (Identity + JWT).
- [x] Centralização de Configurações Dinâmicas com `.env` e variáveis de ambiente.
- [x] Cobertura de 280+ Testes (Unitários e Integração com Respawn).
- [x] Finalização das Regras de Negócio do Módulo de Matrícula.
- [ ] Implementação de Controle de Acesso por Roles/Claims (Em progresso).
- [ ] Configuração de CI/CD via GitHub Actions.

---

**Desenvolvido por Zander Gustavo** ([gustavo-limaa](https://github.com/gustavo-limaa))

![Suíte de Testes](docs/testes-sucesso.png)
