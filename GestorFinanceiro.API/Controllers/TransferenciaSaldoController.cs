using GestorFinanceiro.API.Hubs;
using GestorFinanceiro.API.Models;
using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciaSaldoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<FinanceHub> _hubContext;

        public TransferenciaSaldoController(ApplicationDbContext context, IHubContext<FinanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Transferir([FromBody] TransferenciaSaldoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.SessaoOrigemId == request.SessaoDestinoId)
                return BadRequest("A sessão de origem e destino devem ser diferentes.");

            var origem = await _context.SessoesFinanceiras.FindAsync(request.SessaoOrigemId);
            var destino = await _context.SessoesFinanceiras.FindAsync(request.SessaoDestinoId);

            if (origem == null || destino == null)
                return NotFound("Sessão de origem ou destino não encontrada.");

            var totalReceitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == request.SessaoOrigemId)
                .SumAsync(r => r.Valor);

            var totalDespesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == request.SessaoOrigemId)
                .SumAsync(d => d.Valor);

            var saldoOrigem = totalReceitas - totalDespesas;

            if (request.Valor > saldoOrigem)
                return BadRequest($"Saldo insuficiente. Disponível: {saldoOrigem:N2} €.");

            var motivo = string.IsNullOrWhiteSpace(request.Descricao)
                ? "Transferência de saldo"
                : request.Descricao.Trim();

            var data = DateTime.Now;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Despesas.Add(new Despesa
            {
                Descricao = $"{motivo} → {destino.Nome}",
                Valor = request.Valor,
                Data = data,
                SessaoFinanceiraId = request.SessaoOrigemId
            });

            _context.Receitas.Add(new Receita
            {
                Descricao = $"{motivo} ← {origem.Nome}",
                Valor = request.Valor,
                Data = data,
                SessaoFinanceiraId = request.SessaoDestinoId
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Notificar todos os browsers via SignalR
            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoOrigemId);
            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoDestinoId);

            return Ok(new { mensagem = "Transferência concluída com sucesso." });
        }
    }
}
