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
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SettingsModel(ApplicationDbContext context)
        {
            _context = context;
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
            var utilizador = await ObterUtilizadorAsync();

            if (utilizador == null)
                return Page();

            Nome = utilizador.Nome;
            Username = utilizador.Username;
            Email = utilizador.Email;
            ImagemPerfil = utilizador.ImagemPerfil;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (!string.IsNullOrWhiteSpace(NovaPassword)
                && NovaPassword != ConfirmarPassword)
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

            var utilizador = await ObterUtilizadorAsync();

            if (utilizador == null)
                return Page();

            utilizador.Nome = Nome.Trim();
            utilizador.Username = Username.Trim();
            utilizador.Email = Email.Trim();

            if (!string.IsNullOrWhiteSpace(NovaPassword))
            {
                utilizador.PasswordHash = NovaPassword;
            }

            try
            {
                _context.Utilizadores.Update(utilizador);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao guardar as alterações: {ex.Message}";
                return Page();
            }

            await AtualizarClaimsAsync(
                utilizador.Username,
                utilizador.Email);

            TempData["Sucesso"] = "Alterações guardadas com sucesso.";

            return RedirectToPage();
        }

        private int? ObterUserId()
        {
            var claim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(claim, out var id)
                ? id
                : null;
        }

        private async Task<Utilizador?> ObterUtilizadorAsync()
        {
            var userId = ObterUserId();

            if (userId == null)
            {
                MensagemErro = "Sessão inválida.";
                return null;
            }

            try
            {
                return await _context.Utilizadores
                    .FirstOrDefaultAsync(
                        u => u.Id == userId.Value);
            }
            catch (Exception ex)
            {
                MensagemErro = $"Erro ao carregar os dados: {ex.Message}";
                return null;
            }
        }

        private async Task AtualizarClaimsAsync(
            string username,
            string email)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var role =
                User.FindFirstValue(
                    ClaimTypes.Role)
                ?? "Utilizador";

            var imagemPerfil =
                User.FindFirstValue("ImagemPerfil")
                ?? string.Empty;

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