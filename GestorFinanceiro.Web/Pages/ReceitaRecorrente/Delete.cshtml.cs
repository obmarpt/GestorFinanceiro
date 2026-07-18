using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.ReceitaRecorrente
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
        public int DiaDoMes { get; set; }
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            var regra = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.Id == id && r.SessaoFinanceiraId == sessaoId);

            if (regra == null)
            {
                MensagemErro = "Regra não encontrada.";
                return Page();
            }

            Descricao = regra.Descricao;
            Valor = regra.Valor;
            DiaDoMes = regra.DiaDoMes;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;

            var regra = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.Id == id && r.SessaoFinanceiraId == sessaoId);

            if (regra == null)
            {
                MensagemErro = "Regra não encontrada.";
                return Page();
            }

            _context.ReceitasRecorrentes.Remove(regra);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Regra eliminada.";
            return RedirectToPage("Index", new { sessaoId });
        }
    }
}
