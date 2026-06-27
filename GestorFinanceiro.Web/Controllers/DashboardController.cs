using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardIndexViewModel(_httpClientFactory);
        await model.CarregarDadosAsync(User);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Depositar(int metaId, int sessaoOrigemId, decimal valor)
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Levantar(int metaId, int sessaoDestinoId, decimal valor)
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TransferirBolsaSessao(int sessaoDestinoId, decimal valor)
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TransferirBolsaMeta(int metaDestinoId, decimal valor)
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ApagarHistorico(int registoId, string tipoRegisto)
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
                return RedirectToAction(nameof(Index));
            }

            var response = await client.DeleteAsync(url);
            TempData[response.IsSuccessStatusCode ? "Sucesso" : "Erro"] = response.IsSuccessStatusCode
                ? "Registo apagado com sucesso."
                : "Não foi possível apagar o registo.";
        }
        catch (HttpRequestException ex) { TempData["Erro"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
