using System;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Data.Models
{
    public class Despesa
    {
        public int Id { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        [Required]
        public int SessaoFinanceiraId { get; set; }
        public SessaoFinanceira SessaoFinanceira { get; set; }

        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}