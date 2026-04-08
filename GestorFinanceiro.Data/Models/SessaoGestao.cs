using System;
using System.Collections.Generic;
using System.Text;

namespace GestorFinanceiro.Data.Models
{
    public class SessaoGestao
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public int UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; }

        public ICollection<Gasto> Gastos { get; set; }
            = new List<Gasto>();

        public ICollection<SessaoCategoria> SessaoCategorias { get; set; }
            = new List<SessaoCategoria>();
    }
}
