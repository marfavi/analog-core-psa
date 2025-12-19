using System.Net.Http;
using System.Threading.Tasks;
using CoffeeCard.MobilePay.Generated.Api.WebhooksApi;
using Microsoft.Extensions.Logging;

namespace CoffeeCard.MobilePay.Clients;

public class WebhooksClientMock(HttpClient httpClient, ILogger<WebhooksClient> logger) : IWebhooksClient
{
    public Task<RegisterResponse> CreateWebhookAsync(RegisterRequest request)
    {
        return Task.FromResult(new RegisterResponse());
    }

    public Task<QueryResponse> GetAllWebhooksAsync()
    {
        return Task.FromResult(new QueryResponse());
    }
}
