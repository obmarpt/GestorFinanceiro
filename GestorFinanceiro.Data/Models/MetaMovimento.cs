namespace GestorFinanceiro.Data.Models
{
    public class MetaMovimento
    {
        public int Id { get; set; }
        public int MetaId { get; set; }
        public Meta Meta { get; set; } = null!;
        public string Tipo { get; set; } = string.Empty; // "Deposito" ou "Levantamento"
        public decimal Valor { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;
    }
}
