using Chess.Server.Services.Games;
using Chess.Server.Services.Auth;
using Chess.Server.Models;
using Chess.Server.Hubs;
using Chess.Server.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddControllers();
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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ChessAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("LocalhostDev");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

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
