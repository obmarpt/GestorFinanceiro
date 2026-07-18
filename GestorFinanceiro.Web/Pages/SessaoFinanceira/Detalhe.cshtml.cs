using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Web.Helpers;
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

        public IList<DespesaCategoriaResumo> DespesasPorCategoria { get; set; } = [];

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

            await ReceitaRecorrenteHelper.GerarReceitasPendentesAsync(_context, sessaoId: id);

            Nome = sessao.Nome;
            Descricao = sessao.Descricao;
            DataCriacao = sessao.DataCriacao;

            Receitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == id)
                .OrderByDescending(r => r.Data)
                .Take(5)
                .ToListAsync();

            Despesas = await _context.Despesas
                .Include(d => d.Categoria)
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

            DespesasPorCategoria = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == id && d.Categoria != null)
                .Include(d => d.Categoria)
                .GroupBy(d => d.Categoria!.Nome)
                .Select(g => new DespesaCategoriaResumo
                {
                    Categoria = g.Key,
                    Total = g.Sum(d => d.Valor)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return Page();
        }

        public class DespesaCategoriaResumo
        {
            public string Categoria { get; set; } = string.Empty;
            public decimal Total { get; set; }
        }
    }
}