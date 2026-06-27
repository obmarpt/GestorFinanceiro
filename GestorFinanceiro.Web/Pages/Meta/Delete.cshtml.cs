using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace GestorFinanceiro.Web.Pages.Meta
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string? NomeMeta { get; set; }
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
                NomeMeta = meta?.Nome;
                ValorAtual = meta?.ValorAtual ?? 0;
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
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.DeleteAsync($"api/Meta/{Id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Erro"] = "Não foi possível apagar a meta.";
                }
                else
                {
                    TempData["Sucesso"] = "Meta apagada com sucesso.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligação à API: {ex.Message}";
            }

            return RedirectToPage("/Dashboard");
        }
    }
}
