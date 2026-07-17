using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Meta
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

        [BindProperty]
        [Required]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O valor alvo deve ser maior que zero.")]
        public decimal ValorAlvo { get; set; }

        [BindProperty]
        [Range(0, double.MaxValue)]
        public decimal ValorAtual { get; set; } = 0;

        public string? MensagemErro { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizadorId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var meta = new Data.Models.Meta
            {
                Nome = Nome.Trim(),
                Descricao = Descricao?.Trim(),
                ValorAlvo = ValorAlvo,
                ValorAtual = ValorAtual,
                UtilizadorId = utilizadorId
            };

            try
            {
                _context.Metas.Add(meta);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Meta criada com sucesso.";

                return RedirectToPage("/Dashboard/Index");
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível criar a meta: {ex.Message}";

                return Page();
            }
        }
    }
}