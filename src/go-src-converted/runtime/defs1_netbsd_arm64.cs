// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_netbsd.go defs_netbsd_arm.go

// package runtime -- go2cs converted at 2020 October 08 03:19:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs1_netbsd_arm64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EINTR = (ulong)0x4UL;
        private static readonly ulong _EFAULT = (ulong)0xeUL;
        private static readonly ulong _EAGAIN = (ulong)0x23UL;
        private static readonly ulong _ENOSYS = (ulong)0x4eUL;

        private static readonly ulong _O_NONBLOCK = (ulong)0x4UL;
        private static readonly ulong _O_CLOEXEC = (ulong)0x400000UL;

        private static readonly ulong _PROT_NONE = (ulong)0x0UL;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x1000UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x10UL;

        private static readonly ulong _MADV_FREE = (ulong)0x6UL;

        private static readonly ulong _SA_SIGINFO = (ulong)0x40UL;
        private static readonly ulong _SA_RESTART = (ulong)0x2UL;
        private static readonly ulong _SA_ONSTACK = (ulong)0x1UL;

        private static readonly ulong _SIGHUP = (ulong)0x1UL;
        private static readonly ulong _SIGINT = (ulong)0x2UL;
        private static readonly ulong _SIGQUIT = (ulong)0x3UL;
        private static readonly ulong _SIGILL = (ulong)0x4UL;
        private static readonly ulong _SIGTRAP = (ulong)0x5UL;
        private static readonly ulong _SIGABRT = (ulong)0x6UL;
        private static readonly ulong _SIGEMT = (ulong)0x7UL;
        private static readonly ulong _SIGFPE = (ulong)0x8UL;
        private static readonly ulong _SIGKILL = (ulong)0x9UL;
        private static readonly ulong _SIGBUS = (ulong)0xaUL;
        private static readonly ulong _SIGSEGV = (ulong)0xbUL;
        private static readonly ulong _SIGSYS = (ulong)0xcUL;
        private static readonly ulong _SIGPIPE = (ulong)0xdUL;
        private static readonly ulong _SIGALRM = (ulong)0xeUL;
        private static readonly ulong _SIGTERM = (ulong)0xfUL;
        private static readonly ulong _SIGURG = (ulong)0x10UL;
        private static readonly ulong _SIGSTOP = (ulong)0x11UL;
        private static readonly ulong _SIGTSTP = (ulong)0x12UL;
        private static readonly ulong _SIGCONT = (ulong)0x13UL;
        private static readonly ulong _SIGCHLD = (ulong)0x14UL;
        private static readonly ulong _SIGTTIN = (ulong)0x15UL;
        private static readonly ulong _SIGTTOU = (ulong)0x16UL;
        private static readonly ulong _SIGIO = (ulong)0x17UL;
        private static readonly ulong _SIGXCPU = (ulong)0x18UL;
        private static readonly ulong _SIGXFSZ = (ulong)0x19UL;
        private static readonly ulong _SIGVTALRM = (ulong)0x1aUL;
        private static readonly ulong _SIGPROF = (ulong)0x1bUL;
        private static readonly ulong _SIGWINCH = (ulong)0x1cUL;
        private static readonly ulong _SIGINFO = (ulong)0x1dUL;
        private static readonly ulong _SIGUSR1 = (ulong)0x1eUL;
        private static readonly ulong _SIGUSR2 = (ulong)0x1fUL;

        private static readonly ulong _FPE_INTDIV = (ulong)0x1UL;
        private static readonly ulong _FPE_INTOVF = (ulong)0x2UL;
        private static readonly ulong _FPE_FLTDIV = (ulong)0x3UL;
        private static readonly ulong _FPE_FLTOVF = (ulong)0x4UL;
        private static readonly ulong _FPE_FLTUND = (ulong)0x5UL;
        private static readonly ulong _FPE_FLTRES = (ulong)0x6UL;
        private static readonly ulong _FPE_FLTINV = (ulong)0x7UL;
        private static readonly ulong _FPE_FLTSUB = (ulong)0x8UL;

        private static readonly ulong _BUS_ADRALN = (ulong)0x1UL;
        private static readonly ulong _BUS_ADRERR = (ulong)0x2UL;
        private static readonly ulong _BUS_OBJERR = (ulong)0x3UL;

        private static readonly ulong _SEGV_MAPERR = (ulong)0x1UL;
        private static readonly ulong _SEGV_ACCERR = (ulong)0x2UL;

        private static readonly ulong _ITIMER_REAL = (ulong)0x0UL;
        private static readonly ulong _ITIMER_VIRTUAL = (ulong)0x1UL;
        private static readonly ulong _ITIMER_PROF = (ulong)0x2UL;

        private static readonly ulong _EV_ADD = (ulong)0x1UL;
        private static readonly ulong _EV_DELETE = (ulong)0x2UL;
        private static readonly ulong _EV_CLEAR = (ulong)0x20UL;
        private static readonly long _EV_RECEIPT = (long)0L;
        private static readonly ulong _EV_ERROR = (ulong)0x4000UL;
        private static readonly ulong _EV_EOF = (ulong)0x8000UL;
        private static readonly ulong _EVFILT_READ = (ulong)0x0UL;
        private static readonly ulong _EVFILT_WRITE = (ulong)0x1UL;


        private partial struct sigset
        {
            public array<uint> __bits;
        }

        private partial struct siginfo
        {
            public int _signo;
            public int _code;
            public int _errno;
            public System.UIntPtr _reason;
            public array<byte> _reasonx;
        }

        private partial struct stackt
        {
            public System.UIntPtr ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
        }

        private partial struct timespec
        {
            public long tv_sec;
            public long tv_nsec;
        }

        private static void setNsec(this ptr<timespec> _addr_ts, long ns)
        {
            ref timespec ts = ref _addr_ts.val;

            ts.tv_sec = ns / 1e9F;
            ts.tv_nsec = ns % 1e9F;
        }

        private partial struct timeval
        {
            public long tv_sec;
            public int tv_usec;
            public array<byte> _; // EABI
        }

        private static void set_usec(this ptr<timeval> _addr_tv, int x)
        {
            ref timeval tv = ref _addr_tv.val;

            tv.tv_usec = x;
        }

        private partial struct itimerval
        {
            public timeval it_interval;
            public timeval it_value;
        }

        private partial struct mcontextt
        {
            public array<ulong> __gregs;
            public array<byte> __fregs; // _NFREG * 128 + 32 + 32
            public array<ulong> _; // future use
        }

        private partial struct ucontextt
        {
            public uint uc_flags;
            public ptr<ucontextt> uc_link;
            public sigset uc_sigmask;
            public stackt uc_stack;
            public array<byte> _; // EABI
            public mcontextt uc_mcontext;
            public array<int> __uc_pad;
        }

        private partial struct keventt
        {
            public ulong ident;
            public uint filter;
            public uint flags;
            public uint fflags;
            public array<byte> pad_cgo_0;
            public long data;
            public ptr<byte> udata;
        }

        // created by cgo -cdefs and then converted to Go
        // cgo -cdefs defs_netbsd.go defs_netbsd_arm.go

        private static readonly long _REG_X0 = (long)0L;
        private static readonly long _REG_X1 = (long)1L;
        private static readonly long _REG_X2 = (long)2L;
        private static readonly long _REG_X3 = (long)3L;
        private static readonly long _REG_X4 = (long)4L;
        private static readonly long _REG_X5 = (long)5L;
        private static readonly long _REG_X6 = (long)6L;
        private static readonly long _REG_X7 = (long)7L;
        private static readonly long _REG_X8 = (long)8L;
        private static readonly long _REG_X9 = (long)9L;
        private static readonly long _REG_X10 = (long)10L;
        private static readonly long _REG_X11 = (long)11L;
        private static readonly long _REG_X12 = (long)12L;
        private static readonly long _REG_X13 = (long)13L;
        private static readonly long _REG_X14 = (long)14L;
        private static readonly long _REG_X15 = (long)15L;
        private static readonly long _REG_X16 = (long)16L;
        private static readonly long _REG_X17 = (long)17L;
        private static readonly long _REG_X18 = (long)18L;
        private static readonly long _REG_X19 = (long)19L;
        private static readonly long _REG_X20 = (long)20L;
        private static readonly long _REG_X21 = (long)21L;
        private static readonly long _REG_X22 = (long)22L;
        private static readonly long _REG_X23 = (long)23L;
        private static readonly long _REG_X24 = (long)24L;
        private static readonly long _REG_X25 = (long)25L;
        private static readonly long _REG_X26 = (long)26L;
        private static readonly long _REG_X27 = (long)27L;
        private static readonly long _REG_X28 = (long)28L;
        private static readonly long _REG_X29 = (long)29L;
        private static readonly long _REG_X30 = (long)30L;
        private static readonly long _REG_X31 = (long)31L;
        private static readonly long _REG_ELR = (long)32L;
        private static readonly long _REG_SPSR = (long)33L;
        private static readonly long _REG_TPIDR = (long)34L;

    }
}
