# WSNotify

###### WebSocket middleware for ASP.Net Core 3.1 and later



You can add the following _JSON_ into **appsettings.json** of
your project

```json
"WSNotify": {
    "Route": "/ws",
    "ConnectionPerIP": 1
  }
```

Then add the these codes to **Startup.cs** file:

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddWebSocketNotify(Configuration);
    ...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseDeveloperExceptionPage();

    // This line should be placed at top most of the ASP.NET Core pipeline
    app.UseWebSocketNotify();
    ...
    ...
    ...
}
```