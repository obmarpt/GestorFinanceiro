using System;
using System.Collections.Generic;
using System.Text;

namespace GestorFinanceiro.Data.Models
{
    public class SessaoCategoria
    {
        public int SessaoGestaoId { get; set; }
        public SessaoGestao SessaoGestao { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}
