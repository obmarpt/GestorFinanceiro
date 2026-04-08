using System;
using System.Collections.Generic;
using System.Text;

namespace GestorFinanceiro.Data.Models
{
    public class Gasto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public DateTime Data { get; set; }

        public string? Descricao { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public int SessaoGestaoId { get; set; }
        public SessaoGestao SessaoGestao { get; set; }
    }
}
