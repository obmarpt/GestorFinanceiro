using GestorFinanceiro.API.Models;
using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MetaController(ApplicationDbContext context)
        {
            _context = context;
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
            if (meta == null)
                return NotFound();
            return Ok(meta);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] MetaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            return CreatedAtAction(nameof(GetById), new { id = meta.Id }, meta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] MetaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null)
                return NotFound();

            meta.Nome = request.Nome.Trim();
            meta.Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim();
            meta.ValorAlvo = request.ValorAlvo;
            meta.ValorAtual = request.ValorAtual;

            await _context.SaveChangesAsync();
            return Ok(meta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Apagar(int id)
        {
            var meta = await _context.Metas.FindAsync(id);
            if (meta == null)
                return NotFound();

            _context.Metas.Remove(meta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/depositar")]
        public async Task<IActionResult> Depositar(int id, [FromBody] MetaDepositarRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null)
                return NotFound("Meta não encontrada.");

            if (request.Valor <= 0)
                return BadRequest("O valor deve ser maior que zero.");

            meta.ValorAtual += request.Valor;

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Depósito registado com sucesso." });
        }

        [HttpPost("{id}/levantar")]
        public async Task<IActionResult> Levantar(int id, [FromBody] MetaLevantarRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var meta = await _context.Metas.FindAsync(id);
            if (meta == null)
                return NotFound("Meta não encontrada.");

            if (request.Valor > meta.ValorAtual)
                return BadRequest($"Valor superior ao disponível na meta ({meta.ValorAtual:N2} €).");

            meta.ValorAtual -= request.Valor;

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Levantamento registado com sucesso." });
        }
    }
}
