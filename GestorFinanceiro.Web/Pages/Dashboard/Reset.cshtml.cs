using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Dashboard
{
    [Authorize]
    public class ResetModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "A password é obrigatória.")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Tipo { get; set; } = "tudo";

        public string? MensagemErro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                MensagemErro = "A password é obrigatória.";
                return Page();
            }

            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var payload = new { UtilizadorId = utilizadorId, Password, Tipo };
                var response = await client.PostAsJsonAsync("api/Reset/apagar-movimentos", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    MensagemErro = erro.Trim('"');
                    return Page();
                }

                var mensagem = Tipo switch
                {
                    "receitas" => "Todas as receitas foram apagadas com sucesso.",
                    "despesas" => "Todas as despesas foram apagadas com sucesso.",
                    _ => "Todos os movimentos foram apagados com sucesso."
                };

                TempData["Sucesso"] = mensagem;
                return RedirectToPage("/Dashboard/Index");
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }
        }
    }
}
