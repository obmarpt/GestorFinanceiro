using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Meta
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

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
        public decimal ValorAtual { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.GetAsync($"api/Meta/{Id}");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Meta não encontrada.";
                    return Page();
                }

                var meta = await response.Content.ReadFromJsonAsync<GestorFinanceiro.Data.Models.Meta>();
                if (meta == null)
                {
                    MensagemErro = "Meta não encontrada.";
                    return Page();
                }

                Nome = meta.Nome;
                Descricao = meta.Descricao;
                ValorAlvo = meta.ValorAlvo;
                ValorAtual = meta.ValorAtual;
                return Page();
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var payload = new { Nome, Descricao, ValorAlvo, ValorAtual, UtilizadorId = utilizadorId };

            try
            {
                var response = await client.PutAsJsonAsync($"api/Meta/{Id}", payload);
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível guardar as alterações.";
                    return Page();
                }

                TempData["Sucesso"] = "Meta atualizada com sucesso.";
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
