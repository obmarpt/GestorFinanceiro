using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SessaoResumoViewModel> ResumoPorSessao { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var sessoes = await _context.SessoesFinanceiras
                    .Where(s => s.UtilizadorId == utilizadorId)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToListAsync();

                if (sessoes.Count == 0)
                    return Page();

                var ids = sessoes.Select(s => s.Id).ToList();

                var receitas = await _context.Receitas
                    .Where(r => ids.Contains(r.SessaoFinanceiraId))
                    .ToListAsync();

                var despesas = await _context.Despesas
                    .Where(d => ids.Contains(d.SessaoFinanceiraId))
                    .ToListAsync();

                ResumoPorSessao = sessoes.Select(s => new SessaoResumoViewModel
                {
                    SessaoId = s.Id,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    DataCriacao = s.DataCriacao,
                    TotalReceitas = receitas.Where(r => r.SessaoFinanceiraId == s.Id).Sum(r => r.Valor),
                    TotalDespesas = despesas.Where(d => d.SessaoFinanceiraId == s.Id).Sum(d => d.Valor)
                })
                .OrderByDescending(r => r.DataCriacao)
                .ToList();
            }
            catch (Exception ex)
            {
                MensagemErro = $"Não foi possível carregar as sessões: {ex.Message}";
            }

            return Page();
        }
    }
}