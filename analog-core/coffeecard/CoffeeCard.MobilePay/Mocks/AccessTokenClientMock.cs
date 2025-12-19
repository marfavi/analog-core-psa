using System.Net.Http;
using System.Threading.Tasks;
using CoffeeCard.MobilePay.Exception.v2;
using CoffeeCard.MobilePay.Generated.Api.AccessTokenApi;
using Microsoft.Extensions.Logging;

namespace CoffeeCard.MobilePay.Clients;

public class AccessTokenClientMock(HttpClient httpClient, ILogger<AccessTokenClient> logger)
    : IAccessTokenClient
{

    public async Task<AuthorizationTokenResponse> GetToken(string clientId, string clientSecret)
    {
        return new AuthorizationTokenResponse();
    }
}
