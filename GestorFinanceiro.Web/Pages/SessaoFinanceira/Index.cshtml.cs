using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Data.Models.SessaoFinanceira> Sessoes { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var utilizador = await ObterUtilizadorLogadoAsync();
            if (utilizador == null)
                return RedirectToPage("/Login");

            Sessoes = await _context.SessoesFinanceiras
                .Where(s => s.UtilizadorId == utilizador.Id)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();

            return Page();
        }

        private async Task<Utilizador?> ObterUtilizadorLogadoAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
                return null;

            return await _context.Utilizadores
                .FirstOrDefaultAsync(u =>
                    u.Username == username ||
                    u.Email == email);
        }
    }
}
