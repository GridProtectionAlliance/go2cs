// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_netbsd.go defs_netbsd_amd64.go

// package runtime -- go2cs converted at 2020 August 29 08:16:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs1_netbsd_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EINTR = 0x4UL;
        private static readonly ulong _EFAULT = 0xeUL;

        private static readonly ulong _PROT_NONE = 0x0UL;
        private static readonly ulong _PROT_READ = 0x1UL;
        private static readonly ulong _PROT_WRITE = 0x2UL;
        private static readonly ulong _PROT_EXEC = 0x4UL;

        private static readonly ulong _MAP_ANON = 0x1000UL;
        private static readonly ulong _MAP_PRIVATE = 0x2UL;
        private static readonly ulong _MAP_FIXED = 0x10UL;

        private static readonly ulong _MADV_FREE = 0x6UL;

        private static readonly ulong _SA_SIGINFO = 0x40UL;
        private static readonly ulong _SA_RESTART = 0x2UL;
        private static readonly ulong _SA_ONSTACK = 0x1UL;

        private static readonly ulong _SIGHUP = 0x1UL;
        private static readonly ulong _SIGINT = 0x2UL;
        private static readonly ulong _SIGQUIT = 0x3UL;
        private static readonly ulong _SIGILL = 0x4UL;
        private static readonly ulong _SIGTRAP = 0x5UL;
        private static readonly ulong _SIGABRT = 0x6UL;
        private static readonly ulong _SIGEMT = 0x7UL;
        private static readonly ulong _SIGFPE = 0x8UL;
        private static readonly ulong _SIGKILL = 0x9UL;
        private static readonly ulong _SIGBUS = 0xaUL;
        private static readonly ulong _SIGSEGV = 0xbUL;
        private static readonly ulong _SIGSYS = 0xcUL;
        private static readonly ulong _SIGPIPE = 0xdUL;
        private static readonly ulong _SIGALRM = 0xeUL;
        private static readonly ulong _SIGTERM = 0xfUL;
        private static readonly ulong _SIGURG = 0x10UL;
        private static readonly ulong _SIGSTOP = 0x11UL;
        private static readonly ulong _SIGTSTP = 0x12UL;
        private static readonly ulong _SIGCONT = 0x13UL;
        private static readonly ulong _SIGCHLD = 0x14UL;
        private static readonly ulong _SIGTTIN = 0x15UL;
        private static readonly ulong _SIGTTOU = 0x16UL;
        private static readonly ulong _SIGIO = 0x17UL;
        private static readonly ulong _SIGXCPU = 0x18UL;
        private static readonly ulong _SIGXFSZ = 0x19UL;
        private static readonly ulong _SIGVTALRM = 0x1aUL;
        private static readonly ulong _SIGPROF = 0x1bUL;
        private static readonly ulong _SIGWINCH = 0x1cUL;
        private static readonly ulong _SIGINFO = 0x1dUL;
        private static readonly ulong _SIGUSR1 = 0x1eUL;
        private static readonly ulong _SIGUSR2 = 0x1fUL;

        private static readonly ulong _FPE_INTDIV = 0x1UL;
        private static readonly ulong _FPE_INTOVF = 0x2UL;
        private static readonly ulong _FPE_FLTDIV = 0x3UL;
        private static readonly ulong _FPE_FLTOVF = 0x4UL;
        private static readonly ulong _FPE_FLTUND = 0x5UL;
        private static readonly ulong _FPE_FLTRES = 0x6UL;
        private static readonly ulong _FPE_FLTINV = 0x7UL;
        private static readonly ulong _FPE_FLTSUB = 0x8UL;

        private static readonly ulong _BUS_ADRALN = 0x1UL;
        private static readonly ulong _BUS_ADRERR = 0x2UL;
        private static readonly ulong _BUS_OBJERR = 0x3UL;

        private static readonly ulong _SEGV_MAPERR = 0x1UL;
        private static readonly ulong _SEGV_ACCERR = 0x2UL;

        private static readonly ulong _ITIMER_REAL = 0x0UL;
        private static readonly ulong _ITIMER_VIRTUAL = 0x1UL;
        private static readonly ulong _ITIMER_PROF = 0x2UL;

        private static readonly ulong _EV_ADD = 0x1UL;
        private static readonly ulong _EV_DELETE = 0x2UL;
        private static readonly ulong _EV_CLEAR = 0x20UL;
        private static readonly long _EV_RECEIPT = 0L;
        private static readonly ulong _EV_ERROR = 0x4000UL;
        private static readonly ulong _EV_EOF = 0x8000UL;
        private static readonly ulong _EVFILT_READ = 0x0UL;
        private static readonly ulong _EVFILT_WRITE = 0x1UL;

        private partial struct sigset
        {
            public array<uint> __bits;
        }

        private partial struct siginfo
        {
            public int _signo;
            public int _code;
            public int _errno;
            public int _pad;
            public array<byte> _reason;
        }

        private partial struct stackt
        {
            public System.UIntPtr ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
            public array<byte> pad_cgo_0;
        }

        private partial struct timespec
        {
            public long tv_sec;
            public long tv_nsec;
        }

        private static void set_sec(this ref timespec ts, int x)
        {
            ts.tv_sec = int64(x);
        }

        private static void set_nsec(this ref timespec ts, int x)
        {
            ts.tv_nsec = int64(x);
        }

        private partial struct timeval
        {
            public long tv_sec;
            public int tv_usec;
            public array<byte> pad_cgo_0;
        }

        private static void set_usec(this ref timeval tv, int x)
        {
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
            public ulong _mc_tlsbase;
            public array<sbyte> __fpregs;
        }

        private partial struct ucontextt
        {
            public uint uc_flags;
            public array<byte> pad_cgo_0;
            public ptr<ucontextt> uc_link;
            public sigset uc_sigmask;
            public stackt uc_stack;
            public mcontextt uc_mcontext;
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
        // cgo -cdefs defs_netbsd.go defs_netbsd_amd64.go

        private static readonly ulong _REG_RDI = 0x0UL;
        private static readonly ulong _REG_RSI = 0x1UL;
        private static readonly ulong _REG_RDX = 0x2UL;
        private static readonly ulong _REG_RCX = 0x3UL;
        private static readonly ulong _REG_R8 = 0x4UL;
        private static readonly ulong _REG_R9 = 0x5UL;
        private static readonly ulong _REG_R10 = 0x6UL;
        private static readonly ulong _REG_R11 = 0x7UL;
        private static readonly ulong _REG_R12 = 0x8UL;
        private static readonly ulong _REG_R13 = 0x9UL;
        private static readonly ulong _REG_R14 = 0xaUL;
        private static readonly ulong _REG_R15 = 0xbUL;
        private static readonly ulong _REG_RBP = 0xcUL;
        private static readonly ulong _REG_RBX = 0xdUL;
        private static readonly ulong _REG_RAX = 0xeUL;
        private static readonly ulong _REG_GS = 0xfUL;
        private static readonly ulong _REG_FS = 0x10UL;
        private static readonly ulong _REG_ES = 0x11UL;
        private static readonly ulong _REG_DS = 0x12UL;
        private static readonly ulong _REG_TRAPNO = 0x13UL;
        private static readonly ulong _REG_ERR = 0x14UL;
        private static readonly ulong _REG_RIP = 0x15UL;
        private static readonly ulong _REG_CS = 0x16UL;
        private static readonly ulong _REG_RFLAGS = 0x17UL;
        private static readonly ulong _REG_RSP = 0x18UL;
        private static readonly ulong _REG_SS = 0x19UL;
    }
}
