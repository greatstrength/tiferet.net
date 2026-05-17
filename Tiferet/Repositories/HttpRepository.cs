using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Utilities.Json;

namespace Tiferet.Repositories;

/// <summary>
/// Abstract base class for HTTP-backed repositories.
/// Provides template methods for GET, POST, PUT, and DELETE operations
/// with convention-aware JSON deserialization and automatic transfer object mapping.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type managed by this repository.</typeparam>
public abstract class HttpRepository<TAggregate>
    where TAggregate : Aggregate
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthTokenProvider? _authTokenProvider;

    /// <summary>
    /// The logical name of the <see cref="HttpClient"/> to create via the factory.
    /// Defaults to the concrete repository type name. Override to customize.
    /// </summary>
    protected virtual string HttpClientName => GetType().Name;

    /// <summary>
    /// Initializes the HTTP repository.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating <see cref="HttpClient"/> instances.</param>
    /// <param name="authTokenProvider">Optional provider for auth header injection.</param>
    protected HttpRepository(IHttpClientFactory httpClientFactory, IAuthTokenProvider? authTokenProvider = null)
    {
        _httpClientFactory = httpClientFactory;
        _authTokenProvider = authTokenProvider;
    }

    /// <summary>
    /// Create a configured <see cref="HttpClient"/>, injecting the auth header if a provider is present.
    /// </summary>
    protected async Task<HttpClient> CreateClientAsync()
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        if (_authTokenProvider is not null)
        {
            var token = await _authTokenProvider.GetTokenAsync();
            if (token is not null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    /// <summary>
    /// Perform a GET request, deserialize the response to <typeparamref name="TTransfer"/>,
    /// and map it to <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <typeparam name="TTransfer">The JSON transfer object type.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="overrides">Optional mapping overrides.</param>
    /// <returns>The mapped aggregate.</returns>
    protected async Task<TAggregate> GetAsync<TTransfer>(string url, Dictionary<string, object?>? overrides = null)
        where TTransfer : JsonTransferObject<TAggregate>
    {
        var json = await SendAndReadAsync(HttpMethod.Get, url);
        var transfer = JsonSerializerHelper.Deserialize<TTransfer>(json);
        return transfer.Map(overrides);
    }

    /// <summary>
    /// Perform a POST request with a JSON body, deserialize the response to
    /// <typeparamref name="TTransfer"/>, and map it to <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <typeparam name="TTransfer">The JSON transfer object type.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body object (serialized to JSON).</param>
    /// <param name="overrides">Optional mapping overrides.</param>
    /// <returns>The mapped aggregate.</returns>
    protected async Task<TAggregate> PostAsync<TTransfer>(string url, object body, Dictionary<string, object?>? overrides = null)
        where TTransfer : JsonTransferObject<TAggregate>
    {
        var json = await SendAndReadAsync(HttpMethod.Post, url, body);
        var transfer = JsonSerializerHelper.Deserialize<TTransfer>(json);
        return transfer.Map(overrides);
    }

    /// <summary>
    /// Perform a PUT request with a JSON body, deserialize the response to
    /// <typeparamref name="TTransfer"/>, and map it to <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <typeparam name="TTransfer">The JSON transfer object type.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body object (serialized to JSON).</param>
    /// <param name="overrides">Optional mapping overrides.</param>
    /// <returns>The mapped aggregate.</returns>
    protected async Task<TAggregate> PutAsync<TTransfer>(string url, object body, Dictionary<string, object?>? overrides = null)
        where TTransfer : JsonTransferObject<TAggregate>
    {
        var json = await SendAndReadAsync(HttpMethod.Put, url, body);
        var transfer = JsonSerializerHelper.Deserialize<TTransfer>(json);
        return transfer.Map(overrides);
    }

    /// <summary>
    /// Perform a DELETE request. Does not expect a response body.
    /// </summary>
    /// <param name="url">The request URL.</param>
    protected async Task DeleteAsync(string url)
    {
        await SendAndReadAsync(HttpMethod.Delete, url);
    }

    /// <summary>
    /// Send an HTTP request and return the response body as a string.
    /// Wraps HTTP errors in <see cref="TiferetException"/> with structured error codes.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The request URL.</param>
    /// <param name="body">Optional request body (serialized to JSON).</param>
    /// <returns>The response body string.</returns>
    private async Task<string> SendAndReadAsync(HttpMethod method, string url, object? body = null)
    {
        using var client = await CreateClientAsync();

        var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            var bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = false });
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw new TiferetException(
                ErrorCodes.HttpRequestFailed,
                $"HTTP {method} to {url} failed.",
                ("method", method.Method),
                ("url", url),
                ("error", ex.Message));
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new TiferetException(
                ErrorCodes.HttpRequestFailed,
                $"HTTP {method} to {url} returned {(int)response.StatusCode}.",
                ("method", method.Method),
                ("url", url),
                ("statusCode", (int)response.StatusCode),
                ("responseBody", responseBody));
        }

        return responseBody;
    }
}
