using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketNotify
{
    public static class WSNotifyExtensions
    {
        public static IApplicationBuilder UseWebSocketNotify(this IApplicationBuilder builder) =>
            builder.UseWebSockets()
                   .UseMiddleware<WSNotifyMiddleware>();


        public static IServiceCollection AddWebSocketNotify(this IServiceCollection services) =>
            services.Configure<WSNotifyOptions>(config => new WSNotifyOptions())
                    .AddSingleton<WSNotifyHandler>();


        public static IServiceCollection AddWebSocketNotify(this IServiceCollection services, IConfiguration configuration) =>
            services.Configure<WSNotifyOptions>(configuration.GetSection(WSNotifyOptions.CONFIG_NAME))
                    .AddSingleton<WSNotifyHandler>();


        public static IServiceCollection AddWebSocketNotify(this IServiceCollection services, WSNotifyOptions options) =>
            services.Configure<WSNotifyOptions>(config =>
                        {
                            config.Route = options.Route;
                            config.ConnectionPerIP = options.ConnectionPerIP;
                        })
                    .AddSingleton<WSNotifyHandler>();

    }
}
