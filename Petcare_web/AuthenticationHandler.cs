// Petcare_web/Handlers/AuthenticationHandler.cs

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Lấy token từ session của người dùng hiện tại
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            // Gán token vào Authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Gửi yêu cầu đi
        return await base.SendAsync(request, cancellationToken);
    }
}