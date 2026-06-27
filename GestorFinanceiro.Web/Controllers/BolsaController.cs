using GestorFinanceiro.Web.Hubs;
using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BolsaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<FinanceHub> _hubContext;

        public BolsaController(ApplicationDbContext context, IHubContext<FinanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("{utilizadorId}")]
        public async Task<IActionResult> GetBolsa(int utilizadorId)
        {
            var bolsa = await _context.Bolsas
                .FirstOrDefaultAsync(b => b.UtilizadorId == utilizadorId);

            if (bolsa == null)
                return Ok(new { id = 0, utilizadorId, saldo = 0m, dataAtualizacao = DateTime.Now });

            return Ok(bolsa);
        }

        [HttpPost("apagar-sessao")]
        public async Task<IActionResult> ApagarSessao([FromBody] ApagarSessaoRequest request)
        {
            var sessao = await _context.SessoesFinanceiras.FindAsync(request.SessaoId);
            if (sessao == null)
                return NotFound("Sessão não encontrada.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (request.GuardarNaBolsa)
            {
                var totalReceitas = await _context.Receitas
                    .Where(r => r.SessaoFinanceiraId == request.SessaoId)
                    .SumAsync(r => r.Valor);

                var totalDespesas = await _context.Despesas
                    .Where(d => d.SessaoFinanceiraId == request.SessaoId)
                    .SumAsync(d => d.Valor);

                var saldo = totalReceitas - totalDespesas;

                if (saldo > 0)
                {
                    var bolsa = await _context.Bolsas
                        .FirstOrDefaultAsync(b => b.UtilizadorId == request.UtilizadorId);

                    if (bolsa == null)
                    {
                        bolsa = new Bolsa
                        {
                            UtilizadorId = request.UtilizadorId,
                            Saldo = 0,
                            DataAtualizacao = DateTime.Now
                        };
                        _context.Bolsas.Add(bolsa);
                    }

                    bolsa.Saldo += saldo;
                    bolsa.DataAtualizacao = DateTime.Now;
                }
            }

            _context.SessoesFinanceiras.Remove(sessao);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoId);

            return Ok(new { mensagem = "Sessão apagada com sucesso." });
        }

        [HttpPost("transferir-sessao")]
        public async Task<IActionResult> TransferirParaSessao([FromBody] BolsaTransferirSessaoRequest request)
        {
            var bolsa = await _context.Bolsas
                .FirstOrDefaultAsync(b => b.UtilizadorId == request.UtilizadorId);

            if (bolsa == null || bolsa.Saldo < request.Valor)
                return BadRequest($"Saldo insuficiente na bolsa. Disponível: {bolsa?.Saldo ?? 0:N2} €.");

            var sessao = await _context.SessoesFinanceiras.FindAsync(request.SessaoDestinoId);
            if (sessao == null)
                return NotFound("Sessão não encontrada.");

            bolsa.Saldo -= request.Valor;
            bolsa.DataAtualizacao = DateTime.Now;

            _context.Receitas.Add(new Receita
            {
                Descricao = "Transferência da bolsa",
                Valor = request.Valor,
                Data = DateTime.Now,
                SessaoFinanceiraId = request.SessaoDestinoId
            });

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoDestinoId);

            return Ok(new { mensagem = "Transferência realizada com sucesso." });
        }

        [HttpPost("transferir-meta")]
        public async Task<IActionResult> TransferirParaMeta([FromBody] BolsaTransferirMetaRequest request)
        {
            var bolsa = await _context.Bolsas
                .FirstOrDefaultAsync(b => b.UtilizadorId == request.UtilizadorId);

            if (bolsa == null || bolsa.Saldo < request.Valor)
                return BadRequest($"Saldo insuficiente na bolsa. Disponível: {bolsa?.Saldo ?? 0:N2} €.");

            var meta = await _context.Metas.FindAsync(request.MetaDestinoId);
            if (meta == null)
                return NotFound("Meta não encontrada.");

            bolsa.Saldo -= request.Valor;
            bolsa.DataAtualizacao = DateTime.Now;
            meta.ValorAtual += request.Valor;

            _context.MetaMovimentos.Add(new MetaMovimento
            {
                MetaId = request.MetaDestinoId,
                Tipo = "Deposito",
                Valor = request.Valor,
                Data = DateTime.Now
            });

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", 0);

            return Ok(new { mensagem = "Transferência para meta realizada com sucesso." });
        }
    }

    public class ApagarSessaoRequest
    {
        [Required] public int SessaoId { get; set; }
        [Required] public int UtilizadorId { get; set; }
        public bool GuardarNaBolsa { get; set; }
    }

    public class BolsaTransferirSessaoRequest
    {
        [Required] public int UtilizadorId { get; set; }
        [Required] public int SessaoDestinoId { get; set; }
        [Required][Range(0.01, double.MaxValue)] public decimal Valor { get; set; }
    }

    public class BolsaTransferirMetaRequest
    {
        [Required] public int UtilizadorId { get; set; }
        [Required] public int MetaDestinoId { get; set; }
        [Required][Range(0.01, double.MaxValue)] public decimal Valor { get; set; }
    }
}
