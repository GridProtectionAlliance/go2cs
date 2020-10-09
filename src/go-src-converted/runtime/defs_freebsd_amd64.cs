// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_freebsd.go

// package runtime -- go2cs converted at 2020 October 09 04:45:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_freebsd_amd64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _NBBY = (ulong)0x8UL;
        private static readonly ulong _CTL_MAXNAME = (ulong)0x18UL;
        private static readonly ulong _CPU_LEVEL_WHICH = (ulong)0x3UL;
        private static readonly ulong _CPU_WHICH_PID = (ulong)0x2UL;


        private static readonly ulong _EINTR = (ulong)0x4UL;
        private static readonly ulong _EFAULT = (ulong)0xeUL;
        private static readonly ulong _EAGAIN = (ulong)0x23UL;
        private static readonly ulong _ENOSYS = (ulong)0x4eUL;

        private static readonly ulong _O_NONBLOCK = (ulong)0x4UL;
        private static readonly ulong _O_CLOEXEC = (ulong)0x100000UL;

        private static readonly ulong _PROT_NONE = (ulong)0x0UL;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x1000UL;
        private static readonly ulong _MAP_SHARED = (ulong)0x1UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x10UL;

        private static readonly ulong _MADV_FREE = (ulong)0x5UL;

        private static readonly ulong _SA_SIGINFO = (ulong)0x40UL;
        private static readonly ulong _SA_RESTART = (ulong)0x2UL;
        private static readonly ulong _SA_ONSTACK = (ulong)0x1UL;

        private static readonly ulong _CLOCK_MONOTONIC = (ulong)0x4UL;
        private static readonly ulong _CLOCK_REALTIME = (ulong)0x0UL;

        private static readonly ulong _UMTX_OP_WAIT_UINT = (ulong)0xbUL;
        private static readonly ulong _UMTX_OP_WAIT_UINT_PRIVATE = (ulong)0xfUL;
        private static readonly ulong _UMTX_OP_WAKE = (ulong)0x3UL;
        private static readonly ulong _UMTX_OP_WAKE_PRIVATE = (ulong)0x10UL;

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

        private static readonly ulong _FPE_INTDIV = (ulong)0x2UL;
        private static readonly ulong _FPE_INTOVF = (ulong)0x1UL;
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
        private static readonly ulong _EV_RECEIPT = (ulong)0x40UL;
        private static readonly ulong _EV_ERROR = (ulong)0x4000UL;
        private static readonly ulong _EV_EOF = (ulong)0x8000UL;
        private static readonly ulong _EVFILT_READ = (ulong)-0x1UL;
        private static readonly ulong _EVFILT_WRITE = (ulong)-0x2UL;


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
            public unsafe.Pointer child_tid; // *int64
            public ptr<long> parent_tid;
            public int flags;
            public array<byte> pad_cgo_0;
            public ptr<rtprio> rtp;
            public array<System.UIntPtr> spare;
        }

        private partial struct thread // : long
        {
        } // long

        private partial struct sigset
        {
            public array<uint> __bits;
        }

        private partial struct stackt
        {
            public System.UIntPtr ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
            public array<byte> pad_cgo_0;
        }

        private partial struct siginfo
        {
            public int si_signo;
            public int si_errno;
            public int si_code;
            public int si_pid;
            public uint si_uid;
            public int si_status;
            public ulong si_addr;
            public array<byte> si_value;
            public array<byte> _reason;
        }

        private partial struct mcontext
        {
            public ulong mc_onstack;
            public ulong mc_rdi;
            public ulong mc_rsi;
            public ulong mc_rdx;
            public ulong mc_rcx;
            public ulong mc_r8;
            public ulong mc_r9;
            public ulong mc_rax;
            public ulong mc_rbx;
            public ulong mc_rbp;
            public ulong mc_r10;
            public ulong mc_r11;
            public ulong mc_r12;
            public ulong mc_r13;
            public ulong mc_r14;
            public ulong mc_r15;
            public uint mc_trapno;
            public ushort mc_fs;
            public ushort mc_gs;
            public ulong mc_addr;
            public uint mc_flags;
            public ushort mc_es;
            public ushort mc_ds;
            public ulong mc_err;
            public ulong mc_rip;
            public ulong mc_cs;
            public ulong mc_rflags;
            public ulong mc_rsp;
            public ulong mc_ss;
            public ulong mc_len;
            public ulong mc_fpformat;
            public ulong mc_ownedfp;
            public array<ulong> mc_fpstate;
            public ulong mc_fsbase;
            public ulong mc_gsbase;
            public ulong mc_xfpustate;
            public ulong mc_xfpustate_len;
            public array<ulong> mc_spare;
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
            public long tv_sec;
            public long tv_nsec;
        }

        //go:nosplit
        private static void setNsec(this ptr<timespec> _addr_ts, long ns)
        {
            ref timespec ts = ref _addr_ts.val;

            ts.tv_sec = ns / 1e9F;
            ts.tv_nsec = ns % 1e9F;
        }

        private partial struct timeval
        {
            public long tv_sec;
            public long tv_usec;
        }

        private static void set_usec(this ptr<timeval> _addr_tv, int x)
        {
            ref timeval tv = ref _addr_tv.val;

            tv.tv_usec = int64(x);
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
            public ulong ident;
            public short filter;
            public ushort flags;
            public uint fflags;
            public long data;
            public ptr<byte> udata;
        }

        private partial struct bintime
        {
            public long sec;
            public ulong frac;
        }

        private partial struct vdsoTimehands
        {
            public uint algo;
            public uint gen;
            public ulong scale;
            public uint offset_count;
            public uint counter_mask;
            public bintime offset;
            public bintime boottime;
            public uint x86_shift;
            public uint x86_hpet_idx;
            public array<uint> res;
        }

        private partial struct vdsoTimekeep
        {
            public uint ver;
            public uint enabled;
            public uint current;
            public array<byte> pad_cgo_0;
        }

        private static readonly ulong _VDSO_TK_VER_CURR = (ulong)0x1UL;

        private static readonly ulong vdsoTimehandsSize = (ulong)0x58UL;
        private static readonly ulong vdsoTimekeepSize = (ulong)0x10UL;

    }
}
