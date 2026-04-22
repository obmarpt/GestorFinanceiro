namespace GestorFinanceiro.Data.Models
{
    public class SessaoFinanceiraCategoria
    {
        public int SessaoFinanceiraId { get; set; }
        public SessaoFinanceira SessaoFinanceira { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}