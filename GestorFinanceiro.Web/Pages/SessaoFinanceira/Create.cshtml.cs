using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.SessaoFinanceira
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CreateModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string? Descricao { get; set; }

        public string? MensagemErro { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var utilizadorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(utilizadorIdClaim, out var utilizadorId))
            {
                MensagemErro = "Sessão inválida. Faça login novamente.";
                return Page();
            }

            var sessao = new Data.Models.SessaoFinanceira
            {
                Nome = Nome.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
                DataCriacao = DateTime.Now,
                UtilizadorId = utilizadorId
            };

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("api/SessaoFinanceiras", sessao);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}. Confirme que a API está a correr em {_configuration["ApiBaseUrl"]}.";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                MensagemErro = "Não foi possível criar a sessão. Verifique se a API está a correr.";
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}
