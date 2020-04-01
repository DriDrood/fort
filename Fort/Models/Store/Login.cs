using System;

namespace Fort.Models.Store
{
    public class Login
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string JwtToken { get; set; }
    }
}