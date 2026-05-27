using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestorFinanceiro.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public string Username { get; set; } = "";

        public void OnGet()
        {
            Username = User.Identity?.Name ?? "Utilizador";
        }
    }
}
