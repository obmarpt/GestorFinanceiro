using GestorFinanceiro.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("GestorFinanceiroApi", (sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ApiBaseUrl"]
        ?? "http://localhost:5281";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var handler = new HttpClientHandler();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AcessoNegado";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApenasAdmin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Criar utilizador Admin se não existir
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    if (!context.Utilizadores.Any(u => u.Role == "Admin"))
    {
        context.Utilizadores.Add(new GestorFinanceiro.Data.Models.Utilizador
        {
            Nome = "Administrador",
            Username = "admin",
            Email = "admin@gestorfinanceiro.pt",
            PasswordHash = "admin123",
            Role = "Admin"
        });
        context.SaveChanges();
    }
}

app.Run();
