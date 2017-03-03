using System;
using System.Threading;

namespace Server
{
    internal class EventManager
    {
        internal AutoResetEvent event1;
        internal AutoResetEvent event2;
        internal AutoResetEvent event3;

        public EventManager()
        {
            event1 = new AutoResetEvent(false);
            event2 = new AutoResetEvent(false);
            event3 = new AutoResetEvent(false);
        }

    }
}