using Moq;
using SistemaDeMatricula.Aplicacao.Dtos.Notas;
using SistemaDeMatricula.Aplicacao.Usecases.Notas;
using SistemaDeMatricula.Domain.Interfaces;
using SistemaDeMatricula.Domain.Modelos;
using SistemaDeMatricula.Domain.Uteis;
using SistemaDeMatricula.Infraestrutura.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EstudanteEntity = SistemaDeMatricula.Domain.Modelos.Estudante;
using MatriculaEntity = SistemaDeMatricula.Domain.Modelos.Matricula;
using TurmaEntity = SistemaDeMatricula.Domain.Modelos.Turma;

namespace SistemaDeMatricula.Testes.Teste_Unitarios.NotasTestUnitario;

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