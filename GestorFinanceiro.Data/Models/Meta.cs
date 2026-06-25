using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Data.Models
{
    public class Meta
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        public decimal ValorAlvo { get; set; }

        public decimal ValorAtual { get; set; } = 0;

        public int UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; } = null!;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
