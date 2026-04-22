using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestorFinanceiro.Data.Context
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=GestorFinanceiroDb;Trusted_Connection=True;MultipleActiveResultSets=true");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}


//Código gerado por ChatGPT, um modelo de linguagem desenvolvido pela OpenAI.
//O código é fornecido para auxiliar nas migracoes uma vez que com o programa offline o context padrao nao é fornecido

//🧠 Para que serve isto?

//Permite ao EF criar o DbContext quando corres Add-Migration
//Funciona fora da API/Web
//Só é usado em tempo de design

//✅ Isto é normal e correto em soluções multi‑projeto