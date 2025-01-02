using Microsoft.EntityFrameworkCore;

namespace VideoGameApi {

  public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): DbContext(options) {
    public DbSet<VideoGame> VideoGames => Set<VideoGame>();
    public DbSet<ErrorLog> ErrorLog => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<VideoGame>().HasData(
        new VideoGame{
          Id = 1,
          Title = "Spider-man",
          Developer = "",
          Platform = "",
          Publisher = ""
        }
      );
    }
  }
}
