namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensProfessor
    {
        public const string ProfessorNaoEncontrado = "Professor não encontrado.";
        public const string ProfessorJaExiste = "Professor já existe.";
        public const string ProfessorInvalido = "Professor inválido.";
        public const string ProfessorNaoPodeSerRemovido = "Professor não pode ser removido.";
        public const string ProfessorNaoPodeSerAtualizado = "Professor não pode ser atualizado.";
        public const string ProfessorJaDesativado = "Professor já está desativado.";
        public const string ProfessorNaoPodeSerAdicionado = "Professor não pode ser adicionado.";
        public const string ProfessorNaoPodeSerAtivado = "Professor não pode ser ativado.";
        public const string ProfessorNaopodeTersalarioNegativo = "Professor não pode ter salário negativo.";
        public const string ProfessorNaoPodeTerCategoriaInvalida = "Professor não pode ter categoria inválida.";
        public const string ProfessorNaoPodeTerDataNascimentoInvalida = "Professor não pode ter data de nascimento inválida.";
        public const string ProfessorNaoPodeTerTelefoneInvalido = "Professor não pode ter telefone inválido.";
        public const string ProfessorNaoPodeTerEmailInvalido = "Professor não pode ter email inválido.";
        public const string ProfessorNaoPodeTerCpfInvalido = "Professor não pode ter CPF inválido.";
        public const string ProfessorNaoPodeTerNomeInvalido = "Professor não pode ter nome inválido.";
        public const string ProfessorJaAtivo = "Professor já está ativo.";
        public const string FalhaAoPersistirDados = "Falha ao persistir dados no banco de dados.";
    }
}