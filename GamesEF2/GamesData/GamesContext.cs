using GamesModels;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Remoting.Contexts;

namespace GamesData
{
    public class GamesContext : DbContext
    {
        public DbSet<Game> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к SQL Server LocalDB
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=GamesDb;Trusted_Connection=True;");
        }
    }
}