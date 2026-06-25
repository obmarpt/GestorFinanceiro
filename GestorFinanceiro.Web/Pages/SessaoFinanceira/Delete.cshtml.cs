using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
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

        public string? NomeSessao { get; set; }
        public decimal Saldo { get; set; }
        public string? MensagemErro { get; set; }

        [BindProperty]
        public bool GuardarNaBolsa { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessaoResponse = await client.GetAsync($"api/SessaoFinanceira/{Id}");
                if (!sessaoResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Sessão não encontrada.";
                    return Page();
                }

                var sessao = await sessaoResponse.Content.ReadFromJsonAsync<Data.Models.SessaoFinanceira>(FinanceApiHelper.JsonOptions);
                if (sessao == null)
                {
                    MensagemErro = "Sessão não encontrada.";
                    return Page();
                }

                NomeSessao = sessao.Nome;

                var (receitas, despesas, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                if (erro != null)
                {
                    MensagemErro = erro;
                    return Page();
                }

                var totalReceitas = receitas.Where(r => r.SessaoFinanceiraId == Id).Sum(r => r.Valor);
                var totalDespesas = despesas.Where(d => d.SessaoFinanceiraId == Id).Sum(d => d.Valor);
                Saldo = totalReceitas - totalDespesas;
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var payload = new
                {
                    SessaoId = Id,
                    UtilizadorId = utilizadorId,
                    GuardarNaBolsa
                };

                var response = await client.PostAsJsonAsync("api/Bolsa/apagar-sessao", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = erro.Trim('"');
                    return RedirectToPage("/SessaoFinanceira/Index");
                }

                TempData["Sucesso"] = GuardarNaBolsa
                    ? "Sessão apagada. O saldo foi guardado na bolsa."
                    : "Sessão apagada com sucesso.";

                return RedirectToPage("/SessaoFinanceira/Index");
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligação à API: {ex.Message}";
                return RedirectToPage("/SessaoFinanceira/Index");
            }
        }
    }
}
