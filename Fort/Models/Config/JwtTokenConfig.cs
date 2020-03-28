namespace Fort.Models.Config
{
    public class JwtTokenConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string PrivateKey { get; set; }
    }
}