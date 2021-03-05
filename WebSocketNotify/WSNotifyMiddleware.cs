using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketNotify
{
    public class WSNotifyMiddleware
    {
        private readonly RequestDelegate _next;

        public WSNotifyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IOptionsMonitor<WSNotifyOptions> wsNotifyOptions, WSNotifyHandler wsNotify)
        {
            if (httpContext.Request.Path == wsNotifyOptions.CurrentValue.Route)
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    if (wsNotify.IsConnectionAllowed(httpContext.Connection.RemoteIpAddress))
                    {
                        using WebSocket ws = await httpContext.WebSockets.AcceptWebSocketAsync();

                        var socketTCS = new TaskCompletionSource<WebSocket>();

                        wsNotify.Add(httpContext.Request.Headers["Sec-WebSocket-Key"], httpContext.Connection.RemoteIpAddress, ws, socketTCS);

                        await socketTCS.Task;
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 403;
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
