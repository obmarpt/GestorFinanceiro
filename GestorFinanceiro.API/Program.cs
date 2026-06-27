using GestorFinanceiro.Data.Context;
using GestorFinanceiro.API.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ✅ Swagger (substitui AddOpenApi para ser mais estável)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (protegido para não crashar no Azure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString)
               .ConfigureWarnings(w => w.Ignore(
                   Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}
else
{
    Console.WriteLine("⚠️ Connection string não encontrada");
}

// SignalR
builder.Services.AddSignalR();

// ✅ CORS (mantive o teu + preparado para produção)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7100",
                "http://localhost:5243",
                "https://gestorfinanceiro-e9h0ctd4gnb9fqca.canadacentral-01.azurewebsites.net") // 👈 adiciona o teu domínio Azure depois
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ 🔥 OBRIGATÓRIO PARA AZURE
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// ✅ Swagger disponível sempre
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Ordem correta
app.UseCors("SignalRPolicy");

// ⚠️ Se tiveres problemas podes comentar
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FinanceHub>("/financeHub");

app.Run();
