namespace GestorFinanceiro.Web.Models
{
    public class HistoricoItemViewModel
    {
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}
