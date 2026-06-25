namespace GestorFinanceiro.Data.Models
{
    public class Bolsa
    {
        public int Id { get; set; }
        public int UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; } = null!;
        public decimal Saldo { get; set; } = 0;
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    }
}
