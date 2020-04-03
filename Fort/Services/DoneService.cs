using System;
using System.Collections.Generic;

namespace Fort.Services
{
    public class DoneService
    {
        public void Done(Guid userId, bool done)
        {
            _usersDone[userId] = done;
        }

        private Dictionary<Guid, bool> _usersDone = new Dictionary<Guid, bool>();
    }
}