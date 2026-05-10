using SistemaDeMatricula.Aplicacao.Dtos.estudante;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SistemaDeMatricula.Domain.Mapper;

public static class EstudanteMapper
{
    public static EstudanteDtoResponse ToEstudanteDtoResponse(this Estudante estudante)
    {
        return new EstudanteDtoResponse(
            estudante.Id,
            estudante.NomeCompleto.Valor,
            estudante.Email.Valor,
            estudante.DataNascimento.Valor,
            estudante.Cpf.Valor,
            estudante.Telefone.Valor
        );
    }

    public static Estudante ToEstudante(this EstudanteDtoCreate estudanteDtoCreate)
    {
        return new Estudante(
            Guid.NewGuid(),
            new ObjectNomeCompleto(estudanteDtoCreate.NomeCompleto),
            new ObjectDataNascimento(estudanteDtoCreate.DataNascimento),
            new ObjectCPF(estudanteDtoCreate.Cpf),
            new ObjectEmail(estudanteDtoCreate.Email),
            new ObjectTelefone(estudanteDtoCreate.Telefone)
        );
    }

    public static Estudante ToUpdateEstudante(this EstudanteDtoUpdate dto, Estudante estudanteExistente)
    {
        estudanteExistente.AtualizarDados(
            new ObjectNomeCompleto(dto.NomeCompleto),
            new ObjectEmail(dto.Email),
            new ObjectDataNascimento(dto.DataNascimento),
            new ObjectTelefone(dto.Telefone)
        );

        return estudanteExistente;
    }

    public static EstudanteDtoList ToListDto(this Estudante estudante)
    {
        return new EstudanteDtoList(
            estudante.Id,
            estudante.NomeCompleto.Valor,
            estudante.Email.Valor
        );
    }
}