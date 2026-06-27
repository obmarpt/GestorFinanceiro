namespace GestorFinanceiro.Web.Helpers;

public class SameOriginHttpClientHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SameOriginHttpClientHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            if (request.RequestUri is { IsAbsoluteUri: false })
            {
                var baseUri = new Uri($"{context.Request.Scheme}://{context.Request.Host}/");
                request.RequestUri = new Uri(baseUri, request.RequestUri);
            }

            var cookie = context.Request.Headers.Cookie.ToString();
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
