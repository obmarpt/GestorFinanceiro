namespace GestorFinanceiro.Web.Models
{
    public class MetaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal ValorAlvo { get; set; }
        public decimal ValorAtual { get; set; }
        public int UtilizadorId { get; set; }
        public DateTime DataCriacao { get; set; }

        public decimal Percentagem => ValorAlvo > 0
            ? Math.Min(Math.Round(ValorAtual / ValorAlvo * 100, 1), 100)
            : 0;

        public bool Concluida => ValorAtual >= ValorAlvo;
    }
}
