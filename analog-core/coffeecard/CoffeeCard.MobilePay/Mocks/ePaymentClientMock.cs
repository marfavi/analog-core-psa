using System.Net.Http;
using System.Threading.Tasks;
using CoffeeCard.MobilePay.Clients;
using CoffeeCard.MobilePay.Generated.Api.ePaymentApi;
using Microsoft.Extensions.Logging;

public class EPaymentClientMock(HttpClient httpClient, ILogger<EPaymentClient> logger) : IEPaymentClient
{
    public Task<ModificationResponse> CancelPaymentAsync(string reference, CancelModificationRequest request)
    {
        return Task.FromResult(new ModificationResponse());
    }

    public Task<ModificationResponse> CapturePaymentAsync(string reference, CaptureModificationRequest request)
    {
        return Task.FromResult(new ModificationResponse());
    }

    public Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
    {
        return Task.FromResult(new CreatePaymentResponse());
    }

    public Task<GetPaymentResponse> GetPaymentAsync(string reference)
    {
        return Task.FromResult(new GetPaymentResponse());
    }

    public Task<ModificationResponse> RefundPaymentAsync(string reference, RefundModificationRequest request)
    {
        return Task.FromResult(new ModificationResponse());
    }
}