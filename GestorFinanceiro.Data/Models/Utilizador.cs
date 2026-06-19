using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestorFinanceiro.Data.Models
{
    public class Utilizador
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Utilizador";

        public string? ImagemPerfil { get; set; }

        [JsonIgnore]
        public ICollection<SessaoFinanceira> SessoesFinanceiras { get; set; }
            = new List<SessaoFinanceira>();
    }
}