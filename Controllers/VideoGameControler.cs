using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameApi;

[ApiController]
[Route("api/[controller]")]
public class VideoGameController(VideoGameDbContext context) : ControllerBase
{

  private readonly VideoGameDbContext _context = context;

  static private List<VideoGame> videoGames = new List<VideoGame> {
    new VideoGame {
      Id = 1,
      Title = "Spiderman",
      Platform = "ps5",
      Developer = "Insomiac game",
      Publisher = "sony interactive Entertaiment"
    }
  };

  [HttpGet]
  public async Task<ActionResult<List<VideoGame>>> GetProducts()
  {
    return Ok(await _context.VideoGames.ToListAsync());
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VideoGame>> GetVideoGameById(int id) {

    var game = await _context.VideoGames.FindAsync(id);

    if(game is null) {
      return NotFound();
    }

    return Ok(game);
  }


  [HttpPost]
  public async Task<ActionResult<VideoGame>> AddVideoGame(VideoGame newGame) {
    if(newGame is null) {
      return BadRequest();
    }

    //newGame.Id = videoGames.Max(g => g.Id) + 1;
    //videoGames.Add(newGame);
    _context.VideoGames.Add(newGame);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetVideoGameById), new { id = newGame.Id}, newGame);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateVideoGame(int id, VideoGame updatesGame) {
    var game = await _context.VideoGames.FindAsync(id);

    if(game is null) {
      return NotFound();
    }

    game.Title = updatesGame.Title;
    game.Platform = updatesGame.Platform;
    game.Developer = updatesGame.Developer;
    game.Publisher = updatesGame.Publisher;

    await _context.SaveChangesAsync();

    return NoContent();
  }


  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteVideoGame(int id) {
    var game = await _context.VideoGames.FindAsync(id);

    if(game is null) {
      return NotFound();
    }
    _context.VideoGames.Remove(game);
    await _context.SaveChangesAsync();
    return NoContent();
  }
}
