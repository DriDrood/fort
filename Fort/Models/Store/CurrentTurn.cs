using System;
using Fort.Services;

namespace Fort.Models.Store
{
    public class CurrentTurn
    {
        public int Id { get; set; }
        public string State { get; set; }
        public DateTime? EndsAt { get; set; }
        public Turn Turn { get; set; }
    }
}