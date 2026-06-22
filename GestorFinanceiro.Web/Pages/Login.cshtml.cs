using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

// ✅ Autenticação
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

        // ✅ Username ou Email
        [BindProperty]
        public string LoginInput { get; set; } = string.Empty;

        // ✅ Password
        [BindProperty]
        public string Password { get; set; } = string.Empty;

        // ✅ Mensagens
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
            // ✅ 1. Validar campos
            if (string.IsNullOrWhiteSpace(LoginInput) ||
                string.IsNullOrWhiteSpace(Password))
            {
                MensagemErro = "Preencha todos os campos.";
                return Page();
            }

            // ✅ 2. Procurar utilizador
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u =>
                    u.Username == LoginInput ||
                    u.Email == LoginInput);

            if (utilizador == null)
            {
                MensagemErro = "Utilizador não encontrado.";
                return Page();
            }

            // ✅ 3. Validar password
            if (utilizador.PasswordHash != Password)
            {
                MensagemErro = "Password incorreta.";
                return Page();
            }

            // ✅ 4. Criar claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Name, utilizador.Username),
                new Claim(ClaimTypes.Email, utilizador.Email)
            };

            // ✅ 5. Criar identidade
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ✅ 6. Login (cookie)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            // ✅ 7. Redirecionar
            return RedirectToPage("/Dashboard/Index");
        }
    }
}