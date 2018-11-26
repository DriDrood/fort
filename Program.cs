using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Fort.Models;

namespace Fort
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host = CreateWebHostBuilder(args).Build();
            Host.Run();
        }
        
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(opt => opt.Listen(IPAddress.Loopback, 80))
                .UseStartup<Startup>();

        public static FortConfig Config { get; set; } = new FortConfig();
        public static IWebHost Host { get; private set; }
        public static TService GetService<TService>() => Host.Services.GetService<TService>();
    }
}
