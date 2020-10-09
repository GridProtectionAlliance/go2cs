// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_openbsd.go

// package runtime -- go2cs converted at 2020 October 09 04:45:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_openbsd_arm.go
using @unsafe = go.@unsafe_package;
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
        private static readonly ulong _O_CLOEXEC = (ulong)0x10000UL;

        private static readonly ulong _PROT_NONE = (ulong)0x0UL;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x1000UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x10UL;
        private static readonly ulong _MAP_STACK = (ulong)0x4000UL;

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
        private static readonly ulong _EV_ERROR = (ulong)0x4000UL;
        private static readonly ulong _EV_EOF = (ulong)0x8000UL;
        private static readonly ulong _EVFILT_READ = (ulong)-0x1UL;
        private static readonly ulong _EVFILT_WRITE = (ulong)-0x2UL;


        private partial struct tforkt
        {
            public unsafe.Pointer tf_tcb;
            public ptr<int> tf_tid;
            public System.UIntPtr tf_stack;
        }

        private partial struct sigcontext
        {
            public int __sc_unused;
            public int sc_mask;
            public uint sc_spsr;
            public uint sc_r0;
            public uint sc_r1;
            public uint sc_r2;
            public uint sc_r3;
            public uint sc_r4;
            public uint sc_r5;
            public uint sc_r6;
            public uint sc_r7;
            public uint sc_r8;
            public uint sc_r9;
            public uint sc_r10;
            public uint sc_r11;
            public uint sc_r12;
            public uint sc_usr_sp;
            public uint sc_usr_lr;
            public uint sc_svc_lr;
            public uint sc_pc;
            public uint sc_fpused;
            public uint sc_fpscr;
            public array<ulong> sc_fpreg;
        }

        private partial struct siginfo
        {
            public int si_signo;
            public int si_code;
            public int si_errno;
            public array<byte> pad_cgo_0;
            public array<byte> _data;
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
            public int tv_nsec;
            public array<byte> pad_cgo_0;
        }

        //go:nosplit
        private static void setNsec(this ptr<timespec> _addr_ts, long ns)
        {
            ref timespec ts = ref _addr_ts.val;

            ts.tv_sec = int64(timediv(ns, 1e9F, _addr_ts.tv_nsec));
        }

        private partial struct timeval
        {
            public long tv_sec;
            public int tv_usec;
            public array<byte> pad_cgo_0;
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

        private partial struct keventt
        {
            public uint ident;
            public short filter;
            public ushort flags;
            public uint fflags;
            public array<byte> pad_cgo_0;
            public long data;
            public ptr<byte> udata;
            public array<byte> pad_cgo_1;
        }
    }
}
