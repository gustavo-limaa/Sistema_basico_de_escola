namespace SistemaDeMatricula.Domain.Erros
{
    public class MensagensProfessor
    {
        public const string ProfessorNaoEncontrado = "Professor não encontrado.";
        public const string ProfessorJaExiste = "Professor já existe.";
        public const string ProfessorInvalido = "Professor inválido.";
        public const string ErroSemAutoridade = "voce nao tem permiçao para fazer essa açao";
        public const string ErroInativo_ou_Ativo = "Professor ja esta desativado ou ativado,favor chegar pelo barra de buscas";
        public const string ErroDeDuplicidade = "Os Dados Nao Podem ser Duplicado Ou Retirado De Seus Donos A Força";
        public const string FalhaAoPersistirDados = "Falha ao persistir dados no banco de dados.";
    }
}