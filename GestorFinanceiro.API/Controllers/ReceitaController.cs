using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReceitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Receita
        [HttpGet]
        public IActionResult GetReceitas()
        {
            var receitas = _context.Receitas.ToList();
            return Ok(receitas);
        }

        // GET: api/Receita/5
        [HttpGet("{id}")]
        public IActionResult GetReceita(int id)
        {
            var receita = _context.Receitas.FirstOrDefault(r => r.Id == id);

            if (receita == null)
                return NotFound("Receita não encontrada.");

            return Ok(receita);
        }

        // POST: api/Receita
        [HttpPost]
        public async Task<IActionResult> CriarReceita([FromBody] Receita receita)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Receitas.Add(receita);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetReceita),
                new { id = receita.Id },
                receita
            );
        }

        // PUT: api/Receita/5
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

            return NoContent();
        }

        // DELETE: api/Receita/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarReceita(int id)
        {
            var receita = await _context.Receitas.FindAsync(id);

            if (receita == null)
                return NotFound("Receita não encontrada.");

            _context.Receitas.Remove(receita);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
