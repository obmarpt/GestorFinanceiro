using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Meta
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string? Descricao { get; set; }

        [BindProperty]
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor alvo deve ser maior que zero.")]
        public decimal ValorAlvo { get; set; }

        [BindProperty]
        [Range(0, double.MaxValue)]
        public decimal ValorAtual { get; set; } = 0;

        public string? MensagemErro { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var payload = new { Nome, Descricao, ValorAlvo, ValorAtual, UtilizadorId = utilizadorId };

            try
            {
                var response = await client.PostAsJsonAsync("api/Meta", payload);
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível criar a meta.";
                    return Page();
                }

                TempData["Sucesso"] = "Meta criada com sucesso.";
                return RedirectToPage("/Dashboard");
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }
        }
    }
}
