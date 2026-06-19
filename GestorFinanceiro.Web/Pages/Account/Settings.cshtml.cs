using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Account
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SettingsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "O username é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? NovaPassword { get; set; }

        [BindProperty]
        public string? ConfirmarPassword { get; set; }

        public string? ImagemPerfil { get; set; }
        public string? MensagemErro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var u = await ObterUtilizadorAsync();
            if (u == null) return Page();
            Nome = u.Nome;
            Username = u.Username;
            Email = u.Email;
            ImagemPerfil = u.ImagemPerfil;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (!string.IsNullOrWhiteSpace(NovaPassword) && NovaPassword != ConfirmarPassword)
            {
                MensagemErro = "As passwords não coincidem.";
                return Page();
            }

            var userId = ObterUserId();
            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return Page();
            }

            var atual = await ObterUtilizadorAsync();
            if (atual == null)
                return Page();

            var utilizador = new Data.Models.Utilizador
            {
                Id = userId.Value,
                Nome = Nome.Trim(),
                Username = Username.Trim(),
                Email = Email.Trim(),
                PasswordHash = string.IsNullOrWhiteSpace(NovaPassword) ? atual.PasswordHash : NovaPassword,
                Role = atual.Role,
                ImagemPerfil = atual.ImagemPerfil
            };

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.PutAsJsonAsync($"api/Utilizador/{userId}", utilizador);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    MensagemErro = string.IsNullOrWhiteSpace(erro) ? "Não foi possível guardar." : erro.Trim('"');
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return Page();
            }

            await AtualizarClaimsAsync(utilizador.Username, utilizador.Email);
            TempData["Sucesso"] = "Alterações guardadas com sucesso.";
            return RedirectToPage();
        }

        private int? ObterUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<Data.Models.Utilizador?> ObterUtilizadorAsync()
        {
            var userId = ObterUserId();
            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return null;
            }

            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.GetAsync($"api/Utilizador/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar os dados.";
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<Data.Models.Utilizador>(FinanceApiHelper.JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return null;
            }
        }

        private async Task AtualizarClaimsAsync(string username, string email)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Email, email)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
