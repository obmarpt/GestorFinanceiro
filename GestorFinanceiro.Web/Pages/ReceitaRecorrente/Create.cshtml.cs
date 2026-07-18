using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.ReceitaRecorrente
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser superior a zero.")]
        public decimal Valor { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O dia do mês é obrigatório.")]
        [Range(1, 31, ErrorMessage = "O dia deve estar entre 1 e 31.")]
        public int DiaDoMes { get; set; } = 1;

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; } = DateTime.Today;

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? DataFim { get; set; }

        [BindProperty]
        public bool Ativa { get; set; } = true;

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;

            if (!await _context.SessoesFinanceiras.AnyAsync(s => s.Id == sessaoId))
            {
                MensagemErro = "Sessão financeira não encontrada.";
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId)
        {
            SessaoId = sessaoId;

            if (!ModelState.IsValid)
                return Page();

            if (DataFim.HasValue && DataFim.Value.Date < DataInicio.Date)
            {
                ModelState.AddModelError(nameof(DataFim), "A data de fim não pode ser anterior à data de início.");
                return Page();
            }

            _context.ReceitasRecorrentes.Add(new Data.Models.ReceitaRecorrente
            {
                Descricao = Descricao.Trim(),
                Valor = Valor,
                DiaDoMes = DiaDoMes,
                DataInicio = DataInicio,
                DataFim = DataFim,
                Ativa = Ativa,
                SessaoFinanceiraId = sessaoId
            });

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Regra de receita recorrente criada.";
            return RedirectToPage("Index", new { sessaoId });
        }
    }
}
