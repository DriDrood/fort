using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Module.Comm
{
    public class Queue
    {
        public Queue(CommService comm)
        {
            _queue = new Dictionary<Guid, QueueItem>();
            _comm = comm;
        }

        private Dictionary<Guid, QueueItem> _queue;
        private CommService _comm;

        public void AddItem(QueueItem item)
        {
            // save to Redis
            _queue.Add(item.Id, item);
        }

        public QueueItem GetItem(Guid id)
        {
            // get from redis
            var value = _queue[id];
            _queue.Remove(id);
            return value;
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return _queue.Values;
        }

        public void Reset()
        {
            foreach (Guid key in _queue.Keys.ToList())
            {
                if (_queue[key].Lifetime != Lifetime.Important)
                    _queue.Remove(key);
            }
        }
    }
}