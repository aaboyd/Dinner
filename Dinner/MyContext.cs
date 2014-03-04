using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AlexBoyd.Dinner
{
  public interface IMyContext
  {
    IDbSet<Dinner> Dinners { get; set; }

    DbEntityEntry Entry(object entity);
    int SaveChanges();
  }

  public class MyContext : DbContext, IMyContext
  {
    public MyContext() { }
    public MyContext(string connectionString) : base(connectionString) { }

    public IDbSet<Dinner> Dinners { get; set; }

    protected override void OnModelCreating(DbModelBuilder builder) 
    {
      builder.Entity<Dinner>().Property(d => d.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
      builder.Entity<Dinner>().HasKey(d => d.Id);
    }
  }
}