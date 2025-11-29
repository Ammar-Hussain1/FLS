using Microsoft.EntityFrameworkCore;
using FLS_API.DL.Models;

namespace FLS_API.DL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserMemory> UserMemory { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<UserMemory>()
                .HasOne(um => um.User)
                .WithMany(u => u.Memories)
                .HasForeignKey(um => um.UserId);

            modelBuilder.Entity<ChatLog>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.ChatLogs)
                .HasForeignKey(cl => cl.UserId);
        }
    }
}