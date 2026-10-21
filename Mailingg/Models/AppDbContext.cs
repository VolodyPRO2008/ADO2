using Microsoft.EntityFrameworkCore;
using System.Runtime.Remoting.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<MailingSection> MailingSections { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ArchiveProduct> ArchiveProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=NewsletterDb;Trusted_Connection=True;");
    }
}