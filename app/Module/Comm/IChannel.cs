using System;
using System.Threading.Tasks;

namespace app.Module.Comm
{
    public interface IChannel
    {
        string PlayerId { get; }
        bool ReadyToSend { get; }
        CommService Comm { set; }
        Queue Q { set; }

        Task SendMessage(string method, object data);
        void Disconnect(string reason);
    }
}