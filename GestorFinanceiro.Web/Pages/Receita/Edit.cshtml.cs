using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.Pages.Receita
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
        [Range(0.01, double.MaxValue,
            ErrorMessage = "O valor deve ser superior a zero.")]
        public decimal Valor { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;

            var receita = await _context.Receitas.FindAsync(id);

            if (receita == null)
            {
                MensagemErro = "Receita não encontrada.";
                return Page();
            }

            if (receita.SessaoFinanceiraId != sessaoId)
            {
                return RedirectToPage("Index", new { sessaoId });
            }

            Id = receita.Id;
            Descricao = receita.Descricao;
            Valor = receita.Valor;
            Data = receita.Data;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            var receita = await _context.Receitas.FindAsync(id);

            if (receita == null)
            {
                MensagemErro = "Receita não encontrada.";
                return Page();
            }

            receita.Descricao = Descricao.Trim();
            receita.Valor = Valor;
            receita.Data = Data;
            receita.SessaoFinanceiraId = sessaoId;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Receita atualizada com sucesso.";

            return RedirectToPage("Index", new { sessaoId });
        }
    }
}