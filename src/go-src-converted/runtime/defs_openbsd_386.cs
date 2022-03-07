// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_openbsd.go

// package runtime -- go2cs converted at 2022 March 06 22:08:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_openbsd_386.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EAGAIN = 0x23;
private static readonly nuint _ENOSYS = 0x4e;

private static readonly nuint _O_NONBLOCK = 0x4;
private static readonly nuint _O_CLOEXEC = 0x10000;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x1000;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;
private static readonly nuint _MAP_STACK = 0x4000;

private static readonly nuint _MADV_FREE = 0x6;

private static readonly nuint _SA_SIGINFO = 0x40;
private static readonly nuint _SA_RESTART = 0x2;
private static readonly nuint _SA_ONSTACK = 0x1;

private static readonly nuint _PTHREAD_CREATE_DETACHED = 0x1;

private static readonly nuint _F_SETFD = 0x2;
private static readonly nuint _F_GETFL = 0x3;
private static readonly nuint _F_SETFL = 0x4;
private static readonly nuint _FD_CLOEXEC = 0x1;

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
private static readonly nuint _EV_ERROR = 0x4000;
private static readonly nuint _EV_EOF = 0x8000;
private static readonly nuint _EVFILT_READ = -0x1;
private static readonly nuint _EVFILT_WRITE = -0x2;


private partial struct tforkt {
    public unsafe.Pointer tf_tcb;
    public ptr<int> tf_tid;
    public System.UIntPtr tf_stack;
}

private partial struct sigcontext {
    public uint sc_gs;
    public uint sc_fs;
    public uint sc_es;
    public uint sc_ds;
    public uint sc_edi;
    public uint sc_esi;
    public uint sc_ebp;
    public uint sc_ebx;
    public uint sc_edx;
    public uint sc_ecx;
    public uint sc_eax;
    public uint sc_eip;
    public uint sc_cs;
    public uint sc_eflags;
    public uint sc_esp;
    public uint sc_ss;
    public uint __sc_unused;
    public uint sc_mask;
    public uint sc_trapno;
    public uint sc_err;
    public unsafe.Pointer sc_fpstate;
}

private partial struct siginfo {
    public int si_signo;
    public int si_code;
    public int si_errno;
    public array<byte> _data;
}

private partial struct stackt {
    public System.UIntPtr ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
}

private partial struct timespec {
    public long tv_sec;
    public int tv_nsec;
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = int64(timediv(ns, 1e9F, _addr_ts.tv_nsec));
}

private partial struct timeval {
    public long tv_sec;
    public int tv_usec;
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = x;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct keventt {
    public uint ident;
    public short filter;
    public ushort flags;
    public uint fflags;
    public long data;
    public ptr<byte> udata;
}

private partial struct pthread { // : System.UIntPtr
}
private partial struct pthreadattr { // : System.UIntPtr
}
private partial struct pthreadcond { // : System.UIntPtr
}
private partial struct pthreadcondattr { // : System.UIntPtr
}
private partial struct pthreadmutex { // : System.UIntPtr
}
private partial struct pthreadmutexattr { // : System.UIntPtr
}

} // end runtime_package
