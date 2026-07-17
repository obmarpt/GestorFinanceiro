using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class DetalheModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetalheModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }

        public decimal TotalReceitas { get; set; }

        public decimal TotalDespesas { get; set; }

        public decimal Saldo => TotalReceitas - TotalDespesas;

        public IList<Data.Models.Receita> Receitas { get; set; } = [];

        public IList<Data.Models.Despesa> Despesas { get; set; } = [];

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var sessao = await _context.SessoesFinanceiras
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sessao == null)
            {
                MensagemErro = "Sessão não encontrada.";
                return Page();
            }

            Nome = sessao.Nome;
            Descricao = sessao.Descricao;
            DataCriacao = sessao.DataCriacao;

            Receitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == id)
                .OrderByDescending(r => r.Data)
                .Take(5)
                .ToListAsync();

            Despesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == id)
                .OrderByDescending(d => d.Data)
                .Take(5)
                .ToListAsync();

            TotalReceitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == id)
                .SumAsync(r => r.Valor);

            TotalDespesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == id)
                .SumAsync(d => d.Valor);

            return Page();
        }
    }
}