namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensEstudante
    {
        public const string ErroEstudanteInvalido = "Estudante inválido.";
        public const string ErroEstudanteNaoEncontrado = "Estudante não encontrado.";
        public const string EstudanteJaExiste = "Estudante já existe.";
        public const string ErroEstudanteIdInvalido = "Estudante identificador inválido.";
        public const string ErroEstudanteNaoPodeTerCpfFalso = "O estudante não pode ter um CPF falso.";
        public const string ErroEstudanteNaoPodeTerEmailFalso = "O estudante não pode ter um email falso.";
        public const string ErroEstudanteNaoPodeTerTelefoneFalso = "O estudante não pode ter um telefone falso.";
        public const string ErroEstudanteNaoPodeTerNomeFalso = "O estudante não pode ter um nome falso.";
        public const string ErroEstudanteNaoPodeTerDataNascimentoFalsa = "O estudante não pode ter uma data de nascimento falsa.";
        public const string EstudanteNaoPodeTerMatriculaDuplicada = "O estudante não pode ter uma matrícula duplicada.";
    }
}