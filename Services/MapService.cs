using System;
using System.Collections.Generic;
using Fort.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Fort.Services
{
    public class MapService
    {
        public MapService(IConfiguration configuration)
        {
            Helpers.Position.RealHeight = double.Parse(configuration["realResolution:height"]);
            Helpers.Position.RealWidth = double.Parse(configuration["realResolution:width"]);
        }

        private string _map =
@"
{
    ""Forts"": {
      ""one"": {
        ""x"": 50,
        ""y"": 50
      },
      ""two"": {
        ""x"": 100,
        ""y"": 100
      },
      ""five"": {
        ""x"": 150,
        ""y"": 80
      },
      ""far"": {
        ""x"": 823,
        ""y"": 683
      }
    },
    ""Paths"": {
        ""1"": {
            ""Source"":""one"",
            ""Target"":""two""
        },
        ""2"": {
            ""Source"":""one"",
            ""Target"":""five""
        },
        ""3"": {
            ""Source"":""five"",
            ""Target"":""two""
        },
        ""4"": {
            ""Source"":""five"",
            ""Target"":""far""
        }
    }
}
";

        public Dictionary<string, Fortress> Fortresses { get; private set; }

        public List<Path> Paths { get; private set; }

        public void Load()
        {
            JToken items = JToken.Parse(_map);
            Random rand = new Random();

            Fortresses = new Dictionary<string, Fortress>();
            foreach (JProperty fort in items["Forts"])
            {
                Fortresses.Add(fort.Name, new Fortress
                {
                    Name = fort.Name,
                    X = fort.Value["x"].Value<int>(),
                    Y = fort.Value["y"].Value<int>(),
                    Population = rand.Next() % 100,
                    Owner = new Owner
                    {
                        Color = $"rgb({rand.Next() % 256}, {rand.Next() % 256}, {rand.Next() % 256})"
                    }
                });
            }

            Paths = new List<Path>();
            foreach (JProperty path in items["Paths"])
            {
                Paths.Add(new Path
                {
                    Id = path.Name,
                    Source = Fortresses[path.Value["Source"].Value<string>()],
                    Target = Fortresses[path.Value["Target"].Value<string>()]
                });
            }
        }
    }
}