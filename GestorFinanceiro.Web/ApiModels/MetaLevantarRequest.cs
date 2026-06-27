using System.ComponentModel.DataAnnotations;

namespace GestorFinanceiro.Web.ApiModels
{
    public class MetaLevantarRequest
    {
        [Required]
        public int SessaoDestinoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }
}
