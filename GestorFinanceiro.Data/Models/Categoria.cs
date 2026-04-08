using System;
using System.Collections.Generic;
using System.Text;

namespace GestorFinanceiro.Data.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public ICollection<Gasto> Gastos { get; set; }
            = new List<Gasto>();

        public ICollection<SessaoCategoria> SessaoCategorias { get; set; }
            = new List<SessaoCategoria>();
    }
}
