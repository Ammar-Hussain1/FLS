using Microsoft.EntityFrameworkCore;
using FLS_API.DL.Models;

namespace FLS_API.DL
{
    /// <summary>
    /// DbContext for EF Core operations (if needed)
    /// Note: Primary database is Supabase, this is for migrations/local dev only
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserMemory> UserMemory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // UserMemory configuration
            modelBuilder.Entity<UserMemory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Importance });
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            });
        }
    }
}