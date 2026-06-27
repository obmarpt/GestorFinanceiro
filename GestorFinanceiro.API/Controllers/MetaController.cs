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
    public class MetaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<FinanceHub> _hubContext;

        public MetaController(ApplicationDbContext context, IHubContext<FinanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var metas = await _context.Metas.ToListAsync();
            return Ok(metas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var meta = await _context.Metas.FindAsync(id);
            if (meta == null) return NotFound();
            return Ok(meta);
        }

        [HttpGet("movimentos")]
        public async Task<IActionResult> GetMovimentos()
        {
            var movimentos = await _context.MetaMovimentos
                .Include(m => m.Meta)
                .OrderByDescending(m => m.Data)
                .ToListAsync();
            return Ok(movimentos);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] MetaRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var meta = new Meta
            {
                Nome = request.Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim(),
                ValorAlvo = request.ValorAlvo,
                ValorAtual = request.ValorAtual,
                UtilizadorId = request.UtilizadorId,
                DataCriacao = DateTime.Now
            };

            _context.Metas.Add(meta);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", meta.Id);

            return CreatedAtAction(nameof(GetById), new { id = meta.Id }, meta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] MetaRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null) return NotFound();

            meta.Nome = request.Nome.Trim();
            meta.Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim();
            meta.ValorAlvo = request.ValorAlvo;
            meta.ValorAtual = request.ValorAtual;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", meta.Id);

            return Ok(meta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Apagar(int id)
        {
            var meta = await _context.Metas.FindAsync(id);
            if (meta == null) return NotFound();

            _context.Metas.Remove(meta);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", id);

            return NoContent();
        }

        [HttpPost("{id}/depositar")]
        public async Task<IActionResult> Depositar(int id, [FromBody] MetaDepositarRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null) return NotFound("Poupança não encontrada.");

            var sessao = await _context.SessoesFinanceiras.FindAsync(request.SessaoOrigemId);
            if (sessao == null) return NotFound("Sessão não encontrada.");

            var totalReceitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == request.SessaoOrigemId)
                .SumAsync(r => r.Valor);

            var totalDespesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == request.SessaoOrigemId)
                .SumAsync(d => d.Valor);

            var saldo = totalReceitas - totalDespesas;

            if (request.Valor > saldo)
                return BadRequest($"Saldo insuficiente na sessão. Disponível: {saldo:N2} €.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Despesas.Add(new Despesa
            {
                Descricao = $"Depósito para poupança: {meta.Nome}",
                Valor = request.Valor,
                Data = DateTime.Now,
                SessaoFinanceiraId = request.SessaoOrigemId
            });

            meta.ValorAtual += request.Valor;

            _context.MetaMovimentos.Add(new MetaMovimento
            {
                MetaId = id,
                Tipo = "Deposito",
                Valor = request.Valor,
                Data = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoOrigemId);

            return Ok(new { mensagem = "Depósito realizado com sucesso." });
        }

        [HttpPost("{id}/levantar")]
        public async Task<IActionResult> Levantar(int id, [FromBody] MetaLevantarRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null) return NotFound("Poupança não encontrada.");

            if (request.Valor > meta.ValorAtual)
                return BadRequest($"Valor superior ao disponível na poupança ({meta.ValorAtual:N2} €).");

            var sessao = await _context.SessoesFinanceiras.FindAsync(request.SessaoDestinoId);
            if (sessao == null) return NotFound("Sessão não encontrada.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Receitas.Add(new Receita
            {
                Descricao = $"Levantamento de poupança: {meta.Nome}",
                Valor = request.Valor,
                Data = DateTime.Now,
                SessaoFinanceiraId = request.SessaoDestinoId
            });

            meta.ValorAtual -= request.Valor;

            _context.MetaMovimentos.Add(new MetaMovimento
            {
                MetaId = id,
                Tipo = "Levantamento",
                Valor = request.Valor,
                Data = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", request.SessaoDestinoId);

            return Ok(new { mensagem = "Levantamento realizado com sucesso." });
        }

        [HttpDelete("movimentos/{id}")]
        public async Task<IActionResult> ApagarMovimento(int id)
        {
            var movimento = await _context.MetaMovimentos
                .Include(m => m.Meta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimento == null)
                return NotFound("Movimento não encontrado.");

            if (movimento.Tipo == "Deposito")
                movimento.Meta.ValorAtual -= movimento.Valor;
            else if (movimento.Tipo == "Levantamento")
                movimento.Meta.ValorAtual += movimento.Valor;

            if (movimento.Meta.ValorAtual < 0)
                movimento.Meta.ValorAtual = 0;

            _context.MetaMovimentos.Remove(movimento);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", movimento.MetaId);

            return NoContent();
        }
    }
}
