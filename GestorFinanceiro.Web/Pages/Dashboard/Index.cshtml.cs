using GestorFinanceiro.Web.Helpers;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public string Username { get; set; } = string.Empty;
        public int TotalSessoes { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
        public IList<SessaoResumoViewModel> ResumoPorSessao { get; set; } = [];
        public IList<MetaViewModel> Metas { get; set; } = [];
        public string? MensagemErro { get; set; }
        public string ChartDataJson { get; set; } = "{}";
        public bool TemDadosGraficos { get; set; }

        public SessaoResumoViewModel? SessaoComMaiorSaldo =>
            ResumoPorSessao.Where(r => r.Saldo > 0).OrderByDescending(r => r.Saldo).FirstOrDefault();

        public int? EncontrarSessaoId(string termo) =>
            ResumoPorSessao.FirstOrDefault(r =>
                r.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))?.SessaoId;

        public async Task OnGetAsync()
        {
            Username = User.Identity?.Name ?? "Utilizador";
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessoesResponse = await client.GetAsync("api/SessaoFinanceira");
                if (!sessoesResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar o dashboard. Verifique se a API está a correr.";
                    return;
                }

                var todas = await sessoesResponse.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                var sessoes = FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToList();

                TotalSessoes = sessoes.Count;

                if (sessoes.Count > 0)
                {
                    var (receitas, despesas, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                    if (erro != null)
                    {
                        MensagemErro = erro;
                        return;
                    }

                    var ids = sessoes.Select(s => s.Id).ToHashSet();
                    receitas = receitas.Where(r => ids.Contains(r.SessaoFinanceiraId)).ToList();
                    despesas = despesas.Where(d => ids.Contains(d.SessaoFinanceiraId)).ToList();

                    ResumoPorSessao = FinanceApiHelper.ConstruirResumosPorSessao(sessoes, receitas, despesas)
                        .OrderByDescending(r => r.DataCriacao)
                        .ToList();

                    (TotalReceitas, TotalDespesas) = FinanceApiHelper.CalcularTotaisAgregados(ResumoPorSessao);

                    ChartDataJson = JsonSerializer.Serialize(new
                    {
                        sessoes = ResumoPorSessao.Select(r => new
                        {
                            id = r.SessaoId,
                            nome = r.Nome,
                            receitas = r.TotalReceitas,
                            despesas = r.TotalDespesas
                        }).ToList(),
                        totalReceitas = TotalReceitas,
                        totalDespesas = TotalDespesas
                    });

                    TemDadosGraficos = TotalReceitas > 0 || TotalDespesas > 0;
                }

                // Carregar metas do utilizador
                var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var metasResponse = await client.GetAsync("api/Meta");
                if (metasResponse.IsSuccessStatusCode)
                {
                    var todasMetas = await metasResponse.Content.ReadFromJsonAsync<List<Data.Models.Meta>>(FinanceApiHelper.JsonOptions) ?? [];
                    Metas = todasMetas
                        .Where(m => m.UtilizadorId == utilizadorId)
                        .OrderBy(m => m.DataCriacao)
                        .Select(m => new MetaViewModel
                        {
                            Id = m.Id,
                            Nome = m.Nome,
                            Descricao = m.Descricao,
                            ValorAlvo = m.ValorAlvo,
                            ValorAtual = m.ValorAtual,
                            UtilizadorId = m.UtilizadorId,
                            DataCriacao = m.DataCriacao
                        })
                        .ToList();
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }
        }
    }
}
