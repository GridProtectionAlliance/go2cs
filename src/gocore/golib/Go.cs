using System;
using System.Threading;

namespace go
{
    public class GoRoutine
    {
        public static bool operator <(GoRoutine src, Action action)
        {
            ThreadPool.QueueUserWorkItem(_ => action());
            return true;
        }
        public static bool operator >(GoRoutine arc, Action _)
        {
            return false;
        }
    }
}
