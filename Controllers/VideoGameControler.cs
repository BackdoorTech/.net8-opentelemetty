using System.Diagnostics;
using CleanArchitecture.Infrastructure.Interface;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Mvc;
using VideoGameApi;



using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



public class VideoGameSchema
{

  // [Required(ErrorMessage = "ID is required")]
  [Range(1, int.MaxValue, ErrorMessage = "ID must be a positive number")]
  public int Id { get; set;}

  [Required(ErrorMessage = "Street is required")]
  [MaxLength(100, ErrorMessage = "Street cannot exceed 100 characters")]
  public string Title { get; set; }

  [Required(ErrorMessage = "City is required")]
  public string Platform { get; set; }

  [Required(ErrorMessage = "Country is required")]
  public string Developer { get; set; }

  [Required(ErrorMessage = "Country is required")]
  public string Publisher { get; set; }
}

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


  [HttpGet("annotation")]
  public async Task<ActionResult> Anotation()
  {
    var activity = Activity.Current;

    activity.AddEvent(new ActivityEvent("start complex execution"));
    Thread.Sleep(500);
    activity.AddEvent(new ActivityEvent("Stop complex execution"));

    activity.SetTag("customTag", "123");

    Thread.Sleep(500);

    return StatusCode(200);
  }

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
      Right: games => StatusCode(500, games),       // Success: return 200 OK with the list of games
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
  //[ValidateModel]
  public async Task<ActionResult<VideoGame>> AddVideoGame(VideoGameSchema product) {

    var newGame = new VideoGame {
      Developer = product.Developer,
      Platform = product.Platform,
      Publisher = product.Publisher,
      Title = product.Title
    };

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
