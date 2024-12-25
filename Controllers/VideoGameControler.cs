using Microsoft.AspNetCore.Mvc;
using VideoGameApi;

[ApiController]
[Route("api/[controller]")]
public class VideoGameController : ControllerBase
{

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
    public ActionResult<List<VideoGame>> GetProducts()
    {
        return Ok(videoGames);
    }

    [HttpGet("{id}")]
    public ActionResult<VideoGame> GetVideoGameById(int id) {

        var game = videoGames.FirstOrDefault(f => f.Id == id);

        if(game is null) {
            return NotFound();
        } 

        return Ok(game);
    }


    [HttpPost]
    public ActionResult<VideoGame> AddVideoGame(VideoGame newGame) {
        if(newGame is null) {
            return BadRequest();
        }

        newGame.Id = videoGames.Max(g => g.Id) + 1;
        videoGames.Add(newGame);
        return CreatedAtAction(nameof(GetVideoGameById), new { id = newGame.Id}, newGame);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateVideoGame(int id, VideoGame updatesGame) {
        var game = videoGames.FirstOrDefault(f => f.Id == id);

        if(game is null) {
            return NotFound();
        } 

        game.Title = updatesGame.Title;
        game.Platform = updatesGame.Platform;
        game.Developer = updatesGame.Developer;
        game.Publisher = updatesGame.Publisher;

        return NoContent();
    }


    [HttpDelete("{id}")]
    public IActionResult DeleteVideoGame(int id) {
        var game = videoGames.FirstOrDefault(f => f.Id == id);

        if(game is null) {
            return NotFound();
        } 

        videoGames.Remove(game);

        return NoContent();
    }
}
