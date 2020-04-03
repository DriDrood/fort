using Fort.Services;

namespace Fort.Models.Params
{
    public class CheckParams
    {
        public ELifecycleState State { get; set; }
        public int TurnId { get; set; }
    }
}