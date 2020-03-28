using Fort.Models.Config;
using Microsoft.Extensions.Configuration;

namespace Fort.Managers
{
    public class ConfigManager
    {
        public ConfigManager(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public string ConnectionString { get; }
        public JwtTokenConfig JwtToken { get; }
    }
}