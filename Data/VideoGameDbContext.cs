using Microsoft.EntityFrameworkCore;

namespace VideoGameApi {

  public class VideoGameDbContext(DbContextOptions<VideoGameDbContext> options): DbContext(options) {
    public DbSet<VideoGame> VideoGames => Set<VideoGame>();

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
