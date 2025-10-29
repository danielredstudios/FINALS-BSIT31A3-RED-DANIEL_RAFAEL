using BookSwapHub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookSwapHub.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Book> Books => Set<Book>();
    public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Book>(b =>
        {
            b.HasOne(x => x.Owner)
             .WithMany()
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SwapRequest>(sr =>
        {
            sr.HasOne(x => x.FromUser)
              .WithMany()
              .HasForeignKey(x => x.FromUserId)
              .OnDelete(DeleteBehavior.Restrict);

            sr.HasOne(x => x.ToUser)
              .WithMany()
              .HasForeignKey(x => x.ToUserId)
              .OnDelete(DeleteBehavior.Restrict);

            sr.HasOne(x => x.FromBook)
              .WithMany()
              .HasForeignKey(x => x.FromBookId)
              .OnDelete(DeleteBehavior.Restrict);

            sr.HasOne(x => x.ToBook)
              .WithMany()
              .HasForeignKey(x => x.ToBookId)
              .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
