// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_darwin.go

// package runtime -- go2cs converted at 2020 October 09 04:45:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_darwin_amd64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EINTR = (ulong)0x4UL;
        private static readonly ulong _EFAULT = (ulong)0xeUL;
        private static readonly ulong _EAGAIN = (ulong)0x23UL;
        private static readonly ulong _ETIMEDOUT = (ulong)0x3cUL;

        private static readonly ulong _PROT_NONE = (ulong)0x0UL;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x1000UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x10UL;

        private static readonly ulong _MADV_DONTNEED = (ulong)0x4UL;
        private static readonly ulong _MADV_FREE = (ulong)0x5UL;
        private static readonly ulong _MADV_FREE_REUSABLE = (ulong)0x7UL;
        private static readonly ulong _MADV_FREE_REUSE = (ulong)0x8UL;

        private static readonly ulong _SA_SIGINFO = (ulong)0x40UL;
        private static readonly ulong _SA_RESTART = (ulong)0x2UL;
        private static readonly ulong _SA_ONSTACK = (ulong)0x1UL;
        private static readonly ulong _SA_USERTRAMP = (ulong)0x100UL;
        private static readonly ulong _SA_64REGSET = (ulong)0x200UL;

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

        private static readonly ulong _FPE_INTDIV = (ulong)0x7UL;
        private static readonly ulong _FPE_INTOVF = (ulong)0x8UL;
        private static readonly ulong _FPE_FLTDIV = (ulong)0x1UL;
        private static readonly ulong _FPE_FLTOVF = (ulong)0x2UL;
        private static readonly ulong _FPE_FLTUND = (ulong)0x3UL;
        private static readonly ulong _FPE_FLTRES = (ulong)0x4UL;
        private static readonly ulong _FPE_FLTINV = (ulong)0x5UL;
        private static readonly ulong _FPE_FLTSUB = (ulong)0x6UL;

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

        private static readonly ulong _PTHREAD_CREATE_DETACHED = (ulong)0x2UL;

        private static readonly ulong _F_SETFD = (ulong)0x2UL;
        private static readonly ulong _F_GETFL = (ulong)0x3UL;
        private static readonly ulong _F_SETFL = (ulong)0x4UL;
        private static readonly ulong _FD_CLOEXEC = (ulong)0x1UL;

        private static readonly long _O_NONBLOCK = (long)4L;


        private partial struct stackt
        {
            public ptr<byte> ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
            public array<byte> pad_cgo_0;
        }

        private partial struct sigactiont
        {
            public array<byte> __sigaction_u;
            public unsafe.Pointer sa_tramp;
            public uint sa_mask;
            public int sa_flags;
        }

        private partial struct usigactiont
        {
            public array<byte> __sigaction_u;
            public uint sa_mask;
            public int sa_flags;
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
            public long si_band;
            public array<ulong> __pad;
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

        private partial struct fpcontrol
        {
            public array<byte> pad_cgo_0;
        }

        private partial struct fpstatus
        {
            public array<byte> pad_cgo_0;
        }

        private partial struct regmmst
        {
            public array<sbyte> mmst_reg;
            public array<sbyte> mmst_rsrv;
        }

        private partial struct regxmm
        {
            public array<sbyte> xmm_reg;
        }

        private partial struct regs64
        {
            public ulong rax;
            public ulong rbx;
            public ulong rcx;
            public ulong rdx;
            public ulong rdi;
            public ulong rsi;
            public ulong rbp;
            public ulong rsp;
            public ulong r8;
            public ulong r9;
            public ulong r10;
            public ulong r11;
            public ulong r12;
            public ulong r13;
            public ulong r14;
            public ulong r15;
            public ulong rip;
            public ulong rflags;
            public ulong cs;
            public ulong fs;
            public ulong gs;
        }

        private partial struct floatstate64
        {
            public array<int> fpu_reserved;
            public fpcontrol fpu_fcw;
            public fpstatus fpu_fsw;
            public byte fpu_ftw;
            public byte fpu_rsrv1;
            public ushort fpu_fop;
            public uint fpu_ip;
            public ushort fpu_cs;
            public ushort fpu_rsrv2;
            public uint fpu_dp;
            public ushort fpu_ds;
            public ushort fpu_rsrv3;
            public uint fpu_mxcsr;
            public uint fpu_mxcsrmask;
            public regmmst fpu_stmm0;
            public regmmst fpu_stmm1;
            public regmmst fpu_stmm2;
            public regmmst fpu_stmm3;
            public regmmst fpu_stmm4;
            public regmmst fpu_stmm5;
            public regmmst fpu_stmm6;
            public regmmst fpu_stmm7;
            public regxmm fpu_xmm0;
            public regxmm fpu_xmm1;
            public regxmm fpu_xmm2;
            public regxmm fpu_xmm3;
            public regxmm fpu_xmm4;
            public regxmm fpu_xmm5;
            public regxmm fpu_xmm6;
            public regxmm fpu_xmm7;
            public regxmm fpu_xmm8;
            public regxmm fpu_xmm9;
            public regxmm fpu_xmm10;
            public regxmm fpu_xmm11;
            public regxmm fpu_xmm12;
            public regxmm fpu_xmm13;
            public regxmm fpu_xmm14;
            public regxmm fpu_xmm15;
            public array<sbyte> fpu_rsrv4;
            public int fpu_reserved1;
        }

        private partial struct exceptionstate64
        {
            public ushort trapno;
            public ushort cpu;
            public uint err;
            public ulong faultvaddr;
        }

        private partial struct mcontext64
        {
            public exceptionstate64 es;
            public regs64 ss;
            public floatstate64 fs;
            public array<byte> pad_cgo_0;
        }

        private partial struct regs32
        {
            public uint eax;
            public uint ebx;
            public uint ecx;
            public uint edx;
            public uint edi;
            public uint esi;
            public uint ebp;
            public uint esp;
            public uint ss;
            public uint eflags;
            public uint eip;
            public uint cs;
            public uint ds;
            public uint es;
            public uint fs;
            public uint gs;
        }

        private partial struct floatstate32
        {
            public array<int> fpu_reserved;
            public fpcontrol fpu_fcw;
            public fpstatus fpu_fsw;
            public byte fpu_ftw;
            public byte fpu_rsrv1;
            public ushort fpu_fop;
            public uint fpu_ip;
            public ushort fpu_cs;
            public ushort fpu_rsrv2;
            public uint fpu_dp;
            public ushort fpu_ds;
            public ushort fpu_rsrv3;
            public uint fpu_mxcsr;
            public uint fpu_mxcsrmask;
            public regmmst fpu_stmm0;
            public regmmst fpu_stmm1;
            public regmmst fpu_stmm2;
            public regmmst fpu_stmm3;
            public regmmst fpu_stmm4;
            public regmmst fpu_stmm5;
            public regmmst fpu_stmm6;
            public regmmst fpu_stmm7;
            public regxmm fpu_xmm0;
            public regxmm fpu_xmm1;
            public regxmm fpu_xmm2;
            public regxmm fpu_xmm3;
            public regxmm fpu_xmm4;
            public regxmm fpu_xmm5;
            public regxmm fpu_xmm6;
            public regxmm fpu_xmm7;
            public array<sbyte> fpu_rsrv4;
            public int fpu_reserved1;
        }

        private partial struct exceptionstate32
        {
            public ushort trapno;
            public ushort cpu;
            public uint err;
            public uint faultvaddr;
        }

        private partial struct mcontext32
        {
            public exceptionstate32 es;
            public regs32 ss;
            public floatstate32 fs;
        }

        private partial struct ucontext
        {
            public int uc_onstack;
            public uint uc_sigmask;
            public stackt uc_stack;
            public ptr<ucontext> uc_link;
            public ulong uc_mcsize;
            public ptr<mcontext64> uc_mcontext;
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

        private partial struct pthread // : System.UIntPtr
        {
        }
        private partial struct pthreadattr
        {
            public long X__sig;
            public array<sbyte> X__opaque;
        }
        private partial struct pthreadmutex
        {
            public long X__sig;
            public array<sbyte> X__opaque;
        }
        private partial struct pthreadmutexattr
        {
            public long X__sig;
            public array<sbyte> X__opaque;
        }
        private partial struct pthreadcond
        {
            public long X__sig;
            public array<sbyte> X__opaque;
        }
        private partial struct pthreadcondattr
        {
            public long X__sig;
            public array<sbyte> X__opaque;
        }

        private partial struct machTimebaseInfo
        {
            public uint numer;
            public uint denom;
        }
    }
}
