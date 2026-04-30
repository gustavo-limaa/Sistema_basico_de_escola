using SitemaDeMatricula.Aplicacao.Dtos.turma;
using SitemaDeMatricula.Domain.Modelos;
using SitemaDeMatricula.Domain.Value_Object;

public static class TurmaMapper
{
    // 1. MAPEAMENTO DE SAÍDA (Para o Usuário)
    public static TurmaDtoResponse ToTurmaDtoResponse(this Turma turma)
    {
        // Não precisamos "Criar" o código aqui, ele já existe na entidade!
        // Apenas usamos as propriedades dele.
        return new TurmaDtoResponse(
            turma.TurmaId,
            turma.CodigoTurma.ValorFormatado, // A string inteligente "MAT-2026-1-001"
            turma.CodigoTurma.Semestre,
            turma.CodigoTurma.Ano,
            turma.CodigoTurma.Numero,
            turma.Disciplina?.Nome ?? "Disciplina não carregada",
            turma.Professor?.NomeCompleto?.Valor ?? "Professor não carregado",
            turma.Ativo
        );
    }

    // 2. MAPEAMENTO DE ENTRADA (Criação)
    // Dica: Geralmente fazemos isso no UseCase por causa do 'Result',
    // mas se quiser no Mapper, ele precisa receber o VO já validado.
    public static Turma ToTurma(this TurmaDtoCreate dto, CodigoTurma codigoValidado)
    {
        return new Turma(
            codigoValidado, // O VO que o Use Case criou com sucesso
            dto.ProfessorId,
            dto.DisciplinaId
        );
    }

    // 3. MAPEAMENTO DE ATUALIZAÇÃO
    public static void ToUpdateTurma(this Turma turma, TurmaDtoUpdate dto, CodigoTurma novoCodigo)
    {
        // Chamamos o método de domínio que protege as regras
        turma.AtualizarDados(
        novoCodigo, // 👈 Agora os tipos batem: VO com VO
        dto.ProfessorId,
        dto.DisciplinaId
    );

        // Se o DTO de update trouxer o status, atualizamos também
        if (dto.Ativo != turma.Ativo)
        {
            // Aqui você usaria o método de ativar/desativar do seu repositório ou entidade
        }
    }
}