using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClanService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;

namespace ClanService.IntegrationTests.Infrastructure;

[Collection(IntegrationTestFixture.Name)]
public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected IntegrationTestsBase(IntegrationTestsWebFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        Services = factory.Services;
        AuthServiceClient = factory.AuthServiceClient;
    }

    protected IntegrationTestsWebFactory Factory { get; }

    protected HttpClient HttpClient { get; }

    protected IServiceProvider Services { get; }

    protected FakeAuthServiceClient AuthServiceClient { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Factory.ResetDatabaseAsync();

    protected void AuthorizeAs(
        Guid? userId = null,
        string name = "Test User",
        string email = "test@example.com",
        params string[] roles)
    {
        Guid resolvedUserId = userId ?? Guid.NewGuid();

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TestAuthHandler.SchemeName,
            $"{resolvedUserId}|{name}|{email}|{string.Join(',', roles)}");
    }

    protected void ClearAuthorization() =>
        HttpClient.DefaultRequestHeaders.Authorization = null;

    protected async Task<Envelope<T>> PostAsync<T>(string url, object body)
    {
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        Envelope<T>? envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>();
        return envelope ?? throw new InvalidOperationException($"Empty envelope from {url}");
    }

    protected Task<HttpResponseMessage> PostRawAsync(string url, object? body = null)
    {
        if (body is null)
        {
            HttpRequestMessage request = new(HttpMethod.Post, url);
            return HttpClient.SendAsync(request);
        }

        return HttpClient.PostAsJsonAsync(url, body);
    }

    protected async Task<Envelope<T>> GetAsync<T>(string url)
    {
        HttpResponseMessage response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Envelope<T>? envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>();
        return envelope ?? throw new InvalidOperationException($"Empty envelope from {url}");
    }

    protected async Task<T> ExecuteInDbAsync<T>(Func<ClanServiceDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ClanServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ClanServiceDbContext>();

        return await action(dbContext);
    }
}
