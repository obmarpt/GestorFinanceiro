using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Receita
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public int SessaoId { get; set; }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser superior a zero.")]
        public decimal Valor { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int sessaoId, int id)
        {
            SessaoId = sessaoId;
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var response = await client.GetAsync($"api/Receita/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Receita não encontrada.";
                    return Page();
                }

                var receita = await response.Content.ReadFromJsonAsync<Data.Models.Receita>(JsonOptions);
                if (receita == null || receita.SessaoFinanceiraId != sessaoId)
                    return RedirectToPage("Index", new { sessaoId });

                Id = receita.Id;
                Descricao = receita.Descricao;
                Valor = receita.Valor;
                Data = receita.Data;
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

            if (!ModelState.IsValid)
                return Page();

            var receita = new Data.Models.Receita
            {
                Id = id,
                Descricao = Descricao.Trim(),
                Valor = Valor,
                Data = Data,
                SessaoFinanceiraId = sessaoId
            };

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            HttpResponseMessage response;
            try
            {
                response = await client.PutAsJsonAsync($"api/Receita/{id}", receita);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Não foi possível atualizar a receita.";
                return Page();
            }

            TempData["Sucesso"] = "Receita atualizada com sucesso.";
            return RedirectToPage("Index", new { sessaoId });
        }
    }
}
