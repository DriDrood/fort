using Microsoft.AspNetCore.Mvc;

namespace Fort.Controllers
{
    public class PlayController : Controller
    {
        // public PlayController(FortDbContext context, CurrentPlayerService currentPlayerService, CommService commService)
        // {
        //     _context = context;
        //     _currentPlayerService = currentPlayerService;
        //     _commService = commService;
        // }

        // private FortDbContext _context;
        // private CurrentPlayerService _currentPlayerService;
        // private CommService _commService;

        // public IActionResult Login()
        // {
        //     return View();
        // }
        // [HttpPost]
        // public IActionResult Login(string code)
        // {
        //     Player player = (Player)_context.Users.Find(code)
        //         ?? _context.Teams.Find(code);

        //     if (player != null)
        //         return RedirectToAction("Map", new { code = code });

        //     Logger.Log(ELogLevel.Warning, code, "Neplatný kód!");
        //     ViewData["errorMessage"] = "Neplatný kód!";
        //     return View();
        // }

        // public IActionResult Map(string code)
        // {
        //     ViewData["player"] = _currentPlayerService;
        //     return View(MapBaseService.GetMapServiceForPlayer(_context, _currentPlayerService.Player));
        // }
        // public IActionResult Connect(string code)
        // {
        //     if (!_httpChannels.ContainsKey(code))
        //     {
        //         var channel = new HttpChannel(code);
        //         _commService.CreateNewConnection(channel);
        //         _httpChannels.Add(code, channel);
        //     }

        //     return Ok("Done");
        // }
        // public IActionResult GetQueue(string code)
        // {
        //     if (!_httpChannels.ContainsKey(code))
        //         return Ok(new { method = "notification", param = new { type = "error", message = "Nejste připojen" } });

        //     return Ok(_httpChannels[code].GetQueue());
        // }
        // [HttpPost]
        // public IActionResult PostMessage(string code, [FromBody]JToken message)
        // {
        //     if (!_httpChannels.ContainsKey(code))
        //         return Ok(new { method = "notification", param = new { type = "error", message = "Nejste připojen" } });

        //     _httpChannels[code].OnMessage(code, message.ToString());

        //     return Ok("Done");
        // }

        // private static Dictionary<string, HttpChannel> _httpChannels = new Dictionary<string, HttpChannel>();
    }
}