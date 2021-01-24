using System;
using System.Collections.Generic;

namespace Fort.Services
{
    public class CloseService
    {
        public void Close(Guid userId, bool done)
        {
            _usersDone[userId] = done;
        }

        public void RestoreAll()
        {
            _usersDone.Clear();
        }

        public bool IsClosed(Guid userId) => _usersDone.TryGetValue(userId, out var closed) ? closed : false;

        private Dictionary<Guid, bool> _usersDone = new Dictionary<Guid, bool>();
    }
}