using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizadorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UtilizadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult GetUtilizador(int id)
        {
            var utilizador = _context.Utilizadores.Find(id);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            return Ok(utilizador);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarUtilizador(int id, [FromBody] Utilizador utilizador)
        {
            if (id != utilizador.Id)
                return BadRequest("O ID do utilizador é inválido.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _context.Utilizadores.FindAsync(id);
            if (existente == null)
                return NotFound("Utilizador não encontrado.");

            if (await _context.Utilizadores.AnyAsync(u => u.Email == utilizador.Email && u.Id != id))
                return BadRequest("Já existe uma conta com este email.");

            if (await _context.Utilizadores.AnyAsync(u => u.Username == utilizador.Username && u.Id != id))
                return BadRequest("Já existe uma conta com este username.");

            existente.Nome = utilizador.Nome.Trim();
            existente.Username = utilizador.Username.Trim();
            existente.Email = utilizador.Email.Trim();
            existente.ImagemPerfil = utilizador.ImagemPerfil;

            if (!string.IsNullOrWhiteSpace(utilizador.PasswordHash))
                existente.PasswordHash = utilizador.PasswordHash;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
