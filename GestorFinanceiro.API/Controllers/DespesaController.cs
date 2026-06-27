using GestorFinanceiro.API.Hubs;
using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DespesaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<FinanceHub> _hubContext;

        public DespesaController(ApplicationDbContext context, IHubContext<FinanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetDespesas()
        {
            var despesas = _context.Despesas
                .Include(d => d.Categoria)
                .ToList();
            return Ok(despesas);
        }

        [HttpGet("{id}")]
        public IActionResult GetDespesa(int id)
        {
            var despesa = _context.Despesas
                .Include(d => d.Categoria)
                .FirstOrDefault(d => d.Id == id);
            if (despesa == null)
                return NotFound("Despesa não encontrada.");
            return Ok(despesa);
        }

        [HttpPost]
        public async Task<IActionResult> CriarDespesa([FromBody] Despesa despesa)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            despesa.Data = despesa.Data.Date + DateTime.Now.TimeOfDay;
            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", despesa.SessaoFinanceiraId);

            return CreatedAtAction(nameof(GetDespesa), new { id = despesa.Id }, despesa);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarDespesa(int id, [FromBody] Despesa despesa)
        {
            if (id != despesa.Id)
                return BadRequest("O ID da despesa é inválido.");

            var despesaExistente = await _context.Despesas.FindAsync(id);
            if (despesaExistente == null)
                return NotFound("Despesa não encontrada.");

            despesaExistente.Descricao = despesa.Descricao;
            despesaExistente.Valor = despesa.Valor;
            despesaExistente.Data = despesa.Data;
            despesaExistente.CategoriaId = despesa.CategoriaId;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", despesaExistente.SessaoFinanceiraId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDespesa(int id)
        {
            var despesa = await _context.Despesas.FindAsync(id);
            if (despesa == null)
                return NotFound("Despesa não encontrada.");

            var sessaoId = despesa.SessaoFinanceiraId;
            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", sessaoId);

            return NoContent();
        }
    }
}
