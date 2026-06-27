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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    Console.WriteLine("⚠️ Connection string não encontrada!");
}

// SignalR
builder.Services.AddSignalR();

// Auth
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ PORTA (OBRIGATÓRIO)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// Swagger sempre disponível (opcional)
app.UseSwagger();
app.UseSwaggerUI();

// ⚠️ PODES comentar se der problemas
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FinanceHub>("/financeHub");

app.Run();