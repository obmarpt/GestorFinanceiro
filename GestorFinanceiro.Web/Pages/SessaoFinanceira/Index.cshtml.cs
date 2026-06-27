using GestorFinanceiro.Web.Helpers;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<SessaoResumoViewModel> ResumoPorSessao { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var response = await client.GetAsync("api/SessaoFinanceiras");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar as sessões. Verifique se a API está a correr.";
                    return Page();
                }

                var todas = await response.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                var sessoes = FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToList();

                if (sessoes.Count == 0)
                    return Page();

                var (receitas, despesas, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                if (erro != null)
                {
                    MensagemErro = erro;
                    return Page();
                }

                var ids = sessoes.Select(s => s.Id).ToHashSet();
                receitas = receitas.Where(r => ids.Contains(r.SessaoFinanceiraId)).ToList();
                despesas = despesas.Where(d => ids.Contains(d.SessaoFinanceiraId)).ToList();

                ResumoPorSessao = FinanceApiHelper.ConstruirResumosPorSessao(sessoes, receitas, despesas)
                    .OrderByDescending(r => r.DataCriacao)
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
