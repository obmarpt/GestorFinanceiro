using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.ApiModels
{
    public class MetaRequest
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor alvo deve ser maior que zero.")]
        public decimal ValorAlvo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValorAtual { get; set; } = 0;

        [Required]
        public int UtilizadorId { get; set; }
    }
}
