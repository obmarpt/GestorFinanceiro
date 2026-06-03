using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Despesa
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public int SessaoId { get; set; }
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var response = await client.GetAsync($"api/Despesa/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Despesa não encontrada.";
                    return Page();
                }

                var despesa = await response.Content.ReadFromJsonAsync<Data.Models.Despesa>(JsonOptions);
                if (despesa == null || despesa.SessaoFinanceiraId != sessaoId)
                    return RedirectToPage("Index", new { sessaoId });

                Descricao = despesa.Descricao;
                Valor = despesa.Valor;
                Data = despesa.Data;
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            Id = id;

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            HttpResponseMessage response;
            try
            {
                response = await client.DeleteAsync($"api/Despesa/{id}");
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Não foi possível eliminar a despesa.";
                return Page();
            }

            TempData["Sucesso"] = "Despesa eliminada com sucesso.";
            return RedirectToPage("Index", new { sessaoId });
        }
    }
}
