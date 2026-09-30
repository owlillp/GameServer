using System.Net.Http.Headers;
using CSharpFunctionalExtensions;
using Shared.Framework.Authentication.HttpClients;
using Shared.SharedKernel.Errors;

namespace AuthService.Contracts.HttpCommunication;

/// <summary>
/// Internal API AuthService вызывается только под service-токеном
/// (client_credentials), без форвардинга пользовательского токена.
/// </summary>
public sealed class ServiceTokenHandler(ServiceTokenProvider serviceTokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<string, Error> token = await serviceTokenProvider
            .GetTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        if (token.IsSuccess)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
