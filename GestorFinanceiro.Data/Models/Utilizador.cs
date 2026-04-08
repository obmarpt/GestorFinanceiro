
using System;
using System.Collections.Generic;
using System.Text;

namespace GestorFinanceiro.Data.Models
{
    public class Utilizador
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Utilizador";

        public ICollection<SessaoGestao> SessoesGestao { get; set; }
            = new List<SessaoGestao>();
    }
}
