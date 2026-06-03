using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestorFinanceiro.Data.Models
{
    public class SessaoFinanceira
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Required]
        public int UtilizadorId { get; set; }

        [JsonIgnore]
        public Utilizador? Utilizador { get; set; }

        public ICollection<Receita> Receitas { get; set; }
            = new List<Receita>();

        public ICollection<ReceitaRecorrente> ReceitasRecorrentes { get; set; }
            = new List<ReceitaRecorrente>();

        public ICollection<Despesa> Despesas { get; set; }
            = new List<Despesa>();

        public ICollection<SessaoFinanceiraCategoria> SessaoFinanceiraCategorias { get; set; }
            = new List<SessaoFinanceiraCategoria>();
    }
}
