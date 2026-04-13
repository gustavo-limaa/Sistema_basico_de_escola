🎓 Sistema Básico de Escola
Este é um projeto de API para gerenciamento escolar, desenvolvido com o objetivo de aplicar conceitos avançados de Clean Architecture, Domain-Driven Design (DDD) e C#. O sistema foca em um fluxo robusto, utilizando o Result Pattern para garantir a estabilidade e previsibilidade das respostas.

🚀 Tecnologias Utilizadas
Linguagem: C# (.NET 8/9)

Arquitetura: Clean Architecture (Domínio, Aplicação, Infraestrutura e Apresentação)

Banco de Dados: Entity Framework Core (SQL Server)

Padronização: Result Pattern para controle de fluxo e erros.

Auxiliares: Bogus (Dados Fakes) e Injeção de Dependência nativa.

🏗️ Estrutura do Projeto
Domain: Entidades ricas (Estudante, Professor, Turma), Value Objects e interfaces.

Application: Casos de Uso (Use Cases) que isolam a lógica da aplicação.

Infrastructure: Persistência de dados, Repositórios e Configurações de Banco.

Presentation (API): Controllers desacoplados e focados em contratos HTTP.

🛠️ Módulos do Sistema
👤 Gestão de Estudantes
CRUD completo utilizando DTOs de entrada e saída.

Validações de domínio para garantir dados íntegros desde a criação.

👨‍🏫 Gestão de Professores (Próxima Fase)
Cadastro de docentes e suas especialidades.

📚 Disciplinas e Turmas (Em Desenvolvimento)
Vinculação de professores às suas respectivas disciplinas.

Organização de turmas, unindo alunos e conteúdos por ano letivo.

📖 Como Rodar o Projeto
Clone o repositório:

Bash
git clone https://github.com/gustavo-limaa/Sistema_basico_de_escola.git
Restaure as dependências:

Bash
dotnet restore
Execute a aplicação:

Bash
dotnet run --project SitemaDeMatricula.Percistencia
💡 Roadmap / Backlog
[x] CRUD de Estudante com Clean Architecture.

[x] Implementação de Result Pattern.

[ ] Módulo de Professores: Cadastro e especialidades.

[ ] Módulo de Disciplinas: Vinculação com professores e carga horária.

[ ] Módulo de Turmas: Gestão de alunos e anos letivos.

[ ] Registro de Logs com Serilog.
