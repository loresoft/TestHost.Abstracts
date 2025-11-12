using Microsoft.EntityFrameworkCore;

namespace TestHost.Abstracts.Tests.Data;

public class SampleDataContext : DbContext
{
    public SampleDataContext(DbContextOptions<SampleDataContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.Email)
                .IsRequired();
        });
    }
}
