using SitemaDeMatricula.Aplicacao.Dtos.Matricola;
using SitemaDeMatricula.Domain;
using SitemaDeMatricula.Domain.Interfaces;
using SitemaDeMatricula.Domain.Mapper;
using SitemaDeMatricula.Domain.Modelos;

namespace SitemaDeMatricula.Aplicacao.Usecases.Matriculas;

public class MatricularEstudanteUsecase
{
    private readonly IRepositorioMatricula _matriculaRepo;
    private readonly IRepositorioEstudante _estudanteRepo;
    private readonly IRepositorioTurma _turmaRepo;

    public MatricularEstudanteUsecase(
        IRepositorioMatricula matriculaRepo,
        IRepositorioEstudante estudanteRepo,
        IRepositorioTurma turmaRepo)
    {
        _matriculaRepo = matriculaRepo;
        _estudanteRepo = estudanteRepo;
        _turmaRepo = turmaRepo;
    }

    public async Task<Result<MatriculaDtoResponse>> ExecutarAsync(MatriculaDtoCreate dto)
    {
        // 1. O Estudante existe no mundo real?
        var estudante = await _estudanteRepo.ObterPorIdAsync(dto.EstudanteId);
        if (estudante == null) return Result<MatriculaDtoResponse>.Falha("Estudante não encontrado.");

        // 2. A Turma existe no mundo real?
        var turma = await _turmaRepo.ObterPorIdAsync(dto.TurmaId);
        if (turma == null) return Result<MatriculaDtoResponse>.Falha("Turma não encontrada.");

        // 3. Sua lógica matadora de duplicidade
        if (await _matriculaRepo.ExisteMatriculaAtivaAsync(dto.EstudanteId, dto.TurmaId))
        {
            return Result<MatriculaDtoResponse>.Falha("Este estudante já está matriculado nesta turma.");
        }

        // 4. Criação e Persistência
        var novaMatricula = new Matricula(dto.EstudanteId, dto.TurmaId);
        await _matriculaRepo.AdicionarAsync(novaMatricula);

        // 5. Retorno com os dados completos (Mapper em ação!)
        return Result<MatriculaDtoResponse>.Ok(novaMatricula.ToMatriculaDtoResponse());
    }
}