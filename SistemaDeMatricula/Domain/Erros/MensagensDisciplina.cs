namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensDisciplina
    {
        public const string NomeObrigatorio = "O nome da disciplina é obrigatório.";
        public const string CargaHorariaInvalida = "A carga horária deve ser positiva.";
        public const string DisciplinaNaoEncontrada = "Disciplina não encontrada.";
        public const string DisciplinaJaExiste = "Disciplina já existe.";
        public const string DisciplinaInativa = "Disciplina está inativa.";
        public const string DisciplinaAtiva = "Disciplina está ativa.";
        public const string DisciplinaNaoPodeSerRemovida = "Disciplina não pode ser removida.";
        public const string DisciplinaNaoPodeSerAtualizada = "Disciplina não pode ser atualizada.";
        public const string DisciplinaInvalida = "Disciplina inválida.";
        public const string DisciplinaNaoPodeSerDesativada = "Disciplina não pode ser desativada.";
        public const string ErroDoBancoDeDados = "Erro ao tentar salvar no banco de dados.";
        public const string desativarDisciplina = "Disciplina desativada com sucesso!";
    }
}