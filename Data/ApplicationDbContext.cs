using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using pyatform.Models;
using pyatform.DTOs.User;

namespace pyatform.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<Solution> Solutions { get; set; } 
    public DbSet<TestResult> TestResults { get; set; }
    public DbSet<TopUser> TopUser { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TopUser>()
            .HasNoKey();

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

        modelBuilder.Entity<Solution>()
                .HasOne(s => s.TestResult)
                .WithOne(tr => tr.Solution)
                .HasForeignKey<TestResult>(tr => tr.SolutionId)
                .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Challenge>()
                .HasOne(c => c.User)
                .WithMany(u => u.Challenges)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}
