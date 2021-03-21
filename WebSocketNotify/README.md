## WebSocketNotify

WebSocketNotify is a dotnet-core package available on _[github.com](https://github.com/hshahpouri/WebSocketNotify)_ and _[nuget.org](https://www.nuget.org/packages/hShahpouri.WebSocketNotify/)_ to work with WebSocket standard in your website.


### How it works?

It is a very lightweight ClassLibrary built for dotnet core 3.1 and later that uses DI(dependency injection) to register `WSNotifyHandler`, Options Pattern and `IOptionMonitor` to carry out options for `WSNotifyHandler` and Extension Methods to append itself into dotnet core pipeline.

### How to use it?

> Although there is a **sample of asp.net core project** available on [github.com](https://github.com/hshahpouri/WebSocketNotify) , here is a step-by-step
> tutorial to show you how to use _WSNotify_ package into your own project.

##### Server side
To enable your website to accept WebSocket requests and receive message from clients or send/broadcast message
to clients, you need to follow these steps:

1. You need to add **hShahpouri.WebSocketNotify** package to your project. It is available on github.com and nuget.org
2. Create your desired options into _appsettings.json_ file like these default settings:
   ```json
   "WSNotify": {
       "Route": "/ws",
       "ConnectionPerIP": 1
   }
   ```
3. Append this line into `ConfigureServices()` method in _Startup.cs_
   ```c#
   // using WebSocketNotify;
   
   public void ConfigureServices(IServiceCollection services)
   {
       ...
       services.AddWebSocketNotify(Configuration);
       ...
   }
   ```
4. Prepend this line into `Configure()` method in _Startup.cs_
   ```c#
   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
   {
       // this code should be placed at the most first line of method's body or
       // immediately after your error handling middleware, for example:
       // app.UseDeveloperExceptionPage();
       app.UseWebSocketNotify();
       ...
   }
   ```
5. Now you need to get `WSNotifyHandler` service from **ServiceCollection** using DI and register a delegate for receiving messages from clients. The best place for doing it is in _Program.cs_ file, such as below:
   ```c#
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
      
      // Use this method to register a delegate to handle all received messages
      private static void RegisterReceiver(IHost host)
      {
          var handler = host.Services.GetRequiredService<WSNotifyHandler>();
          handler.OnReceive += (key, value, type) =>
          {
              Console.WriteLine();
              Console.WriteLine("--------------------------------[BEGIN]");
              Console.WriteLine($"({type}) from {key}");
              Console.WriteLine(value);
              Console.WriteLine("--------------------------------[END]");
              Console.WriteLine();
          };
      }
   }
   ```
6. Wherever you want to send a message to clients you just need to get `WSNotifyHandler` service using DI and then call `SendAsync()` method.

##### Client side
To start communicating with server using WebSocket, you need to follow these steps:

1. In your webpage, inside a `<script>` tag (or a .js file) add this block:
    ```javascript
    /** @type{WebSocket} */
    var ws = null;
    
    function socket_connect() {

        ws = new WebSocket("wss://localhost:5001/ws");

        ws.onopen = function (ev) {
            console.log("onopen", ev);
        };

        ws.onerror = function (ev) {
            console.log("onerror", ev);
        };

        ws.onclose = function (ev) {
            console.log("onclose", ev);
        };

        ws.onmessage = function (ev) {
            console.log("onmessage", ev.data);
        };
    }

    function socket_close() {
        ws.close();
    }

    function socket_send() {
        ws.send("this is a message from client to server over WebSocket");
    }
    ```

For more details see [Writing WebSocket client applications](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_client_applications).
