using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GestorFinanceiro.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
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

            var utilizador = await ObterUtilizadorAsync(userId.Value);

            if (utilizador == null)
                return Page();

            utilizador.Nome = Nome.Trim();
            utilizador.Username = Username.Trim();
            utilizador.Email = Email.Trim();

            if (!await GuardarUtilizadorAsync(utilizador))
                return Page();

            await AtualizarClaimsAsync(
                utilizador.Username,
                utilizador.Email,
                utilizador.ImagemPerfil ?? string.Empty);

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
                MensagemErro = "Formato não suportado. Utilize JPG, PNG ou WEBP.";
                return Page();
            }

            var pasta = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "avatars");

            Directory.CreateDirectory(pasta);

            var nomeFicheiro = $"user_{userId}{ext}";
            var caminho = Path.Combine(pasta, nomeFicheiro);

            await using (var stream = new FileStream(caminho, FileMode.Create))
            {
                await ImagemUpload.CopyToAsync(stream);
            }

            var url = $"/uploads/avatars/{nomeFicheiro}";

            var utilizador = await ObterUtilizadorAsync(userId.Value);

            if (utilizador == null)
                return Page();

            utilizador.ImagemPerfil = url;

            if (!await GuardarUtilizadorAsync(utilizador))
                return Page();

            await AtualizarClaimsAsync(
                User.Identity!.Name!,
                User.FindFirstValue(ClaimTypes.Email)!,
                url);

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

            var utilizador = await ObterUtilizadorAsync(userId.Value);

            if (utilizador == null)
                return Page();

            Nome = utilizador.Nome;
            Username = utilizador.Username;
            Email = utilizador.Email;
            Role = utilizador.Role;
            ImagemPerfil = utilizador.ImagemPerfil;

            return Page();
        }

        private int? ObterUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(claim, out var id)
                ? id
                : null;
        }

        private async Task<Utilizador?> ObterUtilizadorAsync(int userId)
        {
            try
            {
                return await _context.Utilizadores
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao carregar o perfil: {ex.Message}";
                return null;
            }
        }

        private async Task<bool> GuardarUtilizadorAsync(Utilizador utilizador)
        {
            try
            {
                _context.Utilizadores.Update(utilizador);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao guardar o perfil: {ex.Message}";
                return false;
            }
        }

        private async Task AtualizarClaimsAsync(
            string username,
            string email,
            string imagemPerfil = "")
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var role =
                User.FindFirstValue(ClaimTypes.Role)
                ?? "Utilizador";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role),
                new("ImagemPerfil", imagemPerfil)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}