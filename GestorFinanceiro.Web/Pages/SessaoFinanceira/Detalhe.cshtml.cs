using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class DetalheModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetalheModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
        public IList<Data.Models.Receita> Receitas { get; set; } = [];
        public IList<Data.Models.Despesa> Despesas { get; set; } = [];
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessaoResponse = await client.GetAsync($"api/SessaoFinanceiras/{id}");
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

                Nome = sessao.Nome;
                Descricao = sessao.Descricao;
                DataCriacao = sessao.DataCriacao;

                var (receitas, despesas, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                if (erro != null)
                {
                    MensagemErro = erro;
                    return Page();
                }

                Receitas = receitas.Where(r => r.SessaoFinanceiraId == id).OrderByDescending(r => r.Data).Take(5).ToList();
                Despesas = despesas.Where(d => d.SessaoFinanceiraId == id).OrderByDescending(d => d.Data).Take(5).ToList();
                TotalReceitas = receitas.Where(r => r.SessaoFinanceiraId == id).Sum(r => r.Valor);
                TotalDespesas = despesas.Where(d => d.SessaoFinanceiraId == id).Sum(d => d.Valor);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }

            return Page();
        }
    }
}
