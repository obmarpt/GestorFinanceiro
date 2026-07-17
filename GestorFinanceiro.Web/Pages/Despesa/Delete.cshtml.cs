using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.Despesa
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
                var despesa = await _context.Despesas
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (despesa == null)
                {
                    MensagemErro = "Despesa não encontrada.";
                    return Page();
                }

                if (despesa.SessaoFinanceiraId != sessaoId)
                {
                    return RedirectToPage(
                        "Index",
                        new { sessaoId });
                }

                Descricao = despesa.Descricao;
                Valor = despesa.Valor;
                Data = despesa.Data;
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Erro ao carregar a despesa: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            try
            {
                var despesa = await _context.Despesas
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (despesa == null)
                {
                    MensagemErro = "Despesa não encontrada.";
                    return Page();
                }

                _context.Despesas.Remove(despesa);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível eliminar a despesa: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Despesa eliminada com sucesso.";

            return RedirectToPage(
                "Index",
                new { sessaoId });
        }
    }
}