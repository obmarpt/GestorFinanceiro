using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

            var utilizadorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            {
                MensagemErro = "Sessão inválida. Faça login novamente.";
                return Page();
            }

            var sessao = new Data.Models.SessaoFinanceira
            {
                Nome = Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao)
                    ? null
                    : Descricao.Trim(),
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizadorId
            };

            _context.SessoesFinanceiras.Add(sessao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Sessão criada com sucesso.";

            return RedirectToPage("Index");
        }
    }
}