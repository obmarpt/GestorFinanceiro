using GestorFinanceiro.Data.Context;
using GestorFinanceiro.API.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

<<<<<<< HEAD
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
=======
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(
               Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
>>>>>>> master

builder.Services.AddSignalR();

<<<<<<< HEAD
// Auth
=======
// CORS para permitir ligações SignalR da app Web
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7100",
                "http://localhost:5243")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

>>>>>>> master
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

<<<<<<< HEAD
// ✅ PORTA (OBRIGATÓRIO)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// Swagger sempre disponível (opcional)
app.UseSwagger();
app.UseSwaggerUI();

// ⚠️ PODES comentar se der problemas
// app.UseHttpsRedirection();

=======
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS tem de vir antes do UseHttpsRedirection
app.UseCors("SignalRPolicy");

app.UseHttpsRedirection();
>>>>>>> master
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FinanceHub>("/financeHub");

app.Run();