using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string? NomeSessao { get; set; }

        public decimal Saldo { get; set; }

        public string? MensagemErro { get; set; }

        [BindProperty]
        public bool GuardarNaBolsa { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var sessao = await _context.SessoesFinanceiras
                .FirstOrDefaultAsync(s => s.Id == Id);

            if (sessao == null)
            {
                MensagemErro = "Sessão não encontrada.";
                return Page();
            }

            NomeSessao = sessao.Nome;

            var totalReceitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == Id)
                .SumAsync(r => r.Valor);

            var totalDespesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == Id)
                .SumAsync(d => d.Valor);

            Saldo = totalReceitas - totalDespesas;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var utilizadorIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            {
                TempData["Erro"] =
                    "Sessão inválida. Faça login novamente.";

                return RedirectToPage("/SessaoFinanceira/Index");
            }

            var sessao = await _context.SessoesFinanceiras
                .FirstOrDefaultAsync(s => s.Id == Id);

            if (sessao == null)
            {
                TempData["Erro"] = "Sessão não encontrada.";

                return RedirectToPage("/SessaoFinanceira/Index");
            }

            var receitas = await _context.Receitas
                .Where(r => r.SessaoFinanceiraId == Id)
                .ToListAsync();

            var despesas = await _context.Despesas
                .Where(d => d.SessaoFinanceiraId == Id)
                .ToListAsync();

            var saldo = receitas.Sum(r => r.Valor)
                        - despesas.Sum(d => d.Valor);

            if (GuardarNaBolsa && saldo > 0)
            {
                var bolsa = await _context.Bolsas
                    .FirstOrDefaultAsync(b => b.UtilizadorId == utilizadorId);

                if (bolsa == null)
                {
                    bolsa = new Bolsa
                    {
                        UtilizadorId = utilizadorId,
                        Saldo = saldo,
                        DataAtualizacao = DateTime.Now
                    };

                    _context.Bolsas.Add(bolsa);
                }
                else
                {
                    bolsa.Saldo += saldo;
                    bolsa.DataAtualizacao = DateTime.Now;
                }
            }

            _context.Receitas.RemoveRange(receitas);

            _context.Despesas.RemoveRange(despesas);

            _context.SessoesFinanceiras.Remove(sessao);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = GuardarNaBolsa
                ? "Sessão apagada. O saldo foi guardado na bolsa."
                : "Sessão apagada com sucesso.";

            return RedirectToPage("/SessaoFinanceira/Index");
        }
    }
}