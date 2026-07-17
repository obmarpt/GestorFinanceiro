using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.Receita
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

            var receita = new Data.Models.Receita
            {
                Descricao = Descricao.Trim(),
                Valor = Valor,
                Data = Data,
                SessaoFinanceiraId = sessaoId
            };

            try
            {
                _context.Receitas.Add(receita);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro =
                    $"Não foi possível criar a receita: {ex.Message}";

                return Page();
            }

            TempData["Sucesso"] =
                "Receita criada com sucesso.";

            return RedirectToPage(
                "Index",
                new { sessaoId });
        }
    }
}