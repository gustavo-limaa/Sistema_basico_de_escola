namespace SitemaDeMatricula.Aplicacao.Dtos.turma;
// O que a API vai devolver quando alguém pedir os dados da turma
// Note que aqui já podemos pensar em devolver nomes, para facilitar a vida do Front-end
public record TurmaDtoResponse(
    Guid Id,
    string Codigo,
    string NomeDisciplina,
    string NomeProfessor,
    bool Ativo
);