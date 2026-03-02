using Chess.Server;
using Chess.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalhostDev", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("LocalhostDev");

app.UseDefaultFiles();
app.UseStaticFiles();

// Serve SignalR client library from wwwroot if available, with helpful 404 message
app.MapGet("/signalr/signalr.min.js", async (HttpContext context, IWebHostEnvironment env) =>
{
    var wwwrootPath = Path.Combine(env.ContentRootPath, "wwwroot", "lib", "signalr.min.js");
    
    if (File.Exists(wwwrootPath))
    {
        context.Response.ContentType = "application/javascript";
        await context.Response.SendFileAsync(wwwrootPath);
    }
    else
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = "SignalR client library not found",
            message = "To fix this issue, download signalr.min.js from https://github.com/SignalR/SignalR/releases or use: libman install @microsoft/signalr@latest -d wwwroot/lib/signalr"
        });
    }
});

app.MapHub<ChessHub>("/hubs/chess");

app.Run();

public partial class Program;
