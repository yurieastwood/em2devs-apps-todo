using EM2Devs.Todo.Infrastructure.Auth;

namespace EM2Devs.Todo.Api.Middleware;

public sealed class DemoAuthMiddleware
{
    public const string CookieName = "demo-user";

    private readonly RequestDelegate _next;

    public DemoAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        DemoCurrentUser currentUser = context.RequestServices.GetRequiredService<DemoCurrentUser>();

        bool hasHeader = context.Request.Headers.ContainsKey("X-Demo-User");
        bool hasCookie = context.Request.Cookies.ContainsKey(CookieName);

        if (hasHeader || hasCookie)
        {
            currentUser.IsAuthenticated = true;
        }

        await _next(context).ConfigureAwait(false);
    }
}
