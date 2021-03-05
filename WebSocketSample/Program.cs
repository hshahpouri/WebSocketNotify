using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WebSocketNotify;

namespace WebSocketSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            RegisterReceiver(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        private static void RegisterReceiver(IHost host)
        {
            var handler = host.Services.GetRequiredService<WSNotifyHandler>();
            handler.OnReceive += (key, value, type) =>
            {
                Console.WriteLine("\n--------------------------------[BEGIN]");
                Console.WriteLine($"({type}) from {key}");
                Console.WriteLine(value);
                Console.WriteLine("--------------------------------[END]\n");
            };
        }
    }
}
