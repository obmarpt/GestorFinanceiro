using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string? Descricao { get; set; }

        public string? MensagemErro { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizador = await ObterUtilizadorLogadoAsync();
            if (utilizador == null)
                return RedirectToPage("/Login");

            var sessao = new Data.Models.SessaoFinanceira
            {
                Nome = Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizador.Id
            };

            _context.SessoesFinanceiras.Add(sessao);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
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
