using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UtilizadoresModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UtilizadorResumo> Utilizadores { get; set; } = [];

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Utilizadores = await _context.Utilizadores
                    .Select(u => new UtilizadorResumo
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        TotalSessoes = u.SessoesFinanceiras.Count
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao carregar os utilizadores: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostTornarAdminAsync(int id)
        {
            try
            {
                var utilizador = await _context.Utilizadores
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (utilizador == null)
                {
                    TempData["Erro"] = "Utilizador não encontrado.";
                    return RedirectToPage();
                }

                utilizador.Role = "Admin";

                _context.Utilizadores.Update(utilizador);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Utilizador promovido a Admin com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] =
                    $"Erro ao alterar o role: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApagarAsync(int id)
        {
            try
            {
                var utilizador = await _context.Utilizadores
                    .Include(u => u.SessoesFinanceiras)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (utilizador == null)
                {
                    TempData["Erro"] =
                        "Utilizador não encontrado.";

                    return RedirectToPage();
                }

                _context.Utilizadores.Remove(utilizador);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Utilizador apagado com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] =
                    $"Erro ao apagar o utilizador: {ex.Message}";
            }

            return RedirectToPage();
        }

        public class UtilizadorResumo
        {
            public int Id { get; set; }

            public string Nome { get; set; } = string.Empty;

            public string Username { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Role { get; set; } = string.Empty;

            public int TotalSessoes { get; set; }
        }
    }
}