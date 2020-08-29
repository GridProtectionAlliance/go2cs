// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_freebsd.go

// package runtime -- go2cs converted at 2020 August 29 08:16:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_freebsd_386.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _NBBY = 0x8UL;
        private static readonly ulong _CTL_MAXNAME = 0x18UL;
        private static readonly ulong _CPU_LEVEL_WHICH = 0x3UL;
        private static readonly ulong _CPU_WHICH_PID = 0x2UL;

        private static readonly ulong _EINTR = 0x4UL;
        private static readonly ulong _EFAULT = 0xeUL;

        private static readonly ulong _PROT_NONE = 0x0UL;
        private static readonly ulong _PROT_READ = 0x1UL;
        private static readonly ulong _PROT_WRITE = 0x2UL;
        private static readonly ulong _PROT_EXEC = 0x4UL;

        private static readonly ulong _MAP_ANON = 0x1000UL;
        private static readonly ulong _MAP_PRIVATE = 0x2UL;
        private static readonly ulong _MAP_FIXED = 0x10UL;

        private static readonly ulong _MADV_FREE = 0x5UL;

        private static readonly ulong _SA_SIGINFO = 0x40UL;
        private static readonly ulong _SA_RESTART = 0x2UL;
        private static readonly ulong _SA_ONSTACK = 0x1UL;

        private static readonly ulong _CLOCK_MONOTONIC = 0x4UL;

        private static readonly ulong _UMTX_OP_WAIT_UINT = 0xbUL;
        private static readonly ulong _UMTX_OP_WAIT_UINT_PRIVATE = 0xfUL;
        private static readonly ulong _UMTX_OP_WAKE = 0x3UL;
        private static readonly ulong _UMTX_OP_WAKE_PRIVATE = 0x10UL;

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

        private static readonly ulong _FPE_INTDIV = 0x2UL;
        private static readonly ulong _FPE_INTOVF = 0x1UL;
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
        private static readonly ulong _EV_RECEIPT = 0x40UL;
        private static readonly ulong _EV_ERROR = 0x4000UL;
        private static readonly ulong _EV_EOF = 0x8000UL;
        private static readonly ulong _EVFILT_READ = -0x1UL;
        private static readonly ulong _EVFILT_WRITE = -0x2UL;

        private partial struct rtprio
        {
            public ushort _type;
            public ushort prio;
        }

        private partial struct thrparam
        {
            public System.UIntPtr start_func;
            public unsafe.Pointer arg;
            public System.UIntPtr stack_base;
            public System.UIntPtr stack_size;
            public unsafe.Pointer tls_base;
            public System.UIntPtr tls_size;
            public unsafe.Pointer child_tid; // *int32
            public ptr<int> parent_tid;
            public int flags;
            public ptr<rtprio> rtp;
            public array<System.UIntPtr> spare;
        }

        private partial struct sigset
        {
            public array<uint> __bits;
        }

        private partial struct stackt
        {
            public System.UIntPtr ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
        }

        private partial struct siginfo
        {
            public int si_signo;
            public int si_errno;
            public int si_code;
            public int si_pid;
            public uint si_uid;
            public int si_status;
            public System.UIntPtr si_addr;
            public array<byte> si_value;
            public array<byte> _reason;
        }

        private partial struct mcontext
        {
            public uint mc_onstack;
            public uint mc_gs;
            public uint mc_fs;
            public uint mc_es;
            public uint mc_ds;
            public uint mc_edi;
            public uint mc_esi;
            public uint mc_ebp;
            public uint mc_isp;
            public uint mc_ebx;
            public uint mc_edx;
            public uint mc_ecx;
            public uint mc_eax;
            public uint mc_trapno;
            public uint mc_err;
            public uint mc_eip;
            public uint mc_cs;
            public uint mc_eflags;
            public uint mc_esp;
            public uint mc_ss;
            public uint mc_len;
            public uint mc_fpformat;
            public uint mc_ownedfp;
            public uint mc_flags;
            public array<uint> mc_fpstate;
            public uint mc_fsbase;
            public uint mc_gsbase;
            public uint mc_xfpustate;
            public uint mc_xfpustate_len;
            public array<uint> mc_spare2;
        }

        private partial struct ucontext
        {
            public sigset uc_sigmask;
            public mcontext uc_mcontext;
            public ptr<ucontext> uc_link;
            public stackt uc_stack;
            public int uc_flags;
            public array<int> __spare__;
            public array<byte> pad_cgo_0;
        }

        private partial struct timespec
        {
            public int tv_sec;
            public int tv_nsec;
        }

        private static void set_sec(this ref timespec ts, long x)
        {
            ts.tv_sec = int32(x);
        }

        private partial struct timeval
        {
            public int tv_sec;
            public int tv_usec;
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

        private partial struct umtx_time
        {
            public timespec _timeout;
            public uint _flags;
            public uint _clockid;
        }

        private partial struct keventt
        {
            public uint ident;
            public short filter;
            public ushort flags;
            public uint fflags;
            public int data;
            public ptr<byte> udata;
        }
    }
}
