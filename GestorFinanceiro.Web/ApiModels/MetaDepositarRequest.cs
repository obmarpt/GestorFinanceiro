using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.ApiModels
{
    public class MetaDepositarRequest
    {
        [Required]
        public int SessaoOrigemId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }
}
