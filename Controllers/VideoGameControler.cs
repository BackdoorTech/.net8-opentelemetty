using System.Diagnostics;
using CleanArchitecture.Infrastructure.Interface;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Mvc;
using VideoGameApi;

[ApiController]
[Route("api/[controller]")]
public class VideoGameController(IVideoGameRepository repository) : ControllerBase
{

  private readonly IVideoGameRepository _repository = repository;

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
    // Get the current Activity for the request
    var activity = Activity.Current;
    // Add custom tags to the Activity
    activity?.SetTag("custom.tag.name", "CustomTagValue");
    activity?.SetTag("http.user_agent", Request.Headers["User-Agent"].ToString());
    activity?.SetTag("http.endpoint", HttpContext.Request.Path);

    var result = await _repository.GetAllAsync();

    return result.Match<ActionResult<List<VideoGame>>>(
      Right: games => Ok(games),       // Success: return 200 OK with the list of games
      Left: error => StatusCode(500, error)
    );
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VideoGame>> GetVideoGameById(int id) {

    var result = await _repository.GetByIdAsync(id);

    if(result.IsRight && result.ValueUnsafe() is not null) {
      return Ok(result.ValueUnsafe());
    } else if (result.ValueUnsafe() is null) {
      return NotFound();
    } else if(result.IsLeft) {
      return StatusCode(500, result.ToString());
    }


    // Return generic unexpected error
    return StatusCode(500, "Unexpected error");
  }


  [HttpPost]
  public async Task<ActionResult<VideoGame>> AddVideoGame(VideoGame newGame) {
    if(newGame is null) {
      return BadRequest();
    }

    //newGame.Id = videoGames.Max(g => g.Id) + 1;
    //videoGames.Add(newGame);
    await _repository.AddAsync(newGame);
    var rowsAffected = await _repository.SaveChangesAsync();

    if(rowsAffected.IsRight) {
      if(rowsAffected.ValueUnsafe() > 0) {
        return  CreatedAtAction(nameof(GetVideoGameById), new { id = newGame.Id}, newGame);
      } else {
        return StatusCode(500, "Unexpected");
      }

    } else if(rowsAffected.IsLeft) {
      return StatusCode(500, rowsAffected.ToString());
    }

    return StatusCode(500, "Unexpected end");
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateVideoGame(int id, VideoGame updatesGame) {
    var result = await _repository.GetByIdAsync(id);

    if(result.IsLeft) {
      return NotFound();
    }

    var game = result.ValueUnsafe();
    game.Title = updatesGame.Title;
    game.Platform = updatesGame.Platform;
    game.Developer = updatesGame.Developer;
    game.Publisher = updatesGame.Publisher;

    await _repository.SaveChangesAsync();

    return NoContent();
  }


  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteVideoGame(int id) {
    var result = await _repository.GetByIdAsync(id);


    if(result.IsLeft) {
      return NotFound();
    }

    var game = result.ValueUnsafe();
    _repository.Delete(game);
    await _repository.SaveChangesAsync();

    return NoContent();
  }
}
