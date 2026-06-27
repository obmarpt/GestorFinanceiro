using GestorFinanceiro.Web.Helpers;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Transferencia
{
    [Authorize]
    public class TransferirModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TransferirModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "Selecione a sessão de origem.")]
        public int SessaoOrigemId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Selecione a sessão de destino.")]
        public int SessaoDestinoId { get; set; }

        [BindProperty]
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [BindProperty]
        public string? Descricao { get; set; }

        public IList<SessaoResumoViewModel> Resumos { get; set; } = [];
        public SelectList SessoesOrigem { get; set; } = null!;
        public SelectList SessoesDestino { get; set; } = null!;
        public decimal? SaldoOrigem { get; set; }
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int? origemId, int? destinoId, decimal? valor)
        {
            if (!await CarregarAsync())
                return Page();

            if (origemId.HasValue) SessaoOrigemId = origemId.Value;
            if (destinoId.HasValue) SessaoDestinoId = destinoId.Value;
            if (valor.HasValue && valor > 0) Valor = valor.Value;

            SaldoOrigem = Resumos.FirstOrDefault(r => r.SessaoId == SessaoOrigemId)?.Saldo;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await CarregarAsync())
                return Page();

            if (SessaoOrigemId == SessaoDestinoId)
                ModelState.AddModelError(nameof(SessaoDestinoId), "Origem e destino devem ser diferentes.");

            SaldoOrigem = Resumos.FirstOrDefault(r => r.SessaoId == SessaoOrigemId)?.Saldo ?? 0;
            if (Valor > SaldoOrigem)
                ModelState.AddModelError(nameof(Valor), $"Valor superior ao saldo ({SaldoOrigem:N2} €).");

            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            var payload = new { SessaoOrigemId, SessaoDestinoId, Valor, Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim() };

            try
            {
                var response = await client.PostAsJsonAsync("api/TransferenciaSaldo", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    MensagemErro = string.IsNullOrWhiteSpace(erro) ? "Não foi possível transferir." : erro.Trim('"');
                    return Page();
                }

                TempData["Sucesso"] = "Saldo transferido com sucesso.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }
        }

        private async Task<bool> CarregarAsync()
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var sessoesResponse = await client.GetAsync("api/SessaoFinanceiras");
                if (!sessoesResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar as sessões.";
                    return false;
                }

                var todas = await sessoesResponse.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                var sessoes = FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User).OrderByDescending(s => s.DataCriacao).ToList();

                if (sessoes.Count < 2)
                {
                    MensagemErro = "Precisa de pelo menos duas sessões para transferir saldo.";
                    return false;
                }

                var (receitas, despesas, erro) = await FinanceApiHelper.ObterReceitasEDespesasAsync(client);
                if (erro != null)
                {
                    MensagemErro = erro;
                    return false;
                }

                var ids = sessoes.Select(s => s.Id).ToHashSet();
                Resumos = FinanceApiHelper.ConstruirResumosPorSessao(sessoes,
                    receitas.Where(r => ids.Contains(r.SessaoFinanceiraId)),
                    despesas.Where(d => ids.Contains(d.SessaoFinanceiraId))).ToList();

                SessoesOrigem = new SelectList(Resumos, nameof(SessaoResumoViewModel.SessaoId), nameof(SessaoResumoViewModel.Nome), SessaoOrigemId);
                SessoesDestino = new SelectList(Resumos, nameof(SessaoResumoViewModel.SessaoId), nameof(SessaoResumoViewModel.Nome), SessaoDestinoId);
                return true;
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return false;
            }
        }
    }
}
