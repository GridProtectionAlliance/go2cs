// created by cgo -cdefs and then converted to Go
// cgo -cdefs defs_solaris.go defs_solaris_amd64.go

// package runtime -- go2cs converted at 2020 August 29 08:16:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs1_solaris_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EINTR = 0x4UL;
        private static readonly ulong _EBADF = 0x9UL;
        private static readonly ulong _EFAULT = 0xeUL;
        private static readonly ulong _EAGAIN = 0xbUL;
        private static readonly ulong _ETIMEDOUT = 0x91UL;
        private static readonly ulong _EWOULDBLOCK = 0xbUL;
        private static readonly ulong _EINPROGRESS = 0x96UL;

        private static readonly ulong _PROT_NONE = 0x0UL;
        private static readonly ulong _PROT_READ = 0x1UL;
        private static readonly ulong _PROT_WRITE = 0x2UL;
        private static readonly ulong _PROT_EXEC = 0x4UL;

        private static readonly ulong _MAP_ANON = 0x100UL;
        private static readonly ulong _MAP_PRIVATE = 0x2UL;
        private static readonly ulong _MAP_FIXED = 0x10UL;

        private static readonly ulong _MADV_FREE = 0x5UL;

        private static readonly ulong _SA_SIGINFO = 0x8UL;
        private static readonly ulong _SA_RESTART = 0x4UL;
        private static readonly ulong _SA_ONSTACK = 0x1UL;

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
        private static readonly ulong _SIGURG = 0x15UL;
        private static readonly ulong _SIGSTOP = 0x17UL;
        private static readonly ulong _SIGTSTP = 0x18UL;
        private static readonly ulong _SIGCONT = 0x19UL;
        private static readonly ulong _SIGCHLD = 0x12UL;
        private static readonly ulong _SIGTTIN = 0x1aUL;
        private static readonly ulong _SIGTTOU = 0x1bUL;
        private static readonly ulong _SIGIO = 0x16UL;
        private static readonly ulong _SIGXCPU = 0x1eUL;
        private static readonly ulong _SIGXFSZ = 0x1fUL;
        private static readonly ulong _SIGVTALRM = 0x1cUL;
        private static readonly ulong _SIGPROF = 0x1dUL;
        private static readonly ulong _SIGWINCH = 0x14UL;
        private static readonly ulong _SIGUSR1 = 0x10UL;
        private static readonly ulong _SIGUSR2 = 0x11UL;

        private static readonly ulong _FPE_INTDIV = 0x1UL;
        private static readonly ulong _FPE_INTOVF = 0x2UL;
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

        private static readonly ulong __SC_PAGESIZE = 0xbUL;
        private static readonly ulong __SC_NPROCESSORS_ONLN = 0xfUL;

        private static readonly ulong _PTHREAD_CREATE_DETACHED = 0x40UL;

        private static readonly ulong _FORK_NOSIGCHLD = 0x1UL;
        private static readonly ulong _FORK_WAITPID = 0x2UL;

        private static readonly ulong _MAXHOSTNAMELEN = 0x100UL;

        private static readonly ulong _O_NONBLOCK = 0x80UL;
        private static readonly ulong _FD_CLOEXEC = 0x1UL;
        private static readonly ulong _F_GETFL = 0x3UL;
        private static readonly ulong _F_SETFL = 0x4UL;
        private static readonly ulong _F_SETFD = 0x2UL;

        private static readonly ulong _POLLIN = 0x1UL;
        private static readonly ulong _POLLOUT = 0x4UL;
        private static readonly ulong _POLLHUP = 0x10UL;
        private static readonly ulong _POLLERR = 0x8UL;

        private static readonly ulong _PORT_SOURCE_FD = 0x4UL;

        private partial struct semt
        {
            public uint sem_count;
            public ushort sem_type;
            public ushort sem_magic;
            public array<ulong> sem_pad1;
            public array<ulong> sem_pad2;
        }

        private partial struct sigset
        {
            public array<uint> __sigbits;
        }

        private partial struct stackt
        {
            public ptr<byte> ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
            public array<byte> pad_cgo_0;
        }

        private partial struct siginfo
        {
            public int si_signo;
            public int si_code;
            public int si_errno;
            public int si_pad;
            public array<byte> __data;
        }

        private partial struct sigactiont
        {
            public int sa_flags;
            public array<byte> pad_cgo_0;
            public array<byte> _funcptr;
            public sigset sa_mask;
        }

        private partial struct fpregset
        {
            public array<byte> fp_reg_set;
        }

        private partial struct mcontext
        {
            public array<long> gregs;
            public fpregset fpregs;
        }

        private partial struct ucontext
        {
            public ulong uc_flags;
            public ptr<ucontext> uc_link;
            public sigset uc_sigmask;
            public stackt uc_stack;
            public array<byte> pad_cgo_0;
            public mcontext uc_mcontext;
            public array<long> uc_filler;
            public array<byte> pad_cgo_1;
        }

        private partial struct timespec
        {
            public long tv_sec;
            public long tv_nsec;
        }

        private partial struct timeval
        {
            public long tv_sec;
            public long tv_usec;
        }

        private static void set_usec(this ref timeval tv, int x)
        {
            tv.tv_usec = int64(x);
        }

        private partial struct itimerval
        {
            public timeval it_interval;
            public timeval it_value;
        }

        private partial struct portevent
        {
            public int portev_events;
            public ushort portev_source;
            public ushort portev_pad;
            public ulong portev_object;
            public ptr<byte> portev_user;
        }

        private partial struct pthread // : uint
        {
        }
        private partial struct pthreadattr
        {
            public ptr<byte> __pthread_attrp;
        }

        private partial struct stat
        {
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

        private static readonly ulong _REG_RDI = 0x8UL;
        private static readonly ulong _REG_RSI = 0x9UL;
        private static readonly ulong _REG_RDX = 0xcUL;
        private static readonly ulong _REG_RCX = 0xdUL;
        private static readonly ulong _REG_R8 = 0x7UL;
        private static readonly ulong _REG_R9 = 0x6UL;
        private static readonly ulong _REG_R10 = 0x5UL;
        private static readonly ulong _REG_R11 = 0x4UL;
        private static readonly ulong _REG_R12 = 0x3UL;
        private static readonly ulong _REG_R13 = 0x2UL;
        private static readonly ulong _REG_R14 = 0x1UL;
        private static readonly ulong _REG_R15 = 0x0UL;
        private static readonly ulong _REG_RBP = 0xaUL;
        private static readonly ulong _REG_RBX = 0xbUL;
        private static readonly ulong _REG_RAX = 0xeUL;
        private static readonly ulong _REG_GS = 0x17UL;
        private static readonly ulong _REG_FS = 0x16UL;
        private static readonly ulong _REG_ES = 0x18UL;
        private static readonly ulong _REG_DS = 0x19UL;
        private static readonly ulong _REG_TRAPNO = 0xfUL;
        private static readonly ulong _REG_ERR = 0x10UL;
        private static readonly ulong _REG_RIP = 0x11UL;
        private static readonly ulong _REG_CS = 0x12UL;
        private static readonly ulong _REG_RFLAGS = 0x13UL;
        private static readonly ulong _REG_RSP = 0x14UL;
        private static readonly ulong _REG_SS = 0x15UL;
    }
}
