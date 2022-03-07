// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_dragonfly.go

// package runtime -- go2cs converted at 2022 March 06 22:08:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_dragonfly_amd64.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EBUSY = 0x10;
private static readonly nuint _EAGAIN = 0x23;
private static readonly nuint _ENOSYS = 0x4e;

private static readonly nuint _O_NONBLOCK = 0x4;
private static readonly nuint _O_CLOEXEC = 0x20000;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x1000;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_FREE = 0x5;

private static readonly nuint _SA_SIGINFO = 0x40;
private static readonly nuint _SA_RESTART = 0x2;
private static readonly nuint _SA_ONSTACK = 0x1;

private static readonly nuint _SIGHUP = 0x1;
private static readonly nuint _SIGINT = 0x2;
private static readonly nuint _SIGQUIT = 0x3;
private static readonly nuint _SIGILL = 0x4;
private static readonly nuint _SIGTRAP = 0x5;
private static readonly nuint _SIGABRT = 0x6;
private static readonly nuint _SIGEMT = 0x7;
private static readonly nuint _SIGFPE = 0x8;
private static readonly nuint _SIGKILL = 0x9;
private static readonly nuint _SIGBUS = 0xa;
private static readonly nuint _SIGSEGV = 0xb;
private static readonly nuint _SIGSYS = 0xc;
private static readonly nuint _SIGPIPE = 0xd;
private static readonly nuint _SIGALRM = 0xe;
private static readonly nuint _SIGTERM = 0xf;
private static readonly nuint _SIGURG = 0x10;
private static readonly nuint _SIGSTOP = 0x11;
private static readonly nuint _SIGTSTP = 0x12;
private static readonly nuint _SIGCONT = 0x13;
private static readonly nuint _SIGCHLD = 0x14;
private static readonly nuint _SIGTTIN = 0x15;
private static readonly nuint _SIGTTOU = 0x16;
private static readonly nuint _SIGIO = 0x17;
private static readonly nuint _SIGXCPU = 0x18;
private static readonly nuint _SIGXFSZ = 0x19;
private static readonly nuint _SIGVTALRM = 0x1a;
private static readonly nuint _SIGPROF = 0x1b;
private static readonly nuint _SIGWINCH = 0x1c;
private static readonly nuint _SIGINFO = 0x1d;
private static readonly nuint _SIGUSR1 = 0x1e;
private static readonly nuint _SIGUSR2 = 0x1f;

private static readonly nuint _FPE_INTDIV = 0x2;
private static readonly nuint _FPE_INTOVF = 0x1;
private static readonly nuint _FPE_FLTDIV = 0x3;
private static readonly nuint _FPE_FLTOVF = 0x4;
private static readonly nuint _FPE_FLTUND = 0x5;
private static readonly nuint _FPE_FLTRES = 0x6;
private static readonly nuint _FPE_FLTINV = 0x7;
private static readonly nuint _FPE_FLTSUB = 0x8;

private static readonly nuint _BUS_ADRALN = 0x1;
private static readonly nuint _BUS_ADRERR = 0x2;
private static readonly nuint _BUS_OBJERR = 0x3;

private static readonly nuint _SEGV_MAPERR = 0x1;
private static readonly nuint _SEGV_ACCERR = 0x2;

private static readonly nuint _ITIMER_REAL = 0x0;
private static readonly nuint _ITIMER_VIRTUAL = 0x1;
private static readonly nuint _ITIMER_PROF = 0x2;

private static readonly nuint _EV_ADD = 0x1;
private static readonly nuint _EV_DELETE = 0x2;
private static readonly nuint _EV_CLEAR = 0x20;
private static readonly nuint _EV_ERROR = 0x4000;
private static readonly nuint _EV_EOF = 0x8000;
private static readonly nuint _EVFILT_READ = -0x1;
private static readonly nuint _EVFILT_WRITE = -0x2;


private partial struct rtprio {
    public ushort _type;
    public ushort prio;
}

private partial struct lwpparams {
    public System.UIntPtr start_func;
    public unsafe.Pointer arg;
    public System.UIntPtr stack;
    public unsafe.Pointer tid1; // *int32
    public unsafe.Pointer tid2; // *int32
}

private partial struct sigset {
    public array<uint> __bits;
}

private partial struct stackt {
    public System.UIntPtr ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
    public array<byte> pad_cgo_0;
}

private partial struct siginfo {
    public int si_signo;
    public int si_errno;
    public int si_code;
    public int si_pid;
    public uint si_uid;
    public int si_status;
    public ulong si_addr;
    public array<byte> si_value;
    public long si_band;
    public array<int> __spare__;
    public array<byte> pad_cgo_0;
}

private partial struct mcontext {
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
    public ulong mc_xflags;
    public ulong mc_trapno;
    public ulong mc_addr;
    public ulong mc_flags;
    public ulong mc_err;
    public ulong mc_rip;
    public ulong mc_cs;
    public ulong mc_rflags;
    public ulong mc_rsp;
    public ulong mc_ss;
    public uint mc_len;
    public uint mc_fpformat;
    public uint mc_ownedfp;
    public uint mc_reserved;
    public array<uint> mc_unused;
    public array<int> mc_fpregs;
}

private partial struct ucontext {
    public sigset uc_sigmask;
    public array<byte> pad_cgo_0;
    public mcontext uc_mcontext;
    public ptr<ucontext> uc_link;
    public stackt uc_stack;
    public array<int> __spare__;
}

private partial struct timespec {
    public long tv_sec;
    public long tv_nsec;
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = ns / 1e9F;
    ts.tv_nsec = ns % 1e9F;
}

private partial struct timeval {
    public long tv_sec;
    public long tv_usec;
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = int64(x);
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct keventt {
    public ulong ident;
    public short filter;
    public ushort flags;
    public uint fflags;
    public long data;
    public ptr<byte> udata;
}

} // end runtime_package
