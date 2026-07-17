using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string? Descricao { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var sessao = await CarregarSessaoAsync(id);
            if (sessao == null)
                return RedirectToPage("Index");

            Id = sessao.Id;
            Nome = sessao.Nome;
            Descricao = sessao.Descricao;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            var sessaoExistente = await CarregarSessaoAsync(id);
            if (sessaoExistente == null)
                return RedirectToPage("Index");

            sessaoExistente.Nome = Nome.Trim();
            sessaoExistente.Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                MensagemErro = $"Não foi possível atualizar a sessão: {ex.Message}";
                return Page();
            }

            TempData["Sucesso"] = "Sessão atualizada com sucesso.";
            return RedirectToPage("Index");
        }

        private async Task<Data.Models.SessaoFinanceira?> CarregarSessaoAsync(int id)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var sessao = await _context.SessoesFinanceiras
                .FirstOrDefaultAsync(s => s.Id == id && s.UtilizadorId == utilizadorId);

            if (sessao == null)
                MensagemErro = "Sessão não encontrada.";

            return sessao;
        }
    }
}