using GestorFinanceiro.Data.Context;
using GestorFinanceiro.API.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Controllers (mantém)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ✅ Swagger (mantém)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔴 DB (comentado para evitar crash)
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// if (!string.IsNullOrEmpty(connectionString))
// {
//     builder.Services.AddDbContext<ApplicationDbContext>(options =>
//         options.UseSqlServer(connectionString)
//                .ConfigureWarnings(w => w.Ignore(
//                    Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
// }
// else
// {
//     Console.WriteLine("⚠️ Connection string não encontrada");
// }

// 🔴 SignalR (pode causar crash)
// builder.Services.AddSignalR();

// 🔴 CORS (depende de SignalR / config externa)
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("SignalRPolicy", policy =>
//     {
//         policy.WithOrigins(
//                 "https://localhost:7100",
//                 "http://localhost:5243",
//                 "https://gestorfinanceiro-e9h0ctd4gnb9fqca.canadacentral-01.azurewebsites.net")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

// 🔴 Auth (sem config pode dar erro)
// builder.Services.AddAuthentication();
// builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ 🔥 OBRIGATÓRIO PARA AZURE
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

// ✅ Swagger
app.UseSwagger();
app.UseSwaggerUI();

// 🔴 CORS
// app.UseCors("SignalRPolicy");

// 🔴 HTTPS
// app.UseHttpsRedirection();

app.UseRouting();

// 🔴 Auth
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// 🔴 SignalR endpoint
// app.MapHub<FinanceHub>("/financeHub");

app.Run();