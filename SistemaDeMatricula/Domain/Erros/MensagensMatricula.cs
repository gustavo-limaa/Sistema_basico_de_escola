namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensMatricula
    {
        public const string MatriculaNaoEncontrada = "Matrícula não encontrada.";
        public const string MatriculaJaDesativada = "Matrícula já está desativada.";
        public const string MatriculaDesativadaComSucesso = "Matrícula desativada com sucesso.";
        public const string MatriculaJaExistente = "Este estudante já está matriculado nesta turma.";
        public const string ErroPersistenciaBanco = "Ocorreu um erro ao persistir a matrícula no banco de dados.";
        public const string ErroTecnico = "Ocorreu um erro técnico ao processar a matrícula.";
    }
}