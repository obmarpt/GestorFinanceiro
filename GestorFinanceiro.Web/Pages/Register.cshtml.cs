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
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string MensagemErro { get; set; }
        public string MensagemSucesso { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // ✅ 1. Validar campos vazios
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                MensagemErro = "Preencha todos os campos.";
                return Page();
            }

            // ✅ 2. Validar passwords
            if (Password != ConfirmPassword)
            {
                MensagemErro = "As passwords não coincidem.";
                return Page();
            }

            // ✅ 3. Verificar se email já existe
            bool existe = await _context.Utilizadores
                .AnyAsync(u => u.Email == Email);

            if (existe)
            {
                MensagemErro = "Já existe uma conta com este email.";
                return Page();
            }

            // ✅ 4. Criar utilizador
            var utilizador = new Utilizador
            {
                Email = Email,
                PasswordHash = Password // ⚠️ depois vamos fazer hash
            };

            // ✅ 5. Guardar na BD
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();

            MensagemSucesso = "Conta criada com sucesso!";

            return Page();
        }
    }
}