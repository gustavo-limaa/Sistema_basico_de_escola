using Moq;
using SistemaDeMatricula.Aplicacao.Usecases.Notas;
using SistemaDeMatricula.Domain.Interfaces;

namespace SistemaDeMatricula.Teste.Unit.Teste_Unitarios.NotasTestUnitario;

public class AddcionarNotasTestUnitario
{
    private readonly Mock<IUnitOfWork> _uowMock;

    private readonly AdicionarNotasMatriculaUseCase _Usecase;

    public AddcionarNotasTestUnitario()
    {
        _uowMock = new Mock<IUnitOfWork> { DefaultValue = DefaultValue.Mock };

        _Usecase = new AdicionarNotasMatriculaUseCase(_uowMock.Object);
    }
}