using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Kanban.Models;

namespace Kanban.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<BoardUser> BoardUsers => Set<BoardUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Board>()
            .HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Column>()
            .HasOne(c => c.Board)
            .WithMany(bd => bd.Columns)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Card>()
            .HasOne(c => c.Column)
            .WithMany(col => col.Cards)
            .HasForeignKey(c => c.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BoardUser>()
            .HasKey(x => new { x.BoardId, x.UserId });

        builder.Entity<BoardUser>()
            .HasOne(x => x.Board)
            .WithMany(bd => bd.Members)
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BoardUser>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.Entity<Column>().HasIndex(c => new { c.BoardId, c.Order });
        builder.Entity<Card>().HasIndex(c => new { c.ColumnId, c.Order });
    }
}