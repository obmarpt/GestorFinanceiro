using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.Categoria
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CategoriaItem> Categorias { get; set; } = [];

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
        public string NovaCategoriaNome { get; set; } = string.Empty;

        public string? MensagemErro { get; set; }

        public async Task OnGetAsync()
        {
            await CarregarAsync();
        }

        public async Task<IActionResult> OnPostCriarAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarAsync();
                return Page();
            }

            var nome = NovaCategoriaNome.Trim();
            var existe = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == nome.ToLower());

            if (existe)
            {
                MensagemErro = "Já existe uma categoria com esse nome.";
                await CarregarAsync();
                return Page();
            }

            _context.Categorias.Add(new Data.Models.Categoria { Nome = nome });
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Categoria criada.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                TempData["Erro"] = "Categoria não encontrada.";
                return RedirectToPage();
            }

            var emUso = await _context.Despesas.AnyAsync(d => d.CategoriaId == id);
            if (emUso)
            {
                TempData["Erro"] = "Não é possível eliminar: existem despesas associadas.";
                return RedirectToPage();
            }

            var ligacoes = _context.SessaoFinanceiraCategorias.Where(sc => sc.CategoriaId == id);
            _context.SessaoFinanceiraCategorias.RemoveRange(ligacoes);
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Categoria eliminada.";
            return RedirectToPage();
        }

        private async Task CarregarAsync()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaItem
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    TotalDespesas = _context.Despesas.Count(d => d.CategoriaId == c.Id)
                })
                .ToListAsync();
        }

        public class CategoriaItem
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public int TotalDespesas { get; set; }
        }
    }
}
