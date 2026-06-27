using GestorFinanceiro.Web.Hubs;
using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<FinanceHub> _hubContext;

        public ReceitaController(ApplicationDbContext context, IHubContext<FinanceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetReceitas()
        {
            var receitas = _context.Receitas.ToList();
            return Ok(receitas);
        }

        [HttpGet("{id}")]
        public IActionResult GetReceita(int id)
        {
            var receita = _context.Receitas.FirstOrDefault(r => r.Id == id);
            if (receita == null)
                return NotFound("Receita não encontrada.");
            return Ok(receita);
        }

        [HttpPost]
        public async Task<IActionResult> CriarReceita([FromBody] Receita receita)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            receita.Data = receita.Data.Date + DateTime.Now.TimeOfDay;
            _context.Receitas.Add(receita);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", receita.SessaoFinanceiraId);

            return CreatedAtAction(nameof(GetReceita), new { id = receita.Id }, receita);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarReceita(int id, [FromBody] Receita receita)
        {
            if (id != receita.Id)
                return BadRequest("O ID da receita é inválido.");

            var receitaExistente = await _context.Receitas.FindAsync(id);
            if (receitaExistente == null)
                return NotFound("Receita não encontrada.");

            receitaExistente.Descricao = receita.Descricao;
            receitaExistente.Valor = receita.Valor;
            receitaExistente.Data = receita.Data;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", receitaExistente.SessaoFinanceiraId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarReceita(int id)
        {
            var receita = await _context.Receitas.FindAsync(id);
            if (receita == null)
                return NotFound("Receita não encontrada.");

            var sessaoId = receita.SessaoFinanceiraId;
            _context.Receitas.Remove(receita);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ResumoAtualizado", sessaoId);

            return NoContent();
        }
    }
}
