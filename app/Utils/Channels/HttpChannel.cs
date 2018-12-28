using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Fort.Utils.Channels
{
    public class HttpChannel : IChannel
    {
        public HttpChannel(string playerId)
        {
            PlayerId = playerId;
            _queue = new Queue<(string method, object data)>();
        }
        public string PlayerId { get; private set; }

        public Action<string, string> OnMessage { get; set; }
        public Action<string, string> OnDisconnect { get; set; }
        private Queue<(string method, object data)> _queue;

        public void Disconnect(string reason)
        {
            OnDisconnect(PlayerId, reason);
        }

        public void SendMessage(string method, object data)
        {
            _queue.Enqueue((method, data));
        }

        public string GetQueue()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            bool first = true;
            while (_queue.TryDequeue(out var item))
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");

                sb.Append(JsonConvert.SerializeObject(new { method = item.method, param = item.data }));
            }
            sb.AppendLine("]");

            return sb.ToString();
        }
    }
}