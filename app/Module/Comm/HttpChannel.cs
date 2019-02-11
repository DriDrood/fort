using System.Threading.Tasks;

namespace app.Module.Comm
{
    public class HttpChannel : IChannel
    {
        public HttpChannel(string playerId)
        {
            PlayerId = playerId;
        }

        public string PlayerId { get; private set; }
        public bool ReadyToSend => false;
        public CommService Comm { get; set; }
        public Queue Q { get; set; }

        public void Disconnect(string reason)
        {
            Comm.OnDisconnect(PlayerId, reason);
        }

        public Task SendMessage(string method, object data)
        {
            throw new System.NotImplementedException();
        }
    }
}