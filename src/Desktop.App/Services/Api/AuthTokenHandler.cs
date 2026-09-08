using System.Net.Http;
using System.Net.Http.Headers;
using Desktop.App.Session;

namespace Desktop.App.Services.Api;

public sealed class AuthTokenHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = AppSession.AccessToken;

        if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
