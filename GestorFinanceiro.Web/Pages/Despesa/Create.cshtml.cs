using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.Despesa
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
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O valor deve ser superior a zero.")]
        public decimal Valor { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        public string? MensagemErro { get; set; }

        public IActionResult OnGet(int sessaoId)
        {
            SessaoId = sessaoId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId)
        {
            SessaoId = sessaoId;

            if (!ModelState.IsValid)
                return Page();

            var despesa = new Data.Models.Despesa
            {
                Descricao = Descricao.Trim(),
                Valor = Valor,
                Data = Data,
                SessaoFinanceiraId = sessaoId
            };

            try
            {
                _context.Despesas.Add(despesa);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível criar a despesa: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Despesa criada com sucesso.";

            return RedirectToPage(
                "Index",
                new { sessaoId });
        }
    }
}