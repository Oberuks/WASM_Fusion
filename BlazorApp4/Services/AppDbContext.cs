using Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp4.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Countries { get; protected set; } = null!;
    }
}
