using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Meta
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

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
        public decimal ValorAtual { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var meta = await _context.Metas
                    .FirstOrDefaultAsync(m => m.Id == Id);

                if (meta == null)
                {
                    MensagemErro = "Poupança não encontrada.";
                    return Page();
                }

                Nome = meta.Nome;
                Descricao = meta.Descricao;
                ValorAlvo = meta.ValorAlvo;
                ValorAtual = meta.ValorAtual;

                return Page();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível carregar a poupança: {ex.Message}";

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizadorId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var meta = await _context.Metas
                    .FirstOrDefaultAsync(m => m.Id == Id);

                if (meta == null)
                {
                    MensagemErro = "Poupança não encontrada.";
                    return Page();
                }

                meta.Nome = Nome.Trim();
                meta.Descricao = Descricao?.Trim();
                meta.ValorAlvo = ValorAlvo;
                meta.ValorAtual = ValorAtual;
                meta.UtilizadorId = utilizadorId;

                _context.Metas.Update(meta);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Conta Poupança atualizada com sucesso.";

                return RedirectToPage("/Dashboard/Index");
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível guardar as alterações: {ex.Message}";

                return Page();
            }
        }
    }
}