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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }

        [BindProperty]
        public int Id { get; set; }

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
        public DateTime Data { get; set; }

        [BindProperty]
        public int? CategoriaId { get; set; }

        public SelectList Categorias { get; set; } = null!;

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;

            try
            {
                var despesa = await _context.Despesas
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (despesa == null)
                {
                    MensagemErro = "Despesa não encontrada.";
                    await CarregarCategoriasAsync();
                    return Page();
                }

                if (despesa.SessaoFinanceiraId != sessaoId)
                {
                    return RedirectToPage(
                        "Index",
                        new { sessaoId });
                }

                Id = despesa.Id;
                Descricao = despesa.Descricao;
                Valor = despesa.Valor;
                Data = despesa.Data;
                CategoriaId = despesa.CategoriaId;
                await CarregarCategoriasAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Erro ao carregar a despesa: {ex.Message}";
                await CarregarCategoriasAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;
            await CarregarCategoriasAsync();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var despesa = await _context.Despesas
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (despesa == null)
                {
                    MensagemErro = "Despesa não encontrada.";
                    return Page();
                }

                if (CategoriaId.HasValue)
                {
                    var existe = await _context.Categorias.AnyAsync(c => c.Id == CategoriaId.Value);
                    if (!existe)
                    {
                        ModelState.AddModelError(nameof(CategoriaId), "Categoria inválida.");
                        return Page();
                    }
                }

                despesa.Descricao = Descricao.Trim();
                despesa.Valor = Valor;
                despesa.Data = Data;
                despesa.SessaoFinanceiraId = sessaoId;
                despesa.CategoriaId = CategoriaId;

                _context.Despesas.Update(despesa);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível atualizar a despesa: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Despesa atualizada com sucesso.";

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
