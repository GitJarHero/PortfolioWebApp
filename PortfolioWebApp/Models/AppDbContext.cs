using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models.Entities;

namespace PortfolioWebApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User-UserRole Many-to-Many Mapping mit expliziten FK-Namen
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "map_user_role", // Name der Join-Tabelle
                    j => j
                        .HasOne<UserRole>()
                        .WithMany()
                        .HasForeignKey("role_id")
                        .HasConstraintName("fk_userrole"),
                    j => j
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("user_id")
                        .HasConstraintName("fk_user"),
                    j =>
                    {
                        j.HasKey("user_id", "role_id");
                        j.ToTable("map_user_role");
                    });

            // UserState als Integer speichern
            modelBuilder.Entity<User>()
                .Property(u => u.State)
                .HasConversion<int>();

            // Optional: Konventionen für Tabellen-/Spaltennamen
            modelBuilder.Entity<User>().ToTable("appuser");
            modelBuilder.Entity<UserRole>().ToTable("userrole");
            
            
            
            
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.ToTable("friendship");

                entity.HasOne(f => f.User1)
                    .WithMany()
                    .HasForeignKey("user1")
                    .HasConstraintName("fk_friendship_user1")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.User2)
                    .WithMany()
                    .HasForeignKey("user2")
                    .HasConstraintName("fk_friendship_user2")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasKey("user1", "user2");

                entity.Property(f => f.Created)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            
            
        }
    }
}