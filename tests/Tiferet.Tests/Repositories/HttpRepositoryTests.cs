using System.Net;
using System.Text.Json;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Utilities.Json;

namespace Tiferet.Tests.Repositories;

// Reuse SampleDomain and SampleAggregate from Mappers tests via shared namespace reference.
// We need local copies since they're in a different namespace.

public sealed record HttpTestDomain(string Id, string Name) : DomainObject;

public record HttpTestAggregate : Aggregate<HttpTestDomain>
{
    public HttpTestAggregate(HttpTestDomain state) : base(state) { }
    public string Id => State.Id;
    public string Name => State.Name;
}

[JsonNaming(NamingConvention.SnakeCase)]
public class HttpTestJsonTransfer : JsonTransferObject<HttpTestAggregate>
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";

    public override HttpTestAggregate Map(Dictionary<string, object?>? overrides = null)
        => new(new HttpTestDomain(Id, UserName));
}

// Concrete test repository exposing protected methods.
public class TestHttpRepository : HttpRepository<HttpTestAggregate>
{
    public TestHttpRepository(IHttpClientFactory factory, IAuthTokenProvider? auth = null)
        : base(factory, auth) { }

    public Task<HttpTestAggregate> TestGetAsync(string url)
        => GetAsync<HttpTestJsonTransfer>(url);

    public Task<HttpTestAggregate> TestPostAsync(string url, object body)
        => PostAsync<HttpTestJsonTransfer>(url, body);

    public Task TestDeleteAsync(string url)
        => DeleteAsync(url);
}

// A simple mock message handler for testing.
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

    public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => _handler(request);
}

// Simple mock IHttpClientFactory.
public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;
    public MockHttpClientFactory(HttpClient client) => _client = client;
    public HttpClient CreateClient(string name) => _client;
}

// Simple mock IAuthTokenProvider.
public class MockAuthTokenProvider : IAuthTokenProvider
{
    private readonly string? _token;
    public MockAuthTokenProvider(string? token) => _token = token;
    public Task<string?> GetTokenAsync() => Task.FromResult(_token);
}

public class HttpRepositoryTests
{
    private static (TestHttpRepository repo, MockHttpMessageHandler handler) CreateRepo(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler,
        IAuthTokenProvider? auth = null)
    {
        var mockHandler = new MockHttpMessageHandler(handler);
        var client = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.test.com") };
        var factory = new MockHttpClientFactory(client);
        return (new TestHttpRepository(factory, auth), mockHandler);
    }

    [Fact]
    public async Task GetAsync_DeserializesAndMaps()
    {
        var json = """{"id":"1","user_name":"Alice"}""";
        var (repo, _) = CreateRepo(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            }));

        var result = await repo.TestGetAsync("https://api.test.com/users/1");
        Assert.Equal("1", result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task PostAsync_SendsBodyAndDeserializesResponse()
    {
        string? capturedBody = null;
        var responseJson = """{"id":"2","user_name":"Bob"}""";
        var (repo, _) = CreateRepo(async req =>
        {
            capturedBody = await req.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var body = new { Name = "Bob" };
        var result = await repo.TestPostAsync("https://api.test.com/users", body);
        Assert.Equal("2", result.Id);
        Assert.Equal("Bob", result.Name);
        Assert.NotNull(capturedBody);
        Assert.Contains("Bob", capturedBody!);
    }

    [Fact]
    public async Task DeleteAsync_CompletesOnSuccess()
    {
        var (repo, _) = CreateRepo(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                Content = new StringContent("")
            }));

        // Should not throw.
        await repo.TestDeleteAsync("https://api.test.com/users/1");
    }

    [Fact]
    public async Task GetAsync_NonSuccessStatusCode_ThrowsTiferetException()
    {
        var (repo, _) = CreateRepo(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found")
            }));

        var ex = await Assert.ThrowsAsync<TiferetException>(
            () => repo.TestGetAsync("https://api.test.com/users/999"));
        Assert.Equal(ErrorCodes.HttpRequestFailed, ex.ErrorCode);
        Assert.Equal(404, ex.Context["statusCode"]);
    }

    [Fact]
    public async Task GetAsync_HttpRequestException_ThrowsTiferetException()
    {
        var (repo, _) = CreateRepo(_ =>
            throw new HttpRequestException("Connection refused"));

        var ex = await Assert.ThrowsAsync<TiferetException>(
            () => repo.TestGetAsync("https://api.test.com/users/1"));
        Assert.Equal(ErrorCodes.HttpRequestFailed, ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_WithAuthProvider_InjectsBearer()
    {
        string? authHeader = null;
        var json = """{"id":"1","user_name":"Authed"}""";
        var auth = new MockAuthTokenProvider("my-secret-token");
        var (repo, _) = CreateRepo(req =>
        {
            authHeader = req.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }, auth);

        var result = await repo.TestGetAsync("https://api.test.com/users/1");
        Assert.Equal("Authed", result.Name);
        Assert.Equal("Bearer my-secret-token", authHeader);
    }

    [Fact]
    public async Task GetAsync_WithNullToken_NoAuthHeader()
    {
        string? authHeader = null;
        var json = """{"id":"1","user_name":"NoAuth"}""";
        var auth = new MockAuthTokenProvider(null);
        var (repo, _) = CreateRepo(req =>
        {
            authHeader = req.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });
        }, auth);

        await repo.TestGetAsync("https://api.test.com/users/1");
        Assert.Null(authHeader);
    }
}
