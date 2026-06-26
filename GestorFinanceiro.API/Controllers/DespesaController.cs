using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DespesaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DespesaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Despesa
        [HttpGet]
        public IActionResult GetDespesas()
        {
            var despesas = _context.Despesas
                .Include(d => d.Categoria)
                .ToList();

            return Ok(despesas);
        }

        // GET: api/Despesa/5
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

        // POST: api/Despesa
        [HttpPost]
        public async Task<IActionResult> CriarDespesa([FromBody] Despesa despesa)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            despesa.Data = despesa.Data.Date + DateTime.Now.TimeOfDay;

            _context.Despesas.Add(despesa);
            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDespesa),
                new { id = despesa.Id },
                despesa
            );
        }

        // PUT: api/Despesa/5
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

            return NoContent();
        }

        // DELETE: api/Despesa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDespesa(int id)
        {
            var despesa = await _context.Despesas.FindAsync(id);

            if (despesa == null)
                return NotFound("Despesa não encontrada.");

            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
