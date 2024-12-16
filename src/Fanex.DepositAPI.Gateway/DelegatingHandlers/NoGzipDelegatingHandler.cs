namespace Fanex.DepositAPI.Gateway.DelegatingHandlers;

using System.Net.Http.Headers;

public class NoGzipDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //Trying to avoid gzip encoding in the response, so selecting other compression algorithms
        request.Headers.AcceptEncoding.Clear();
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        return await base.SendAsync(request, cancellationToken);
    }
}