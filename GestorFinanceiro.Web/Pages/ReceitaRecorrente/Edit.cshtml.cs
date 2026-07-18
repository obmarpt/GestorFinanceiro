using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.ReceitaRecorrente
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int SessaoId { get; set; }

        [BindProperty]
        public int Id { get; set; }

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
        public int DiaDoMes { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? DataFim { get; set; }

        [BindProperty]
        public bool Ativa { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;

            var regra = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.Id == id && r.SessaoFinanceiraId == sessaoId);

            if (regra == null)
            {
                MensagemErro = "Regra não encontrada.";
                return Page();
            }

            Id = regra.Id;
            Descricao = regra.Descricao;
            Valor = regra.Valor;
            DiaDoMes = regra.DiaDoMes;
            DataInicio = regra.DataInicio;
            DataFim = regra.DataFim;
            Ativa = regra.Ativa;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            if (DataFim.HasValue && DataFim.Value.Date < DataInicio.Date)
            {
                ModelState.AddModelError(nameof(DataFim), "A data de fim não pode ser anterior à data de início.");
                return Page();
            }

            var regra = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.Id == id && r.SessaoFinanceiraId == sessaoId);

            if (regra == null)
            {
                MensagemErro = "Regra não encontrada.";
                return Page();
            }

            regra.Descricao = Descricao.Trim();
            regra.Valor = Valor;
            regra.DiaDoMes = DiaDoMes;
            regra.DataInicio = DataInicio;
            regra.DataFim = DataFim;
            regra.Ativa = Ativa;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Regra atualizada.";
            return RedirectToPage("Index", new { sessaoId });
        }
    }
}
