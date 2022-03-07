// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_freebsd.go

// package runtime -- go2cs converted at 2022 March 06 22:08:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_freebsd_arm.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint _NBBY = 0x8;
private static readonly nuint _CTL_MAXNAME = 0x18;
private static readonly nuint _CPU_LEVEL_WHICH = 0x3;
private static readonly nuint _CPU_WHICH_PID = 0x2;


private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EAGAIN = 0x23;
private static readonly nuint _ENOSYS = 0x4e;
private static readonly nuint _ETIMEDOUT = 0x3c;

private static readonly nuint _O_NONBLOCK = 0x4;
private static readonly nuint _O_CLOEXEC = 0x100000;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x1000;
private static readonly nuint _MAP_SHARED = 0x1;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_FREE = 0x5;

private static readonly nuint _SA_SIGINFO = 0x40;
private static readonly nuint _SA_RESTART = 0x2;
private static readonly nuint _SA_ONSTACK = 0x1;

private static readonly nuint _CLOCK_MONOTONIC = 0x4;
private static readonly nuint _CLOCK_REALTIME = 0x0;

private static readonly nuint _UMTX_OP_WAIT_UINT = 0xb;
private static readonly nuint _UMTX_OP_WAIT_UINT_PRIVATE = 0xf;
private static readonly nuint _UMTX_OP_WAKE = 0x3;
private static readonly nuint _UMTX_OP_WAKE_PRIVATE = 0x10;

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
private static readonly nuint _EV_RECEIPT = 0x40;
private static readonly nuint _EV_ERROR = 0x4000;
private static readonly nuint _EV_EOF = 0x8000;
private static readonly nuint _EVFILT_READ = -0x1;
private static readonly nuint _EVFILT_WRITE = -0x2;


private partial struct rtprio {
    public ushort _type;
    public ushort prio;
}

private partial struct thrparam {
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

private partial struct thread { // : int
} // long

private partial struct sigset {
    public array<uint> __bits;
}

private partial struct stackt {
    public System.UIntPtr ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
}

private partial struct siginfo {
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

private partial struct mcontext {
    public array<uint> __gregs;
    public array<byte> __fpu;
}

private partial struct ucontext {
    public sigset uc_sigmask;
    public mcontext uc_mcontext;
    public ptr<ucontext> uc_link;
    public stackt uc_stack;
    public int uc_flags;
    public array<int> __spare__;
}

private partial struct timespec {
    public long tv_sec;
    public int tv_nsec;
    public array<byte> pad_cgo_0;
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = int64(timediv(ns, 1e9F, _addr_ts.tv_nsec));
}

private partial struct timeval {
    public long tv_sec;
    public int tv_usec;
    public array<byte> pad_cgo_0;
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = x;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct umtx_time {
    public timespec _timeout;
    public uint _flags;
    public uint _clockid;
}

private partial struct keventt {
    public uint ident;
    public short filter;
    public ushort flags;
    public uint fflags;
    public int data;
    public ptr<byte> udata;
}

private partial struct bintime {
    public long sec;
    public ulong frac;
}

private partial struct vdsoTimehands {
    public uint algo;
    public uint gen;
    public ulong scale;
    public uint offset_count;
    public uint counter_mask;
    public bintime offset;
    public bintime boottime;
    public uint physical;
    public array<uint> res;
}

private partial struct vdsoTimekeep {
    public uint ver;
    public uint enabled;
    public uint current;
    public array<byte> pad_cgo_0;
}

private static readonly nuint _VDSO_TK_VER_CURR = 0x1;

private static readonly nuint vdsoTimehandsSize = 0x58;
private static readonly nuint vdsoTimekeepSize = 0x10;


} // end runtime_package
