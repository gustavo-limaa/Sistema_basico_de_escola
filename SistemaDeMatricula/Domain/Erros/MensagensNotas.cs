namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensNotas
    {
        public const string NotaInvalida = "A nota deve estar entre 0 e 10.";
        public const string NotaNaoEncontrada = "Nota não encontrada.";
        public const string NotaJaExiste = "A nota já existe para este aluno e disciplina.";
        public const string DescricaoNaoPodeSerVazia = "Descrição não pode ser vazia.";
        public const string MatriculaIdNaoPodeSerVazio = "MatriculaId não pode ser vazio.";
        public const string TipoImportanciaInvalido = "Tipo de importância inválido.";
        public const string DataEmissaoInvalida = "Data de emissão inválida.";
        public const string CategoriaAvaliacaoInvalida = "Categoria de avaliação inválida.";
        public const string NotaNaoPodeSerAlterada = "Nota não pode ser alterada.";
        public const string NotaNaoPodeSerExcluida = "Nota não pode ser excluída.";
        public const string ErroBancoDeDados = "Erro ao acessar o banco de dados.";
    }
}