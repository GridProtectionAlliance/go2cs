// package runtime -- go2cs converted at 2020 October 08 03:19:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_linux_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Constants
        private static readonly ulong _EINTR = (ulong)0x4UL;
        private static readonly ulong _ENOMEM = (ulong)0xcUL;
        private static readonly ulong _EAGAIN = (ulong)0xbUL;
        private static readonly ulong _ENOSYS = (ulong)0x26UL;

        private static readonly long _PROT_NONE = (long)0L;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x20UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x10UL;

        private static readonly ulong _MADV_DONTNEED = (ulong)0x4UL;
        private static readonly ulong _MADV_FREE = (ulong)0x8UL;
        private static readonly ulong _MADV_HUGEPAGE = (ulong)0xeUL;
        private static readonly ulong _MADV_NOHUGEPAGE = (ulong)0xfUL;

        private static readonly ulong _SA_RESTART = (ulong)0x10000000UL;
        private static readonly ulong _SA_ONSTACK = (ulong)0x8000000UL;
        private static readonly long _SA_RESTORER = (long)0L; // unused on ARM
        private static readonly ulong _SA_SIGINFO = (ulong)0x4UL;
        private static readonly ulong _SIGHUP = (ulong)0x1UL;
        private static readonly ulong _SIGINT = (ulong)0x2UL;
        private static readonly ulong _SIGQUIT = (ulong)0x3UL;
        private static readonly ulong _SIGILL = (ulong)0x4UL;
        private static readonly ulong _SIGTRAP = (ulong)0x5UL;
        private static readonly ulong _SIGABRT = (ulong)0x6UL;
        private static readonly ulong _SIGBUS = (ulong)0x7UL;
        private static readonly ulong _SIGFPE = (ulong)0x8UL;
        private static readonly ulong _SIGKILL = (ulong)0x9UL;
        private static readonly ulong _SIGUSR1 = (ulong)0xaUL;
        private static readonly ulong _SIGSEGV = (ulong)0xbUL;
        private static readonly ulong _SIGUSR2 = (ulong)0xcUL;
        private static readonly ulong _SIGPIPE = (ulong)0xdUL;
        private static readonly ulong _SIGALRM = (ulong)0xeUL;
        private static readonly ulong _SIGSTKFLT = (ulong)0x10UL;
        private static readonly ulong _SIGCHLD = (ulong)0x11UL;
        private static readonly ulong _SIGCONT = (ulong)0x12UL;
        private static readonly ulong _SIGSTOP = (ulong)0x13UL;
        private static readonly ulong _SIGTSTP = (ulong)0x14UL;
        private static readonly ulong _SIGTTIN = (ulong)0x15UL;
        private static readonly ulong _SIGTTOU = (ulong)0x16UL;
        private static readonly ulong _SIGURG = (ulong)0x17UL;
        private static readonly ulong _SIGXCPU = (ulong)0x18UL;
        private static readonly ulong _SIGXFSZ = (ulong)0x19UL;
        private static readonly ulong _SIGVTALRM = (ulong)0x1aUL;
        private static readonly ulong _SIGPROF = (ulong)0x1bUL;
        private static readonly ulong _SIGWINCH = (ulong)0x1cUL;
        private static readonly ulong _SIGIO = (ulong)0x1dUL;
        private static readonly ulong _SIGPWR = (ulong)0x1eUL;
        private static readonly ulong _SIGSYS = (ulong)0x1fUL;
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
        private static readonly long _ITIMER_REAL = (long)0L;
        private static readonly ulong _ITIMER_PROF = (ulong)0x2UL;
        private static readonly ulong _ITIMER_VIRTUAL = (ulong)0x1UL;
        private static readonly long _O_RDONLY = (long)0L;
        private static readonly ulong _O_NONBLOCK = (ulong)0x800UL;
        private static readonly ulong _O_CLOEXEC = (ulong)0x80000UL;

        private static readonly ulong _EPOLLIN = (ulong)0x1UL;
        private static readonly ulong _EPOLLOUT = (ulong)0x4UL;
        private static readonly ulong _EPOLLERR = (ulong)0x8UL;
        private static readonly ulong _EPOLLHUP = (ulong)0x10UL;
        private static readonly ulong _EPOLLRDHUP = (ulong)0x2000UL;
        private static readonly ulong _EPOLLET = (ulong)0x80000000UL;
        private static readonly ulong _EPOLL_CLOEXEC = (ulong)0x80000UL;
        private static readonly ulong _EPOLL_CTL_ADD = (ulong)0x1UL;
        private static readonly ulong _EPOLL_CTL_DEL = (ulong)0x2UL;
        private static readonly ulong _EPOLL_CTL_MOD = (ulong)0x3UL;

        private static readonly ulong _AF_UNIX = (ulong)0x1UL;
        private static readonly ulong _SOCK_DGRAM = (ulong)0x2UL;


        private partial struct timespec
        {
            public int tv_sec;
            public int tv_nsec;
        }

        //go:nosplit
        private static void setNsec(this ptr<timespec> _addr_ts, long ns)
        {
            ref timespec ts = ref _addr_ts.val;

            ts.tv_sec = timediv(ns, 1e9F, _addr_ts.tv_nsec);
        }

        private partial struct stackt
        {
            public ptr<byte> ss_sp;
            public int ss_flags;
            public System.UIntPtr ss_size;
        }

        private partial struct sigcontext
        {
            public uint trap_no;
            public uint error_code;
            public uint oldmask;
            public uint r0;
            public uint r1;
            public uint r2;
            public uint r3;
            public uint r4;
            public uint r5;
            public uint r6;
            public uint r7;
            public uint r8;
            public uint r9;
            public uint r10;
            public uint fp;
            public uint ip;
            public uint sp;
            public uint lr;
            public uint pc;
            public uint cpsr;
            public uint fault_address;
        }

        private partial struct ucontext
        {
            public uint uc_flags;
            public ptr<ucontext> uc_link;
            public stackt uc_stack;
            public sigcontext uc_mcontext;
            public uint uc_sigmask;
            public array<int> __unused;
            public array<uint> uc_regspace;
        }

        private partial struct timeval
        {
            public int tv_sec;
            public int tv_usec;
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

        private partial struct siginfo
        {
            public int si_signo;
            public int si_errno;
            public int si_code; // below here is a union; si_addr is the only field we use
            public uint si_addr;
        }

        private partial struct sigactiont
        {
            public System.UIntPtr sa_handler;
            public uint sa_flags;
            public System.UIntPtr sa_restorer;
            public ulong sa_mask;
        }

        private partial struct epollevent
        {
            public uint events;
            public uint _pad;
            public array<byte> data; // to match amd64
        }

        private partial struct sockaddr_un
        {
            public ushort family;
            public array<byte> path;
        }
    }
}
