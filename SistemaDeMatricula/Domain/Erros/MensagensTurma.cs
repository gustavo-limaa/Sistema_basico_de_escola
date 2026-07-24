namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensTurma
    {
        public const string Invalida = "Turma inválida.";
        public const string TurmaNaoEncontrada = "Turma não encontrada.";
        public const string TurmaJaDesativada = "Turma já está desativada.";
        public const string TurmaComAlunosMatriculados = "Não é possível desativar uma turma com alunos matriculados.";
        public const string ErroPersistenciaBanco = "Ocorreu um erro ao persistir a turma no banco de dados.";
        public const string TurmaJaExistente = "Já existe uma turma com este código.";
        public const string CodigoTurmaObrigatorio = "O código da turma é obrigatório.";
        public const string TurmaLotada = "Turma lotada! Capacidade máxima atingida.";
    }
}