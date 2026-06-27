using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Receita
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public int SessaoId { get; set; }
        public string SessaoNome { get; set; } = string.Empty;
        public IList<Data.Models.Receita> Receitas { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessaoResponse = await client.GetAsync($"api/SessaoFinanceiras/{sessaoId}");
                if (!sessaoResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Sessão financeira não encontrada.";
                    return Page();
                }

                var sessao = await sessaoResponse.Content.ReadFromJsonAsync<Data.Models.SessaoFinanceira>(JsonOptions);
                SessaoNome = sessao?.Nome ?? "Sessão";

                var response = await client.GetAsync("api/Receita");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar as receitas. Verifique se a API está a correr.";
                    return Page();
                }

                var todas = await response.Content.ReadFromJsonAsync<List<Data.Models.Receita>>(JsonOptions) ?? [];
                Receitas = todas
                    .Where(r => r.SessaoFinanceiraId == sessaoId)
                    .OrderByDescending(r => r.Data)
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }

            return Page();
        }
    }
}
