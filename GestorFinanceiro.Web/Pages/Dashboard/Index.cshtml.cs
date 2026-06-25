using GestorFinanceiro.Web.Helpers;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IList<HistoricoItemViewModel> Historico { get; set; } = [];
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
            await CarregarDadosAsync();
        }

        public async Task<IActionResult> OnPostDepositarAsync(int metaId, decimal valor)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var payload = new { Valor = valor };

            try
            {
                var response = await client.PostAsJsonAsync($"api/Meta/{metaId}/depositar", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = erro.Trim('"');
                }
                else
                {
                    TempData["Sucesso"] = "Deposito registado com sucesso.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligacao a API: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLevantarAsync(int metaId, decimal valor)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var payload = new { Valor = valor };

            try
            {
                var response = await client.PostAsJsonAsync($"api/Meta/{metaId}/levantar", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = erro.Trim('"');
                }
                else
                {
                    TempData["Sucesso"] = "Levantamento registado com sucesso.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Erro"] = $"Erro de ligacao a API: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task CarregarDadosAsync()
        {
            Username = User.Identity?.Name ?? "Utilizador";
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessoesResponse = await client.GetAsync("api/SessaoFinanceira");
                if (!sessoesResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Nao foi possivel carregar o dashboard.";
                    return;
                }

                var todas = await sessoesResponse.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                var sessoes = FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToList();

                TotalSessoes = sessoes.Count;

                List<Data.Models.Receita> receitas = [];
                List<Data.Models.Despesa> despesas = [];

                if (sessoes.Count > 0)
                {
                    var (r, d, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                    if (erro != null) { MensagemErro = erro; return; }

                    var ids = sessoes.Select(s => s.Id).ToHashSet();
                    receitas = r.Where(x => ids.Contains(x.SessaoFinanceiraId)).ToList();
                    despesas = d.Where(x => ids.Contains(x.SessaoFinanceiraId)).ToList();

                    ResumoPorSessao = FinanceApiHelper.ConstruirResumosPorSessao(sessoes, receitas, despesas)
                        .OrderByDescending(x => x.DataCriacao)
                        .ToList();

                    (TotalReceitas, TotalDespesas) = FinanceApiHelper.CalcularTotaisAgregados(ResumoPorSessao);

                    ChartDataJson = JsonSerializer.Serialize(new
                    {
                        sessoes = ResumoPorSessao.Select(x => new
                        {
                            id = x.SessaoId,
                            nome = x.Nome,
                            receitas = x.TotalReceitas,
                            despesas = x.TotalDespesas
                        }).ToList(),
                        totalReceitas = TotalReceitas,
                        totalDespesas = TotalDespesas
                    });

                    TemDadosGraficos = TotalReceitas > 0 || TotalDespesas > 0;
                }

                // Metas
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

                // Historico: transferencias + movimentos de metas
                var metaIds = Metas.Select(m => m.Id).ToHashSet();

                var transferencias = despesas
                    .Where(d => d.Descricao != null && d.Descricao.Contains("→"))
                    .Select(d => new HistoricoItemViewModel
                    {
                        Data = d.Data,
                        Tipo = "Transferencia",
                        Icone = "⇄",
                        Valor = d.Valor,
                        Descricao = d.Descricao ?? ""
                    });

                var movimentosResponse = await client.GetAsync("api/Meta/movimentos");
                IEnumerable<HistoricoItemViewModel> metaMovimentos = [];

                if (movimentosResponse.IsSuccessStatusCode)
                {
                    var movimentos = await movimentosResponse.Content.ReadFromJsonAsync<List<MetaMovimentoDto>>(FinanceApiHelper.JsonOptions) ?? [];
                    metaMovimentos = movimentos
                        .Where(m => metaIds.Contains(m.MetaId))
                        .Select(m => new HistoricoItemViewModel
                        {
                            Data = m.Data,
                            Tipo = m.Tipo == "Deposito" ? "Deposito Meta" : "Levantamento Meta",
                            Icone = m.Tipo == "Deposito" ? "💰" : "💸",
                            Valor = m.Valor,
                            Descricao = $"Meta: {Metas.FirstOrDefault(meta => meta.Id == m.MetaId)?.Nome ?? ""}"
                        });
                }

                Historico = transferencias
                    .Concat(metaMovimentos)
                    .OrderByDescending(h => h.Data)
                    .Take(10)
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligacao a API: {ex.Message}";
            }
        }

        // DTO local para deserializar os movimentos da API
        private class MetaMovimentoDto
        {
            public int Id { get; set; }
            public int MetaId { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public DateTime Data { get; set; }
        }
    }
}
