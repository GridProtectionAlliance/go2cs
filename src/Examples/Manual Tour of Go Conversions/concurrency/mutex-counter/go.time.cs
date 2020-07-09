using System;
using System.Threading;
using Timer = System.Timers.Timer;
using static go.builtin;

namespace go
{
    public static class time_package
    {
        private class ChannelTimer
        {
            private readonly channel<bool> m_notify;
            private readonly Timer m_timer;

            public ChannelTimer(long ticks, bool autoReset)
            {
                m_notify = make_channel<bool>();
                m_timer = new Timer
                {
                    Interval = ticks / (double)TimeSpan.TicksPerMillisecond,
                    AutoReset = autoReset
                };
                m_timer.Elapsed += (_, e) => m_notify.Send(true);
            }

            public channel<bool> Channel
            {
                get
                {
                    m_timer.Start();
                    return m_notify;
                }
            }
        }

        public const long Millisecond = TimeSpan.TicksPerMillisecond;

        public const long Second = TimeSpan.TicksPerSecond;

        public static void Sleep(long ticks) => Thread.Sleep((int)(ticks / TimeSpan.TicksPerMillisecond));

        public static channel<bool> Tick(long ticks) => new ChannelTimer(ticks, true).Channel;

        public static channel<bool> After(long ticks) => new ChannelTimer(ticks, false).Channel;
    }
}
