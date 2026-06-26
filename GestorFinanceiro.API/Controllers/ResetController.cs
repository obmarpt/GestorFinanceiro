using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResetController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("apagar-movimentos")]
        public async Task<IActionResult> ApagarMovimentos([FromBody] ResetRequest request)
        {
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Id == request.UtilizadorId);

            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            if (utilizador.PasswordHash != request.Password)
                return BadRequest("Password incorreta.");

            var sessaoIds = await _context.SessoesFinanceiras
                .Where(s => s.UtilizadorId == request.UtilizadorId)
                .Select(s => s.Id)
                .ToListAsync();

            if (request.Tipo == "receitas" || request.Tipo == "tudo")
            {
                var receitas = _context.Receitas
                    .Where(r => sessaoIds.Contains(r.SessaoFinanceiraId));
                _context.Receitas.RemoveRange(receitas);
            }

            if (request.Tipo == "despesas" || request.Tipo == "tudo")
            {
                var despesas = _context.Despesas
                    .Where(d => sessaoIds.Contains(d.SessaoFinanceiraId));
                _context.Despesas.RemoveRange(despesas);
            }

            await _context.SaveChangesAsync();

            var mensagem = request.Tipo switch
            {
                "receitas" => "Todas as receitas foram apagadas com sucesso.",
                "despesas" => "Todas as despesas foram apagadas com sucesso.",
                _ => "Todos os movimentos foram apagados com sucesso."
            };

            return Ok(new { mensagem });
        }
    }

    public class ResetRequest
    {
        [Required] public int UtilizadorId { get; set; }
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string Tipo { get; set; } = "tudo"; // "receitas", "despesas", "tudo"
    }
}
