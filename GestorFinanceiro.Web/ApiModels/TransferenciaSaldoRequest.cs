using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.ApiModels
{
    public class TransferenciaSaldoRequest
    {
        [Required]
        public int SessaoOrigemId { get; set; }

        [Required]
        public int SessaoDestinoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        public string? Descricao { get; set; }
    }
}
