using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var utilizadores = await _context.Utilizadores
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.ImagemPerfil,
                    TotalSessoes = _context.SessoesFinanceiras.Count(s => s.UtilizadorId == u.Id)
                })
                .ToListAsync();

            return Ok(utilizadores);
        }

        [HttpGet("{id}")]
        public IActionResult GetUtilizador(int id)
        {
            var utilizador = _context.Utilizadores.Find(id);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            return Ok(utilizador);
        }
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> AlterarRole(int id, [FromBody] AlterarRoleRequest request)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            utilizador.Role = request.Role;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class AlterarRoleRequest
        {
            public string Role { get; set; } = string.Empty;
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            if (utilizador.Role == "Admin")
                return BadRequest("Não é possível apagar uma conta de administrador.");

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
