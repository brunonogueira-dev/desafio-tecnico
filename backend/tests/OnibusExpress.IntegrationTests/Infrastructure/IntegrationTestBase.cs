using System.Net.Http.Json;
using System.Text.Json;

namespace OnibusExpress.IntegrationTests.Infrastructure;

[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected OnibusApiFactory Factory { get; }
    protected HttpClient Client { get; }

    protected IntegrationTestBase(OnibusApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Factory.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected static object ReservaBody(Guid viagemId, int assento, string cpf, string nome = "Ana Souza") => new
    {
        viagemId,
        numeroAssento = assento,
        passageiro = new
        {
            nome,
            cpf,
            email = "ana@exemplo.com",
            dataNascimento = "1990-05-20"
        }
    };

    protected async Task<HttpResponseMessage> PostReservaAsync(Guid viagemId, int assento, string cpf) =>
        await Client.PostAsJsonAsync("/reservas", ReservaBody(viagemId, assento, cpf), Json);
}

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<OnibusApiFactory>
{
    public const string Name = "integration";
}
