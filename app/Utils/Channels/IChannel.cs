using System;
using System.Threading.Tasks;

namespace Fort.Utils.Channels
{
    public interface IChannel
    {
        string PlayerId { get; }
        void SendMessage(string method, object data);
        void Disconnect(string reason);
        Action<string, string> OnMessage { set; }
        Action<string, string> OnDisconnect { set; }
    }
}