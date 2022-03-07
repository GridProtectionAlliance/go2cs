// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_solaris.go defs_solaris_amd64.go

// package runtime -- go2cs converted at 2022 March 06 22:08:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs1_solaris_amd64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EBADF = 0x9;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EAGAIN = 0xb;
private static readonly nuint _EBUSY = 0x10;
private static readonly nuint _ETIME = 0x3e;
private static readonly nuint _ETIMEDOUT = 0x91;
private static readonly nuint _EWOULDBLOCK = 0xb;
private static readonly nuint _EINPROGRESS = 0x96;
private static readonly nuint _ENOSYS = 0x59;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x100;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_FREE = 0x5;

private static readonly nuint _SA_SIGINFO = 0x8;
private static readonly nuint _SA_RESTART = 0x4;
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
private static readonly nuint _SIGURG = 0x15;
private static readonly nuint _SIGSTOP = 0x17;
private static readonly nuint _SIGTSTP = 0x18;
private static readonly nuint _SIGCONT = 0x19;
private static readonly nuint _SIGCHLD = 0x12;
private static readonly nuint _SIGTTIN = 0x1a;
private static readonly nuint _SIGTTOU = 0x1b;
private static readonly nuint _SIGIO = 0x16;
private static readonly nuint _SIGXCPU = 0x1e;
private static readonly nuint _SIGXFSZ = 0x1f;
private static readonly nuint _SIGVTALRM = 0x1c;
private static readonly nuint _SIGPROF = 0x1d;
private static readonly nuint _SIGWINCH = 0x14;
private static readonly nuint _SIGUSR1 = 0x10;
private static readonly nuint _SIGUSR2 = 0x11;

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

private static readonly nuint __SC_PAGESIZE = 0xb;
private static readonly nuint __SC_NPROCESSORS_ONLN = 0xf;

private static readonly nuint _PTHREAD_CREATE_DETACHED = 0x40;

private static readonly nuint _FORK_NOSIGCHLD = 0x1;
private static readonly nuint _FORK_WAITPID = 0x2;

private static readonly nuint _MAXHOSTNAMELEN = 0x100;

private static readonly nuint _O_NONBLOCK = 0x80;
private static readonly nuint _O_CLOEXEC = 0x800000;
private static readonly nuint _FD_CLOEXEC = 0x1;
private static readonly nuint _F_GETFL = 0x3;
private static readonly nuint _F_SETFL = 0x4;
private static readonly nuint _F_SETFD = 0x2;

private static readonly nuint _POLLIN = 0x1;
private static readonly nuint _POLLOUT = 0x4;
private static readonly nuint _POLLHUP = 0x10;
private static readonly nuint _POLLERR = 0x8;

private static readonly nuint _PORT_SOURCE_FD = 0x4;
private static readonly nuint _PORT_SOURCE_ALERT = 0x5;
private static readonly nuint _PORT_ALERT_UPDATE = 0x2;


private partial struct semt {
    public uint sem_count;
    public ushort sem_type;
    public ushort sem_magic;
    public array<ulong> sem_pad1;
    public array<ulong> sem_pad2;
}

private partial struct sigset {
    public array<uint> __sigbits;
}

private partial struct stackt {
    public ptr<byte> ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
    public array<byte> pad_cgo_0;
}

private partial struct siginfo {
    public int si_signo;
    public int si_code;
    public int si_errno;
    public int si_pad;
    public array<byte> __data;
}

private partial struct sigactiont {
    public int sa_flags;
    public array<byte> pad_cgo_0;
    public array<byte> _funcptr;
    public sigset sa_mask;
}

private partial struct fpregset {
    public array<byte> fp_reg_set;
}

private partial struct mcontext {
    public array<long> gregs;
    public fpregset fpregs;
}

private partial struct ucontext {
    public ulong uc_flags;
    public ptr<ucontext> uc_link;
    public sigset uc_sigmask;
    public stackt uc_stack;
    public array<byte> pad_cgo_0;
    public mcontext uc_mcontext;
    public array<long> uc_filler;
    public array<byte> pad_cgo_1;
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

private partial struct portevent {
    public int portev_events;
    public ushort portev_source;
    public ushort portev_pad;
    public ulong portev_object;
    public ptr<byte> portev_user;
}

private partial struct pthread { // : uint
}
private partial struct pthreadattr {
    public ptr<byte> __pthread_attrp;
}

private partial struct stat {
    public ulong st_dev;
    public ulong st_ino;
    public uint st_mode;
    public uint st_nlink;
    public uint st_uid;
    public uint st_gid;
    public ulong st_rdev;
    public long st_size;
    public timespec st_atim;
    public timespec st_mtim;
    public timespec st_ctim;
    public int st_blksize;
    public array<byte> pad_cgo_0;
    public long st_blocks;
    public array<sbyte> st_fstype;
}

// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_solaris.go defs_solaris_amd64.go

private static readonly nuint _REG_RDI = 0x8;
private static readonly nuint _REG_RSI = 0x9;
private static readonly nuint _REG_RDX = 0xc;
private static readonly nuint _REG_RCX = 0xd;
private static readonly nuint _REG_R8 = 0x7;
private static readonly nuint _REG_R9 = 0x6;
private static readonly nuint _REG_R10 = 0x5;
private static readonly nuint _REG_R11 = 0x4;
private static readonly nuint _REG_R12 = 0x3;
private static readonly nuint _REG_R13 = 0x2;
private static readonly nuint _REG_R14 = 0x1;
private static readonly nuint _REG_R15 = 0x0;
private static readonly nuint _REG_RBP = 0xa;
private static readonly nuint _REG_RBX = 0xb;
private static readonly nuint _REG_RAX = 0xe;
private static readonly nuint _REG_GS = 0x17;
private static readonly nuint _REG_FS = 0x16;
private static readonly nuint _REG_ES = 0x18;
private static readonly nuint _REG_DS = 0x19;
private static readonly nuint _REG_TRAPNO = 0xf;
private static readonly nuint _REG_ERR = 0x10;
private static readonly nuint _REG_RIP = 0x11;
private static readonly nuint _REG_CS = 0x12;
private static readonly nuint _REG_RFLAGS = 0x13;
private static readonly nuint _REG_RSP = 0x14;
private static readonly nuint _REG_SS = 0x15;


} // end runtime_package
