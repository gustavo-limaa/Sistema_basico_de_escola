namespace SistemaDeMatricula.Testes.Test_Integracao.Setup;

// 1. Damos um nome para a nossa Collection
[CollectionDefinition("ApiMatrix")]
public class ApiCollection : ICollectionFixture<SistemaMatriculaFactory>
{
    // Essa classe fica VAZIA mesmo! É só uma marcação do xUnit.
}