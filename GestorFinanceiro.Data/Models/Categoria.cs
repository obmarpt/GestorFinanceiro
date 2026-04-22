using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Data.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public ICollection<SessaoFinanceiraCategoria> SessaoFinanceiraCategorias { get; set; }
            = new List<SessaoFinanceiraCategoria>();
    }
}