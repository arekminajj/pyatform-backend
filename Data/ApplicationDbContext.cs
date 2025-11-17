using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using pyatform.Models;

namespace pyatform.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<Solution> Solutions { get; set; } 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Challenge>()
            .HasMany(c => c.Solutions)
            .WithOne(s => s.Challenge)
            .HasForeignKey(s => s.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Solution>()
                .HasOne(s => s.User)
                .WithMany(u => u.Solutions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Challenge>()
                .HasOne(c => c.User)
                .WithMany(u => u.Challenges)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}
