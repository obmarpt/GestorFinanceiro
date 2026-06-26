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
        public BolsaViewModel Bolsa { get; set; } = new();
        public string? MensagemErro { get; set; }
        public string ChartDataJson { get; set; } = "{}";
        public bool TemDadosGraficos { get; set; }

        public SessaoResumoViewModel? SessaoComMaiorSaldo =>
            ResumoPorSessao.Where(r => r.Saldo > 0).OrderByDescending(r => r.Saldo).FirstOrDefault();

        public int? EncontrarSessaoId(string termo) =>
            ResumoPorSessao.FirstOrDefault(r =>
                r.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))?.SessaoId;

        public async Task OnGetAsync() => await CarregarDadosAsync();

        public async Task<IActionResult> OnPostDepositarAsync(int metaId, int sessaoOrigemId, decimal valor)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var payload = new { SessaoOrigemId = sessaoOrigemId, Valor = valor };
                var response = await client.PostAsJsonAsync($"api/Meta/{metaId}/depositar", payload);
                TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                    ? "Depósito realizado com sucesso."
                    : (await response.Content.ReadAsStringAsync()).Trim('"');
            }
            catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLevantarAsync(int metaId, int sessaoDestinoId, decimal valor)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var payload = new { SessaoDestinoId = sessaoDestinoId, Valor = valor };
                var response = await client.PostAsJsonAsync($"api/Meta/{metaId}/levantar", payload);
                TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                    ? "Levantamento realizado com sucesso."
                    : (await response.Content.ReadAsStringAsync()).Trim('"');
            }
            catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferirBolsaSessaoAsync(int sessaoDestinoId, decimal valor)
        {
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var payload = new { UtilizadorId = utilizadorId, SessaoDestinoId = sessaoDestinoId, Valor = valor };
                var response = await client.PostAsJsonAsync("api/Bolsa/transferir-sessao", payload);
                TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                    ? "Transferência da bolsa realizada com sucesso."
                    : (await response.Content.ReadAsStringAsync()).Trim('"');
            }
            catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferirBolsaMetaAsync(int metaDestinoId, decimal valor)
        {
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var payload = new { UtilizadorId = utilizadorId, MetaDestinoId = metaDestinoId, Valor = valor };
                var response = await client.PostAsJsonAsync("api/Bolsa/transferir-meta", payload);
                TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                    ? "Transferência para poupança realizada com sucesso."
                    : (await response.Content.ReadAsStringAsync()).Trim('"');
            }
            catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApagarHistoricoAsync(int registoId, string tipoRegisto)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                string url = tipoRegisto switch
                {
                    "receita" => $"api/Receita/{registoId}",
                    "despesa" => $"api/Despesa/{registoId}",
                    "metamovimento" => $"api/Meta/movimentos/{registoId}",
                    _ => ""
                };

                if (string.IsNullOrEmpty(url))
                {
                    TempData["Erro"] = "Tipo de registo desconhecido.";
                    return RedirectToPage();
                }

                var response = await client.DeleteAsync(url);
                TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                    ? "Registo apagado com sucesso."
                    : "Não foi possível apagar o registo.";
            }
            catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
            return RedirectToPage();
        }

        private async Task CarregarDadosAsync()
        {
            Username = User.Identity?.Name ?? "Utilizador";
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var sessoesResponse = await client.GetAsync("api/SessaoFinanceira");
                if (!sessoesResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar o dashboard.";
                    return;
                }

                var todas = await sessoesResponse.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                var sessoes = FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User)
                    .OrderByDescending(s => s.DataCriacao).ToList();

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
                        .OrderByDescending(x => x.DataCriacao).ToList();

                    (TotalReceitas, TotalDespesas) = FinanceApiHelper.CalcularTotaisAgregados(ResumoPorSessao);

                    ChartDataJson = JsonSerializer.Serialize(new
                    {
                        sessoes = ResumoPorSessao.Select(x => new { id = x.SessaoId, nome = x.Nome, receitas = x.TotalReceitas, despesas = x.TotalDespesas }).ToList(),
                        totalReceitas = TotalReceitas,
                        totalDespesas = TotalDespesas
                    });

                    TemDadosGraficos = TotalReceitas > 0 || TotalDespesas > 0;
                }

                // Metas
                var metasResponse = await client.GetAsync("api/Meta");
                if (metasResponse.IsSuccessStatusCode)
                {
                    var todasMetas = await metasResponse.Content.ReadFromJsonAsync<List<Data.Models.Meta>>(FinanceApiHelper.JsonOptions) ?? [];
                    Metas = todasMetas.Where(m => m.UtilizadorId == utilizadorId).OrderBy(m => m.DataCriacao)
                        .Select(m => new MetaViewModel { Id = m.Id, Nome = m.Nome, Descricao = m.Descricao, ValorAlvo = m.ValorAlvo, ValorAtual = m.ValorAtual, UtilizadorId = m.UtilizadorId, DataCriacao = m.DataCriacao })
                        .ToList();
                }

                // Bolsa
                var bolsaResponse = await client.GetAsync($"api/Bolsa/{utilizadorId}");
                if (bolsaResponse.IsSuccessStatusCode)
                {
                    var bolsaData = await bolsaResponse.Content.ReadFromJsonAsync<BolsaDto>(FinanceApiHelper.JsonOptions);
                    if (bolsaData != null)
                        Bolsa = new BolsaViewModel { UtilizadorId = utilizadorId, Saldo = bolsaData.Saldo, DataAtualizacao = bolsaData.DataAtualizacao };
                }

                // Historico
                var metaIds = Metas.Select(m => m.Id).ToHashSet();
                var sessaoNomes = sessoes.ToDictionary(s => s.Id, s => s.Nome);

                var itensReceitas = receitas
                    .Select(r => new HistoricoItemViewModel
                    {
                        RegistoId = r.Id,
                        TipoRegisto = "receita",
                        Data = r.Data,
                        Tipo = "Receita",
                        Icone = "↑",
                        Valor = r.Valor,
                        Descricao = $"{r.Descricao} · {(sessaoNomes.TryGetValue(r.SessaoFinanceiraId, out var nomeR) ? nomeR : "")}"
                    });

                var itensDespesas = despesas
                    .Where(d => d.Descricao == null || !d.Descricao.Contains("→") && !d.Descricao.Contains("poupança"))
                    .Select(d => new HistoricoItemViewModel
                    {
                        RegistoId = d.Id,
                        TipoRegisto = "despesa",
                        Data = d.Data,
                        Tipo = "Despesa",
                        Icone = "↓",
                        Valor = d.Valor,
                        Descricao = $"{d.Descricao} · {(sessaoNomes.TryGetValue(d.SessaoFinanceiraId, out var nomeD) ? nomeD : "")}"
                    });

                var transferencias = despesas
                    .Where(d => d.Descricao != null && d.Descricao.Contains("→"))
                    .Select(d => new HistoricoItemViewModel
                    {
                        RegistoId = d.Id,
                        TipoRegisto = "despesa",
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
                    metaMovimentos = movimentos.Where(m => metaIds.Contains(m.MetaId))
                        .Select(m => new HistoricoItemViewModel
                        {
                            RegistoId = m.Id,
                            TipoRegisto = "metamovimento",
                            Data = m.Data,
                            Tipo = m.Tipo == "Deposito" ? "Deposito na Conta Poupança" : "Levantamento da Conta Poupança",
                            Icone = m.Tipo == "Deposito" ? "↑" : "↓",
                            Valor = m.Valor,
                            Descricao = $"Conta Poupança: {Metas.FirstOrDefault(meta => meta.Id == m.MetaId)?.Nome ?? ""}"
                        });
                }

                Historico = itensReceitas
                    .Concat(itensDespesas)
                    .Concat(transferencias)
                    .Concat(metaMovimentos)
                    .OrderByDescending(h => h.Data)
                    .ThenByDescending(h => h.RegistoId)
                    .Take(10)
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }
        }

        private class MetaMovimentoDto
        {
            public int Id { get; set; }
            public int MetaId { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public DateTime Data { get; set; }
        }

        private class BolsaDto
        {
            public decimal Saldo { get; set; }
            public DateTime DataAtualizacao { get; set; }
        }
    }
}
