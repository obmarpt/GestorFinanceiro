using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string? Descricao { get; set; }

        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var sessao = await CarregarSessaoAsync(id);
            if (sessao == null)
                return RedirectToPage("Index");

            Id = sessao.Id;
            Nome = sessao.Nome;
            Descricao = sessao.Descricao;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Id = id;

            if (!ModelState.IsValid)
                return Page();

            var sessaoExistente = await CarregarSessaoAsync(id);
            if (sessaoExistente == null)
                return RedirectToPage("Index");

            var sessao = new Data.Models.SessaoFinanceira
            {
                Id = id,
                Nome = Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
                DataCriacao = sessaoExistente.DataCriacao,
                UtilizadorId = sessaoExistente.UtilizadorId
            };

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            HttpResponseMessage response;
            try
            {
                response = await client.PutAsJsonAsync($"api/SessaoFinanceira/{id}", sessao);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Não foi possível atualizar a sessão.";
                return Page();
            }

            TempData["Sucesso"] = "Sessão atualizada com sucesso.";
            return RedirectToPage("Index");
        }

        private async Task<Data.Models.SessaoFinanceira?> CarregarSessaoAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            try
            {
                var listResponse = await client.GetAsync("api/SessaoFinanceira");
                if (!listResponse.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar a sessão.";
                    return null;
                }

                var todas = await listResponse.Content.ReadFromJsonAsync<List<Data.Models.SessaoFinanceira>>(FinanceApiHelper.JsonOptions) ?? [];
                return FinanceApiHelper.FiltrarSessoesDoUtilizador(todas, User).FirstOrDefault(s => s.Id == id);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return null;
            }
        }
    }
}
