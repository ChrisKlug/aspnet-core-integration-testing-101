using Microsoft.EntityFrameworkCore;

namespace Demo.AspNet.IntegrationTesting.Api.Data;

public class ToDosDbContext : DbContext
{
    public ToDosDbContext(DbContextOptions<ToDosDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToDoItem>(x => {
            x.ToTable("ToDos");

            x.Ignore(x => x.IsCompleted);

            x.HasKey(x => x.Id);
        });
    }
}