using System;

namespace Fort.Models.Store
{
    public class TurnState
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public DateTime? EndsAt { get; set; }
    }
}