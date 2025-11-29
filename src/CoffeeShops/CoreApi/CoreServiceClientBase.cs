using System.Net;
using System.Net.Http.Json;
using CoffeeShops.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.CoreApi;

internal abstract class CoreServiceClientBase
{
    protected readonly HttpClient Client;
    protected readonly ILogger Logger;

    protected CoreServiceClientBase(HttpClient client, ILogger logger)
    {
        Client = client;
        Logger = logger;
    }

    protected async Task<T?> ReadAsync<T>(HttpResponseMessage response, Func<HttpStatusCode, Exception?>? map = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        if (map != null)
        {
            var mapped = map(response.StatusCode);
            if (mapped != null)
            {
                throw mapped;
            }
        }

        throw await BuildException(response);
    }

    protected async Task EnsureSuccess(HttpResponseMessage response, Func<HttpStatusCode, Exception?>? map = null)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (map != null)
        {
            var mapped = map(response.StatusCode);
            if (mapped != null)
            {
                throw mapped;
            }
        }

        throw await BuildException(response);
    }

    protected async Task<CoreApiException> BuildException(HttpResponseMessage response)
    {
        Error? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<Error>();
        }
        catch
        {
            // ignore
        }

        var message = error?.Message ?? response.ReasonPhrase ?? "Core service error";
        return new CoreApiException(response.StatusCode, message, error);
    }
}
