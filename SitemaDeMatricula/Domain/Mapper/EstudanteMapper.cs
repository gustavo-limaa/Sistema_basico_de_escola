using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;
using SitemaDeMatricula.Domain.Value_Objetc;

namespace SitemaDeMatricula.Domain.Mapper;

public static class EstudanteMapper
{
    public static EstudanteDtoResponse ToEstudanteDtoResponse(this Estudante estudante)
    {
        return new EstudanteDtoResponse(
            estudante.EstudanteId,
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

    // Dentro da sua classe de Mapper ou no DTO
    public static Estudante ToUpdateEstudante(this EstudanteDtoUpdate dto, Estudante estudanteExistente)
    {
        // Aqui você não dá um "new Estudante", você atualiza o que já existe
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
            estudante.EstudanteId,
            estudante.NomeCompleto.Valor,
            estudante.Email.Valor
        );
    }
}