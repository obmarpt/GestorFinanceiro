using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.Despesa
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

        public IList<Data.Models.Despesa> Despesas { get; set; } = [];

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;

            try
            {
                var sessao = await _context.SessoesFinanceiras
                    .FirstOrDefaultAsync(s => s.Id == sessaoId);

                if (sessao == null)
                {
                    MensagemErro = "Sessão financeira não encontrada.";
                    return Page();
                }

                SessaoNome = sessao.Nome;

                Despesas = await _context.Despesas
                    .Include(d => d.Categoria)
                    .Where(d => d.SessaoFinanceiraId == sessaoId)
                    .OrderByDescending(d => d.Data)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível carregar as despesas: {ex.Message}";
            }

            return Page();
        }
    }
}