using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
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

        public IList<Data.Models.SessaoFinanceira> Sessoes { get; set; } = [];

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var response = await client.GetAsync("api/SessaoFinanceira");

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Não foi possível carregar as sessões. Verifique se a API está a correr.";
                return Page();
            }

            var todas = await response.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(JsonOptions) ?? [];

            Sessoes = FiltrarSessoesDoUtilizador(todas)
                .OrderByDescending(s => s.DataCriacao)
                .ToList();

            return Page();
        }

        private List<Data.Models.SessaoFinanceira> FiltrarSessoesDoUtilizador(IEnumerable<Data.Models.SessaoFinanceira> sessoes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);

            return sessoes.Where(s =>
            {
                if (!string.IsNullOrEmpty(userId) && s.UtilizadorId.ToString() == userId)
                    return true;

                if (s.Utilizador == null)
                    return false;

                return (!string.IsNullOrEmpty(username) && s.Utilizador.Username == username)
                    || (!string.IsNullOrEmpty(email) && s.Utilizador.Email == email);
            }).ToList();
        }
    }
}
