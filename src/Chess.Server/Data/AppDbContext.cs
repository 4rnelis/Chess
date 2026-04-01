using Microsoft.EntityFrameworkCore;
using Chess.Server.Models;

namespace Chess.Server.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public string DbPath { get; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "chess.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }
}
