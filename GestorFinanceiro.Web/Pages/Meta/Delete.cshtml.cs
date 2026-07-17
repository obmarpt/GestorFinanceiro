using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.Meta
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string? NomeMeta { get; set; }

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
                    MensagemErro = "Meta não encontrada.";
                    return Page();
                }

                NomeMeta = meta.Nome;
                ValorAtual = meta.ValorAtual;

                return Page();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível carregar a meta: {ex.Message}";

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var meta = await _context.Metas
                    .FirstOrDefaultAsync(m => m.Id == Id);

                if (meta == null)
                {
                    TempData["Erro"] = "Meta não encontrada.";
                    return RedirectToPage("/Dashboard/Index");
                }

                _context.Metas.Remove(meta);

                await _context.SaveChangesAsync();

                TempData["Sucesso"] =
                    "Meta apagada com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] =
                    $"Não foi possível apagar a meta: {ex.Message}";
            }

            return RedirectToPage("/Dashboard/Index");
        }
    }
}