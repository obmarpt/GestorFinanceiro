using GestorFinanceiro.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

// ✅ ADICIONAR ISTO
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

        // ✅ Username OU Email
        [BindProperty]
        public string LoginInput { get; set; }

        // ✅ Password
        [BindProperty]
        public string Password { get; set; }

        // ✅ Mensagens
        public string MensagemErro { get; set; }
        public string MensagemSucesso { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Dashboard/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ✅ 1. Verificar campos vazios
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

            // ✅ 4. LOGIN REAL COM SESSÃO

            // criar claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Name, utilizador.Username),
                new Claim(ClaimTypes.Email, utilizador.Email)
            };

            // criar identidade
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // criar principal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // guardar cookie (login)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            // ✅ redirecionar após login
            return RedirectToPage("/Dashboard/Index");
        }
    }
}