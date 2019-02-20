using System;
using System.Collections.Generic;
using Fort.Database;
using Fort.Database.Entities;
using Fort.Module;
using Fort.Module.Army;
using Fort.Module.Comm;
using Fort.Utils.Channels;
using Fort.Utils.Logger;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Fort.Controllers
{
    public class PlayController : Controller
    {
        public PlayController(ContextService context, CommService commService)
        {
            _context = context;
            _commService = commService;
            _armyService = context.GetArmyService();
        }
        private ContextService _context;
        private CommService _commService;
        private ArmyService _armyService;

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string code)
        {
            Player player = (Player)_context.Database.Users.Find(code)
                ?? _context.Database.Teams.Find(code);

            if (player != null)
                return RedirectToAction("Map", new { code = code });

            Logger.Log(ELogLevel.Warning, code, "Neplatný kód!");
            ViewData["errorMessage"] = "Neplatný kód!";
            return View();
        }

        public string GenerateGuid()
        {
            Random rand = new Random();
            char[] guid = new char[5];
            for (int i = 0; i < 5; i++)
            {
                var chI = rand.Next() % 36;
                // char
                if (chI < 26)
                    guid[i] = (char)(chI + 97);
                else
                    guid[i] = (char)(chI - 26 + 48);
            }

            return new string(guid);
        }

        public IActionResult Map(string code)
        {
            ViewData["initData"] = _armyService.GetInit();
            return View(_context);
        }

        // #region HttpChannel
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
        // #endregion
    }
}