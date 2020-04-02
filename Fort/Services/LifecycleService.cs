using System;
using System.Collections.Generic;

namespace Fort.Services
{
    public class LifecycleService
    {
        public int CurrentTurnId { get; }

        public void Done(Guid userId, bool done)
        {
            _usersDone[userId] = done;
        }

        private Dictionary<Guid, bool> _usersDone = new Dictionary<Guid, bool>();
    }
}