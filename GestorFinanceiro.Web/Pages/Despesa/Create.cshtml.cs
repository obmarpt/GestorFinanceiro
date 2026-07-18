using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.Despesa
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O valor deve ser superior a zero.")]
        public decimal Valor { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        [BindProperty]
        public int? CategoriaId { get; set; }

        public SelectList Categorias { get; set; } = null!;

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;
            await CarregarCategoriasAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId)
        {
            SessaoId = sessaoId;
            await CarregarCategoriasAsync();

            if (!ModelState.IsValid)
                return Page();

            if (CategoriaId.HasValue)
            {
                var existe = await _context.Categorias.AnyAsync(c => c.Id == CategoriaId.Value);
                if (!existe)
                {
                    ModelState.AddModelError(nameof(CategoriaId), "Categoria inválida.");
                    return Page();
                }
            }

            var despesa = new Data.Models.Despesa
            {
                Descricao = Descricao.Trim(),
                Valor = Valor,
                Data = Data,
                SessaoFinanceiraId = sessaoId,
                CategoriaId = CategoriaId
            };

            try
            {
                _context.Despesas.Add(despesa);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível criar a despesa: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Despesa criada com sucesso.";

            return RedirectToPage(
                "Index",
                new { sessaoId });
        }

        private async Task CarregarCategoriasAsync()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .ToListAsync();

            Categorias = new SelectList(categorias, "Id", "Nome", CategoriaId);
        }
    }
}
