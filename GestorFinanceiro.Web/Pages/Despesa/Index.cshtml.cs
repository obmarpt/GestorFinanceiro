using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Despesa
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
        public IList<Data.Models.Despesa> Despesas { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId)
        {
            SessaoId = sessaoId;
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessaoResponse = await client.GetAsync($"api/SessaoFinanceira/{sessaoId}");
                if (!sessaoResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Sessão financeira não encontrada.";
                    return Page();
                }

                var sessao = await sessaoResponse.Content.ReadFromJsonAsync<Data.Models.SessaoFinanceira>(JsonOptions);
                SessaoNome = sessao?.Nome ?? "Sessão";

                var response = await client.GetAsync("api/Despesa");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar as despesas. Verifique se a API está a correr.";
                    return Page();
                }

                var todas = await response.Content.ReadFromJsonAsync<List<Data.Models.Despesa>>(JsonOptions) ?? [];
                Despesas = todas
                    .Where(d => d.SessaoFinanceiraId == sessaoId)
                    .OrderByDescending(d => d.Data)
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
