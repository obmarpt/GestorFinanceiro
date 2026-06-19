using GestorFinanceiro.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ProfileModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
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

        public string Role { get; set; } = string.Empty;
        public string? ImagemPerfil { get; set; }
        public string? MensagemErro { get; set; }
        public bool EditMode { get; set; }

        [BindProperty]
        public IFormFile? ImagemUpload { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return await CarregarPerfilAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            EditMode = true;
            if (!ModelState.IsValid)
                return Page();

            var userId = ObterUserId();
            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return Page();
            }

            var atual = await ObterUtilizadorAsync(userId.Value);
            if (atual == null)
                return Page();

            var utilizador = new Data.Models.Utilizador
            {
                Id = userId.Value,
                Nome = Nome.Trim(),
                Username = Username.Trim(),
                Email = Email.Trim(),
                PasswordHash = atual.PasswordHash,
                Role = atual.Role,
                ImagemPerfil = atual.ImagemPerfil
            };

            if (!await GuardarUtilizadorAsync(utilizador))
                return Page();

            await AtualizarClaimsAsync(utilizador.Username, utilizador.Email);
            TempData["Sucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostImagemAsync()
        {
            var userId = ObterUserId();
            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return Page();
            }

            if (ImagemUpload == null || ImagemUpload.Length == 0)
            {
                MensagemErro = "Selecione uma imagem.";
                return Page();
            }

            var ext = Path.GetExtension(ImagemUpload.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            {
                MensagemErro = "Formato não suportado. Use JPG, PNG ou WEBP.";
                return Page();
            }

            var pasta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(pasta);

            var nomeFicheiro = $"user_{userId}{ext}";
            var caminho = Path.Combine(pasta, nomeFicheiro);

            await using (var stream = new FileStream(caminho, FileMode.Create))
            {
                await ImagemUpload.CopyToAsync(stream);
            }

            var url = $"/uploads/avatars/{nomeFicheiro}";
            var atual = await ObterUtilizadorAsync(userId.Value);
            if (atual == null)
                return Page();

            atual.ImagemPerfil = url;
            if (!await GuardarUtilizadorAsync(atual))
                return Page();

            TempData["Sucesso"] = "Imagem de perfil atualizada.";
            return RedirectToPage();
        }

        private async Task<IActionResult> CarregarPerfilAsync()
        {
            var userId = ObterUserId();
            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return Page();
            }

            var u = await ObterUtilizadorAsync(userId.Value);
            if (u == null)
                return Page();

            Nome = u.Nome;
            Username = u.Username;
            Email = u.Email;
            Role = u.Role;
            ImagemPerfil = u.ImagemPerfil;
            return Page();
        }

        private int? ObterUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<Data.Models.Utilizador?> ObterUtilizadorAsync(int userId)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.GetAsync($"api/Utilizador/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    MensagemErro = "Não foi possível carregar o perfil.";
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

        private async Task<bool> GuardarUtilizadorAsync(Data.Models.Utilizador utilizador)
        {
            var client = _httpClientFactory.CreateClient("GestorFinanceiroApi");
            try
            {
                var response = await client.PutAsJsonAsync($"api/Utilizador/{utilizador.Id}", utilizador);
                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    MensagemErro = string.IsNullOrWhiteSpace(erro) ? "Não foi possível guardar." : erro.Trim('"');
                    return false;
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                MensagemErro = $"Erro de ligação à API: {ex.Message}";
                return false;
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
