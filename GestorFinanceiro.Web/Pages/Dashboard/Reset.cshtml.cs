using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Dashboard
{
    [Authorize]
    public class ResetModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "A password é obrigatória.")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Tipo { get; set; } = "tudo";

        public string? MensagemErro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                MensagemErro = "A password é obrigatória.";
                return Page();
            }

            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Id == utilizadorId);

            if (utilizador == null)
            {
                MensagemErro = "Utilizador não encontrado.";
                return Page();
            }

            if (utilizador.PasswordHash != Password)
            {
                MensagemErro = "Password incorreta.";
                return Page();
            }

            var sessaoIds = await _context.SessoesFinanceiras
                .Where(s => s.UtilizadorId == utilizadorId)
                .Select(s => s.Id)
                .ToListAsync();

            if (Tipo == "receitas" || Tipo == "tudo")
            {
                var receitas = _context.Receitas
                    .Where(r => sessaoIds.Contains(r.SessaoFinanceiraId));
                _context.Receitas.RemoveRange(receitas);
            }

            if (Tipo == "despesas" || Tipo == "tudo")
            {
                var despesas = _context.Despesas
                    .Where(d => sessaoIds.Contains(d.SessaoFinanceiraId));
                _context.Despesas.RemoveRange(despesas);
            }

            await _context.SaveChangesAsync();

            var mensagem = Tipo switch
            {
                "receitas" => "Todas as receitas foram apagadas com sucesso.",
                "despesas" => "Todas as despesas foram apagadas com sucesso.",
                _ => "Todos os movimentos foram apagados com sucesso."
            };

            TempData["Sucesso"] = mensagem;
            return RedirectToPage("/Dashboard/Index");
        }
    }
}