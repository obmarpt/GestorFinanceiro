using GestorFinanceiro.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Dashboard/Index", "");
});

builder.Services.AddHttpClient("GestorFinanceiroApi", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var baseUrl = config["ApiBaseUrl"]
        ?? (env.IsDevelopment()
            ? "http://localhost:5281"
            : "https://gestorfinanceiro-e9h0ctd4gnb9fqca.canadacentral-01.azurewebsites.net");
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    app.Urls.Add($"http://*:{port}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
