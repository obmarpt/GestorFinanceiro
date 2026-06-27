using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessaoFinanceirasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SessaoFinanceirasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSessoesFinanceiras()
        {
            var sessoes = _context.SessoesFinanceiras
                .Include(s => s.Utilizador)
                .ToList();

            return Ok(sessoes);
        }

        [HttpGet("{id}")]
        public IActionResult GetSessaoFinanceira(int id)
        {
            var sessao = _context.SessoesFinanceiras
                .Include(s => s.Receitas)
                .Include(s => s.Despesas)
                .Include(s => s.ReceitasRecorrentes)
                .FirstOrDefault(s => s.Id == id);

            if (sessao == null)
                return NotFound("Sessão financeira não encontrada.");

            return Ok(sessao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarSessaoFinanceira([FromBody] SessaoFinanceira sessao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.SessoesFinanceiras.Add(sessao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSessaoFinanceira),
                new { id = sessao.Id },
                sessao
            );
        }

        [HttpPut("{id}")]
        public IActionResult EditarSessaoFinanceira(int id, [FromBody] SessaoFinanceira sessao)
        {
            if (id != sessao.Id)
                return BadRequest("O ID da sessão é inválido.");

            var sessaoExistente = _context.SessoesFinanceiras.Find(id);

            if (sessaoExistente == null)
                return NotFound("Sessão financeira não encontrada.");

            sessaoExistente.Nome = sessao.Nome;
            sessaoExistente.Descricao = sessao.Descricao;

            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarSessaoFinanceira(int id)
        {
            var sessao = _context.SessoesFinanceiras
                .Include(s => s.Receitas)
                .Include(s => s.Despesas)
                .FirstOrDefault(s => s.Id == id);

            if (sessao == null)
                return NotFound("Sessão financeira não encontrada.");

            if (sessao.Receitas.Any() || sessao.Despesas.Any())
                return BadRequest(
                    "Não é possível eliminar a sessão porque contém receitas ou despesas associadas."
                );

            _context.SessoesFinanceiras.Remove(sessao);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
