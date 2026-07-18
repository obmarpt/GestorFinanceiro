using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.ReceitaRecorrente
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }
        public string SessaoNome { get; set; } = string.Empty;
        public IList<Data.Models.ReceitaRecorrente> Regras { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;

            var sessao = await _context.SessoesFinanceiras
                .FirstOrDefaultAsync(s => s.Id == sessaoId);

            if (sessao == null)
            {
                MensagemErro = "Sessão financeira não encontrada.";
                return Page();
            }

            SessaoNome = sessao.Nome;
            Regras = await _context.ReceitasRecorrentes
                .Where(r => r.SessaoFinanceiraId == sessaoId)
                .OrderBy(r => r.DiaDoMes)
                .ThenBy(r => r.Descricao)
                .ToListAsync();

            return Page();
        }
    }
}
