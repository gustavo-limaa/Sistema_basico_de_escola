using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeMatricula.Testes.Teste_Unitarios;
using SistemaDeMatricula.Testes.Testes_Integracao.Setup;
using SitemaDeMatricula.Aplicacao.Dtos.estudante;
using SitemaDeMatricula.InfraEstrutura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeMatricula.Testes.Test_Integracao.Estudante;

[Collection("ApiMatrix")]
public class CriarEstudanteIntegrationTest : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly SistemaMatriculaFactory _factory; // Guardamos a factory para usar depois

    public CriarEstudanteIntegrationTest(SistemaMatriculaFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // 1. ANTES DE CADA TESTE: Não precisamos de nada especial aqui
    public Task InitializeAsync() => Task.CompletedTask;

    // 2. DEPOIS DE CADA TESTE: Aqui é onde a mágica da limpeza acontece
    public async Task DisposeAsync()
    {
        // Criamos um "escopo" para conseguir pegar o AppDbContext lá de dentro da API
        using var scope = _factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Agora sim! O 'contexto' existe aqui e podemos limpar a tabela
        await contexto.Estudantes.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Deve_Retornar_Sucesso_Quando_Criar_Estudante_Valido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        // ATENÇÃO: Verifique se essa é a rota real da sua Controller!
        // Mude de "/api/estudantes" para "/api/Estudante"
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        // 3. LÊ A FOFOCA INTEIRA DA API
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        // 4. ASSERT COM MENSAGEM CUSTOMIZADA
        // Se o status não for sucesso (200-299), o teste quebra e joga a mensagem real na sua cara!
        Assert.True(response.IsSuccessStatusCode, $"A API recusou o estudante! Motivo: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            "", // NOME VAZIO (INVÁLIDO)
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Email_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            "email-invalido", // EMAIL INVÁLIDO
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com email inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_CPF_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            "12345678900", // CPF INVÁLIDO
            estudanteFake.Telefone.Valor
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com CPF inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_Telefone_Invalido()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();
        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            estudanteFake.DataNascimento.Valor,
            estudanteFake.Cpf.Valor,
            "12345" // TELEFONE INVÁLIDO
        );
        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();
        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com telefone inválido! Resposta: {respostaDaApi}");
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Criar_Estudante_Com_DataNascimento_Futura()
    {
        var estudanteFake = DataFactory.EstudanteFaker.Generate();

        // Pega o dia de amanhã direto do relógio, sem conversão de texto
        var amanha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var dtoCreate = new EstudanteDtoCreate(
            estudanteFake.NomeCompleto.Valor,
            estudanteFake.Email.Valor,
            amanha, // Passa o objeto DateOnly puro
            estudanteFake.Cpf.Valor,
            estudanteFake.Telefone.Valor
        );

        var response = await _client.PostAsJsonAsync("/api/Estudante", dtoCreate);
        var respostaDaApi = await response.Content.ReadAsStringAsync();

        Assert.False(response.IsSuccessStatusCode, $"A API aceitou um estudante com data futura! Resposta: {respostaDaApi}");
    }
}