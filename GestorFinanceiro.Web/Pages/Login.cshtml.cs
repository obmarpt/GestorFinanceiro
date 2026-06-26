using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GestorFinanceiro.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string LoginInput { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string MensagemErro { get; set; } = string.Empty;
        public string MensagemSucesso { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Dashboard/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginInput) || string.IsNullOrWhiteSpace(Password))
            {
                MensagemErro = "Preencha todos os campos.";
                return Page();
            }

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Username == LoginInput || u.Email == LoginInput);

            if (utilizador == null)
            {
                MensagemErro = "Utilizador não encontrado.";
                return Page();
            }

            if (utilizador.PasswordHash != Password)
            {
                MensagemErro = "Password incorreta.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Name, utilizador.Username),
                new Claim(ClaimTypes.Email, utilizador.Email),
                new Claim(ClaimTypes.Role, utilizador.Role),
                new Claim("ImagemPerfil", utilizador.ImagemPerfil ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Redirecionar Admin para painel de administração
            if (utilizador.Role == "Admin")
                return RedirectToPage("/Admin/Utilizadores");

            return RedirectToPage("/Dashboard/Index");
        }
    }
}
