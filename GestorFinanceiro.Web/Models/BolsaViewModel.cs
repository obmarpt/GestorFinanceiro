namespace GestorFinanceiro.Web.Models
{
    public class BolsaViewModel
    {
        public int UtilizadorId { get; set; }
        public decimal Saldo { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool TemSaldo => Saldo > 0;
    }
}
