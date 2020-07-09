using System;
using System.Threading;

namespace go
{
    public static class time_package
    {
        public const long Millisecond = TimeSpan.TicksPerMillisecond;

        public static void Sleep(long ticks) => Thread.Sleep((int)(ticks / TimeSpan.TicksPerMillisecond));
    }
}
