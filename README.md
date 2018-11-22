# ASP.NET Core donut output caching middleware

Donut server-side caching middleware for ASP.NET 2.0. 
With this extension, you'll be able to pull outside the cache any component. This is very useful when you have personnalized content like user profile top nan...
This is a classic ASP.NET CORE 2.0 will put the html output in cache.
This library is based on the great MadKristensen's WebEssentials.AspNetCore.OutputCaching library : https://github.com/madskristensen/WebEssentials.AspNetCore.OutputCaching. 

## Register the middleware

Start by registering the service it in `Startup.cs` like so:

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddDonutOutputCaching();
}
```

...and then register the middleware just before the call to `app.UseMvc(...)` like so:

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseDonutOutputCaching();
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

## Usage examples
There are various ways to use and customize the output caching. Here are some examples.

### Component invocation
Use the classic `Component.InvokeAsync` attribute in a view.
The boolean param excludeFromCache will specify that this component must be refreshed on each page view.

```c#
@await Component.InvokeAsync("MyComponent", excludeFromCache: true)
```

### Action filter
Use the `OutputCache` attribute on a controller action:

```c#
[DonutOutputCache(Duration = 600, VaryByParam = "id")]
public IActionResult Product()
{
    return View();
}
```

### Programmatically

Using the `EnableOutputCaching` extension method on the `HttpContext` object in MVC, WebAPI or Razor Pages:

```c#
public IActionResult About()
{
    HttpContext.EnableOutputCaching(TimeSpan.FromMinutes(1));
    return View();
}
```

## Caching profiles
Set up cache profiles to reuse the same settings.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddDonutOutputCaching(options =>
    {
        options.Profiles["default"] = new OutputCacheProfile
        {
            Duration = 600
        };
    });
}
```

Then use the profile from an action filter:

```c#
[DonutOutputCache(Profile = "default")]
public IActionResult Index()
{
    return View();
}
```
