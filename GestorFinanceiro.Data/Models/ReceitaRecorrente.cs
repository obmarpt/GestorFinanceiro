using System;
using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Data.Models
{
    public class ReceitaRecorrente
    {
        public int Id { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        [Range(1, 31)]
        public int DiaDoMes { get; set; }

        public DateTime DataInicio { get; set; } = DateTime.Now;

        public DateTime? DataFim { get; set; }

        public bool Ativa { get; set; } = true;

        [Required]
        public int SessaoFinanceiraId { get; set; }
        public SessaoFinanceira SessaoFinanceira { get; set; }
    }
}
