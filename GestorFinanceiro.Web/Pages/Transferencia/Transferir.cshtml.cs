using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Transferencia
{
    [Authorize]
    public class TransferirModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TransferirModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Selecione a sessão de origem.")]
        public int SessaoOrigemId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Selecione a sessão de destino.")]
        public int SessaoDestinoId { get; set; }

        [BindProperty]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [BindProperty]
        public string? Descricao { get; set; }

        public IList<SessaoResumoViewModel> Resumos { get; set; } = [];
        public SelectList SessoesOrigem { get; set; } = null!;
        public SelectList SessoesDestino { get; set; } = null!;
        public decimal? SaldoOrigem { get; set; }
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int? origemId, int? destinoId, decimal? valor)
        {
            if (!await CarregarAsync())
                return Page();

            if (origemId.HasValue) SessaoOrigemId = origemId.Value;
            if (destinoId.HasValue) SessaoDestinoId = destinoId.Value;
            if (valor.HasValue && valor > 0) Valor = valor.Value;

            SaldoOrigem = Resumos.FirstOrDefault(r => r.SessaoId == SessaoOrigemId)?.Saldo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await CarregarAsync())
                return Page();

            if (SessaoOrigemId == SessaoDestinoId)
                ModelState.AddModelError(nameof(SessaoDestinoId), "Origem e destino devem ser diferentes.");

            SaldoOrigem = Resumos.FirstOrDefault(r => r.SessaoId == SessaoOrigemId)?.Saldo ?? 0;
            if (Valor > SaldoOrigem)
                ModelState.AddModelError(nameof(Valor), $"Valor superior ao saldo ({SaldoOrigem:N2} €).");

            if (!ModelState.IsValid)
                return Page();

            var origem = await _context.SessoesFinanceiras.FindAsync(SessaoOrigemId);
            var destino = await _context.SessoesFinanceiras.FindAsync(SessaoDestinoId);

            if (origem == null || destino == null)
            {
                MensagemErro = "Sessão de origem ou destino não encontrada.";
                return Page();
            }

            var motivo = string.IsNullOrWhiteSpace(Descricao) ? "Transferência de saldo" : Descricao.Trim();
            var data = DateTime.Now;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Despesas.Add(new Data.Models.Despesa
                {
                    Descricao = $"{motivo} → {destino.Nome}",
                    Valor = Valor,
                    Data = data,
                    SessaoFinanceiraId = SessaoOrigemId
                });

                _context.Receitas.Add(new Data.Models.Receita
                {
                    Descricao = $"{motivo} ← {origem.Nome}",
                    Valor = Valor,
                    Data = data,
                    SessaoFinanceiraId = SessaoDestinoId
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MensagemErro = $"Não foi possível transferir: {ex.Message}";
                return Page();
            }

            TempData["Sucesso"] = "Saldo transferido com sucesso.";
            return RedirectToPage("/Dashboard/Index");
        }

        private async Task<bool> CarregarAsync()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var sessoes = await _context.SessoesFinanceiras
                .Where(s => s.UtilizadorId == utilizadorId)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();

            if (sessoes.Count < 2)
            {
                MensagemErro = "Precisa de pelo menos duas sessões para transferir saldo.";
                return false;
            }

            var ids = sessoes.Select(s => s.Id).ToList();

            var receitas = await _context.Receitas
                .Where(r => ids.Contains(r.SessaoFinanceiraId))
                .ToListAsync();

            var despesas = await _context.Despesas
                .Where(d => ids.Contains(d.SessaoFinanceiraId))
                .ToListAsync();

            Resumos = sessoes.Select(s => new SessaoResumoViewModel
            {
                SessaoId = s.Id,
                Nome = s.Nome,
                Descricao = s.Descricao,
                DataCriacao = s.DataCriacao,
                TotalReceitas = receitas.Where(r => r.SessaoFinanceiraId == s.Id).Sum(r => r.Valor),
                TotalDespesas = despesas.Where(d => d.SessaoFinanceiraId == s.Id).Sum(d => d.Valor)
            }).ToList();

            SessoesOrigem = new SelectList(Resumos, nameof(SessaoResumoViewModel.SessaoId), nameof(SessaoResumoViewModel.Nome), SessaoOrigemId);
            SessoesDestino = new SelectList(Resumos, nameof(SessaoResumoViewModel.SessaoId), nameof(SessaoResumoViewModel.Nome), SessaoDestinoId);
            return true;
        }
    }
}