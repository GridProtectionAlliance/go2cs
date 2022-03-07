// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_netbsd.go defs_netbsd_arm.go

// package runtime -- go2cs converted at 2022 March 06 22:08:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs1_netbsd_arm.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EAGAIN = 0x23;
private static readonly nuint _ENOSYS = 0x4e;

private static readonly nuint _O_NONBLOCK = 0x4;
private static readonly nuint _O_CLOEXEC = 0x400000;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x1000;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_FREE = 0x6;

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

private static readonly nuint _FPE_INTDIV = 0x1;
private static readonly nuint _FPE_INTOVF = 0x2;
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
private static readonly nint _EV_RECEIPT = 0;
private static readonly nuint _EV_ERROR = 0x4000;
private static readonly nuint _EV_EOF = 0x8000;
private static readonly nuint _EVFILT_READ = 0x0;
private static readonly nuint _EVFILT_WRITE = 0x1;


private partial struct sigset {
    public array<uint> __bits;
}

private partial struct siginfo {
    public int _signo;
    public int _code;
    public int _errno;
    public System.UIntPtr _reason;
    public array<byte> _reasonx;
}

private partial struct stackt {
    public System.UIntPtr ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
}

private partial struct timespec {
    public long tv_sec;
    public int tv_nsec;
    public array<byte> _; // EABI
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = int64(timediv(ns, 1e9F, _addr_ts.tv_nsec));
}

private partial struct timeval {
    public long tv_sec;
    public int tv_usec;
    public array<byte> _; // EABI
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = x;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct mcontextt {
    public array<uint> __gregs;
    public array<byte> _; // EABI
    public array<byte> __fpu; // EABI
    public uint _mc_tlsbase;
    public array<byte> _; // EABI
}

private partial struct ucontextt {
    public uint uc_flags;
    public ptr<ucontextt> uc_link;
    public sigset uc_sigmask;
    public stackt uc_stack;
    public array<byte> _; // EABI
    public mcontextt uc_mcontext;
    public array<int> __uc_pad;
}

private partial struct keventt {
    public uint ident;
    public uint filter;
    public uint flags;
    public uint fflags;
    public long data;
    public ptr<byte> udata;
    public array<byte> _; // EABI
}

// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_netbsd.go defs_netbsd_arm.go

private static readonly nuint _REG_R0 = 0x0;
private static readonly nuint _REG_R1 = 0x1;
private static readonly nuint _REG_R2 = 0x2;
private static readonly nuint _REG_R3 = 0x3;
private static readonly nuint _REG_R4 = 0x4;
private static readonly nuint _REG_R5 = 0x5;
private static readonly nuint _REG_R6 = 0x6;
private static readonly nuint _REG_R7 = 0x7;
private static readonly nuint _REG_R8 = 0x8;
private static readonly nuint _REG_R9 = 0x9;
private static readonly nuint _REG_R10 = 0xa;
private static readonly nuint _REG_R11 = 0xb;
private static readonly nuint _REG_R12 = 0xc;
private static readonly nuint _REG_R13 = 0xd;
private static readonly nuint _REG_R14 = 0xe;
private static readonly nuint _REG_R15 = 0xf;
private static readonly nuint _REG_CPSR = 0x10;


} // end runtime_package
