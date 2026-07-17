using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.Receita
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }

        public int Id { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public DateTime Data { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            try
            {
                var receita = await _context.Receitas
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (receita == null)
                {
                    MensagemErro = "Receita não encontrada.";
                    return Page();
                }

                if (receita.SessaoFinanceiraId != sessaoId)
                {
                    return RedirectToPage(
                        "Index",
                        new { sessaoId });
                }

                Descricao = receita.Descricao;
                Valor = receita.Valor;
                Data = receita.Data;
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Erro ao carregar a receita: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            try
            {
                var receita = await _context.Receitas
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (receita == null)
                {
                    MensagemErro = "Receita não encontrada.";
                    return Page();
                }

                _context.Receitas.Remove(receita);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível eliminar a receita: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Receita eliminada com sucesso.";

            return RedirectToPage(
                "Index",
                new { sessaoId });
        }
    }
}