// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips mipsle
// +build linux

// package runtime -- go2cs converted at 2020 August 29 08:16:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_linux_mipsx.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EINTR = 0x4UL;
        private static readonly ulong _EAGAIN = 0xbUL;
        private static readonly ulong _ENOMEM = 0xcUL;

        private static readonly ulong _PROT_NONE = 0x0UL;
        private static readonly ulong _PROT_READ = 0x1UL;
        private static readonly ulong _PROT_WRITE = 0x2UL;
        private static readonly ulong _PROT_EXEC = 0x4UL;

        private static readonly ulong _MAP_ANON = 0x800UL;
        private static readonly ulong _MAP_PRIVATE = 0x2UL;
        private static readonly ulong _MAP_FIXED = 0x10UL;

        private static readonly ulong _MADV_DONTNEED = 0x4UL;
        private static readonly ulong _MADV_HUGEPAGE = 0xeUL;
        private static readonly ulong _MADV_NOHUGEPAGE = 0xfUL;

        private static readonly ulong _SA_RESTART = 0x10000000UL;
        private static readonly ulong _SA_ONSTACK = 0x8000000UL;
        private static readonly ulong _SA_SIGINFO = 0x8UL;

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
        private static readonly ulong _SIGUSR1 = 0x10UL;
        private static readonly ulong _SIGUSR2 = 0x11UL;
        private static readonly ulong _SIGCHLD = 0x12UL;
        private static readonly ulong _SIGPWR = 0x13UL;
        private static readonly ulong _SIGWINCH = 0x14UL;
        private static readonly ulong _SIGURG = 0x15UL;
        private static readonly ulong _SIGIO = 0x16UL;
        private static readonly ulong _SIGSTOP = 0x17UL;
        private static readonly ulong _SIGTSTP = 0x18UL;
        private static readonly ulong _SIGCONT = 0x19UL;
        private static readonly ulong _SIGTTIN = 0x1aUL;
        private static readonly ulong _SIGTTOU = 0x1bUL;
        private static readonly ulong _SIGVTALRM = 0x1cUL;
        private static readonly ulong _SIGPROF = 0x1dUL;
        private static readonly ulong _SIGXCPU = 0x1eUL;
        private static readonly ulong _SIGXFSZ = 0x1fUL;

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

        private static readonly ulong _EPOLLIN = 0x1UL;
        private static readonly ulong _EPOLLOUT = 0x4UL;
        private static readonly ulong _EPOLLERR = 0x8UL;
        private static readonly ulong _EPOLLHUP = 0x10UL;
        private static readonly ulong _EPOLLRDHUP = 0x2000UL;
        private static readonly ulong _EPOLLET = 0x80000000UL;
        private static readonly ulong _EPOLL_CLOEXEC = 0x80000UL;
        private static readonly ulong _EPOLL_CTL_ADD = 0x1UL;
        private static readonly ulong _EPOLL_CTL_DEL = 0x2UL;
        private static readonly ulong _EPOLL_CTL_MOD = 0x3UL;

        private partial struct timespec
        {
            public int tv_sec;
            public int tv_nsec;
        }

        //go:nosplit
        private static void set_sec(this ref timespec ts, long x)
        {
            ts.tv_sec = int32(x);
        }

        //go:nosplit
        private static void set_nsec(this ref timespec ts, int x)
        {
            ts.tv_nsec = x;
        }

        private partial struct timeval
        {
            public int tv_sec;
            public int tv_usec;
        }

        //go:nosplit
        private static void set_usec(this ref timeval tv, int x)
        {
            tv.tv_usec = x;
        }

        private partial struct sigactiont
        {
            public uint sa_flags;
            public System.UIntPtr sa_handler;
            public array<uint> sa_mask; // linux header does not have sa_restorer field,
// but it is used in setsig(). it is no harm to put it here
            public System.UIntPtr sa_restorer;
        }

        private partial struct siginfo
        {
            public int si_signo;
            public int si_code;
            public int si_errno; // below here is a union; si_addr is the only field we use
            public uint si_addr;
        }

        private partial struct itimerval
        {
            public timeval it_interval;
            public timeval it_value;
        }

        private partial struct epollevent
        {
            public uint events;
            public array<byte> pad_cgo_0;
            public ulong data;
        }

        private static readonly ulong _O_RDONLY = 0x0UL;
        private static readonly ulong _O_CLOEXEC = 0x80000UL;
        private static readonly long _SA_RESTORER = 0L;

        private partial struct stackt
        {
            public ptr<byte> ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
        }

        private partial struct sigcontext
        {
            public uint sc_regmask;
            public uint sc_status;
            public ulong sc_pc;
            public array<ulong> sc_regs;
            public array<ulong> sc_fpregs;
            public uint sc_acx;
            public uint sc_fpc_csr;
            public uint sc_fpc_eir;
            public uint sc_used_math;
            public uint sc_dsp;
            public ulong sc_mdhi;
            public ulong sc_mdlo;
            public uint sc_hi1;
            public uint sc_lo1;
            public uint sc_hi2;
            public uint sc_lo2;
            public uint sc_hi3;
            public uint sc_lo3;
        }

        private partial struct ucontext
        {
            public uint uc_flags;
            public ptr<ucontext> uc_link;
            public stackt uc_stack;
            public array<byte> Pad_cgo_0;
            public sigcontext uc_mcontext;
            public array<uint> uc_sigmask;
        }
    }
}
