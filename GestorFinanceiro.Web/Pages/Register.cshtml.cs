using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? MensagemErro { get; set; }
        public string? MensagemSucesso { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToPage("/Dashboard/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                MensagemErro = "Preencha todos os campos.";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                MensagemErro = "As passwords não coincidem.";
                return Page();
            }

            if (await _context.Utilizadores.AnyAsync(u => u.Email == Email))
            {
                MensagemErro = "Já existe uma conta com este email.";
                return Page();
            }

            if (await _context.Utilizadores.AnyAsync(u => u.Username == Username))
            {
                MensagemErro = "Já existe uma conta com este username.";
                return Page();
            }

            _context.Utilizadores.Add(new Utilizador
            {
                Nome = Nome.Trim(),
                Username = Username.Trim(),
                Email = Email.Trim(),
                PasswordHash = Password,
                Role = "Utilizador"
            });

            await _context.SaveChangesAsync();
            MensagemSucesso = "Conta criada com sucesso! Pode fazer login.";
            return Page();
        }
    }
}
