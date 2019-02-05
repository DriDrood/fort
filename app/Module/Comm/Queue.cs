using System;

namespace app.Module.Comm
{
    public class Queue
    {
        public void AddItem(QueueItem item)
        {
            // save to DB
        }

        public QueueItem GetItem()
        {
            return null;
        }

        public event Action<QueueItem> OnAdd;
    }
}