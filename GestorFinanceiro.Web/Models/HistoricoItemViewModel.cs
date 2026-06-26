namespace GestorFinanceiro.Web.Models
{
    public class HistoricoItemViewModel
    {
        public int RegistoId { get; set; }
        public string TipoRegisto { get; set; } = string.Empty; // "receita", "despesa", "metamovimento"
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}
