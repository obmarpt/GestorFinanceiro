using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GestorFinanceiro.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UtilizadoresModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<UtilizadorResumo> Utilizadores { get; set; } = [];
        public string? MensagemErro { get; set; }
        public async Task<IActionResult> OnPostTornarAdminAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.PatchAsync(
                    $"api/Utilizador/{id}/role",
                    JsonContent.Create(new { Role = "Admin" }));

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Erro"] = "Não foi possível alterar o role.";
                }
                else
                {
                    TempData["Sucesso"] = "Utilizador promovido a Admin com sucesso.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligação à API: {ex.Message}";
            }
            return RedirectToPage();
        }
        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.GetAsync("api/Utilizador");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar os utilizadores.";
                    return;
                }
                Utilizadores = await response.Content
                    .ReadFromJsonAsync<List<UtilizadorResumo>>(FinanceApiHelper.JsonOptions) ?? [];
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostApagarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.DeleteAsync($"api/Utilizador/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = erro.Trim('"');
                }
                else
                {
                    TempData["Sucesso"] = "Utilizador apagado com sucesso.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligação à API: {ex.Message}";
            }
            return RedirectToPage();
        }

        public class UtilizadorResumo
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public int TotalSessoes { get; set; }
        }
    }
}
