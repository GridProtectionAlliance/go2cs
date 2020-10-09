// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix

// package runtime -- go2cs converted at 2020 October 09 04:45:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\defs_aix_ppc64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong _EPERM = (ulong)0x1UL;
        private static readonly ulong _ENOENT = (ulong)0x2UL;
        private static readonly ulong _EINTR = (ulong)0x4UL;
        private static readonly ulong _EAGAIN = (ulong)0xbUL;
        private static readonly ulong _ENOMEM = (ulong)0xcUL;
        private static readonly ulong _EACCES = (ulong)0xdUL;
        private static readonly ulong _EFAULT = (ulong)0xeUL;
        private static readonly ulong _EINVAL = (ulong)0x16UL;
        private static readonly ulong _ETIMEDOUT = (ulong)0x4eUL;

        private static readonly ulong _PROT_NONE = (ulong)0x0UL;
        private static readonly ulong _PROT_READ = (ulong)0x1UL;
        private static readonly ulong _PROT_WRITE = (ulong)0x2UL;
        private static readonly ulong _PROT_EXEC = (ulong)0x4UL;

        private static readonly ulong _MAP_ANON = (ulong)0x10UL;
        private static readonly ulong _MAP_PRIVATE = (ulong)0x2UL;
        private static readonly ulong _MAP_FIXED = (ulong)0x100UL;
        private static readonly ulong _MADV_DONTNEED = (ulong)0x4UL;

        private static readonly ulong _SIGHUP = (ulong)0x1UL;
        private static readonly ulong _SIGINT = (ulong)0x2UL;
        private static readonly ulong _SIGQUIT = (ulong)0x3UL;
        private static readonly ulong _SIGILL = (ulong)0x4UL;
        private static readonly ulong _SIGTRAP = (ulong)0x5UL;
        private static readonly ulong _SIGABRT = (ulong)0x6UL;
        private static readonly ulong _SIGBUS = (ulong)0xaUL;
        private static readonly ulong _SIGFPE = (ulong)0x8UL;
        private static readonly ulong _SIGKILL = (ulong)0x9UL;
        private static readonly ulong _SIGUSR1 = (ulong)0x1eUL;
        private static readonly ulong _SIGSEGV = (ulong)0xbUL;
        private static readonly ulong _SIGUSR2 = (ulong)0x1fUL;
        private static readonly ulong _SIGPIPE = (ulong)0xdUL;
        private static readonly ulong _SIGALRM = (ulong)0xeUL;
        private static readonly ulong _SIGCHLD = (ulong)0x14UL;
        private static readonly ulong _SIGCONT = (ulong)0x13UL;
        private static readonly ulong _SIGSTOP = (ulong)0x11UL;
        private static readonly ulong _SIGTSTP = (ulong)0x12UL;
        private static readonly ulong _SIGTTIN = (ulong)0x15UL;
        private static readonly ulong _SIGTTOU = (ulong)0x16UL;
        private static readonly ulong _SIGURG = (ulong)0x10UL;
        private static readonly ulong _SIGXCPU = (ulong)0x18UL;
        private static readonly ulong _SIGXFSZ = (ulong)0x19UL;
        private static readonly ulong _SIGVTALRM = (ulong)0x22UL;
        private static readonly ulong _SIGPROF = (ulong)0x20UL;
        private static readonly ulong _SIGWINCH = (ulong)0x1cUL;
        private static readonly ulong _SIGIO = (ulong)0x17UL;
        private static readonly ulong _SIGPWR = (ulong)0x1dUL;
        private static readonly ulong _SIGSYS = (ulong)0xcUL;
        private static readonly ulong _SIGTERM = (ulong)0xfUL;
        private static readonly ulong _SIGEMT = (ulong)0x7UL;
        private static readonly ulong _SIGWAITING = (ulong)0x27UL;

        private static readonly ulong _FPE_INTDIV = (ulong)0x14UL;
        private static readonly ulong _FPE_INTOVF = (ulong)0x15UL;
        private static readonly ulong _FPE_FLTDIV = (ulong)0x16UL;
        private static readonly ulong _FPE_FLTOVF = (ulong)0x17UL;
        private static readonly ulong _FPE_FLTUND = (ulong)0x18UL;
        private static readonly ulong _FPE_FLTRES = (ulong)0x19UL;
        private static readonly ulong _FPE_FLTINV = (ulong)0x1aUL;
        private static readonly ulong _FPE_FLTSUB = (ulong)0x1bUL;

        private static readonly ulong _BUS_ADRALN = (ulong)0x1UL;
        private static readonly ulong _BUS_ADRERR = (ulong)0x2UL;
        private static readonly ulong _BUS_OBJERR = (ulong)0x3UL;
        private static readonly _SEGV_MAPERR _ = (_SEGV_MAPERR)0x32UL;
        private static readonly ulong _SEGV_ACCERR = (ulong)0x33UL;

        private static readonly ulong _ITIMER_REAL = (ulong)0x0UL;
        private static readonly ulong _ITIMER_VIRTUAL = (ulong)0x1UL;
        private static readonly ulong _ITIMER_PROF = (ulong)0x2UL;

        private static readonly ulong _O_RDONLY = (ulong)0x0UL;
        private static readonly ulong _O_NONBLOCK = (ulong)0x4UL;

        private static readonly ulong _SS_DISABLE = (ulong)0x2UL;
        private static readonly ulong _SI_USER = (ulong)0x0UL;
        private static readonly ulong _SIG_BLOCK = (ulong)0x0UL;
        private static readonly ulong _SIG_UNBLOCK = (ulong)0x1UL;
        private static readonly ulong _SIG_SETMASK = (ulong)0x2UL;

        private static readonly ulong _SA_SIGINFO = (ulong)0x100UL;
        private static readonly ulong _SA_RESTART = (ulong)0x8UL;
        private static readonly ulong _SA_ONSTACK = (ulong)0x1UL;

        private static readonly ulong _PTHREAD_CREATE_DETACHED = (ulong)0x1UL;

        private static readonly ulong __SC_PAGE_SIZE = (ulong)0x30UL;
        private static readonly ulong __SC_NPROCESSORS_ONLN = (ulong)0x48UL;

        private static readonly ulong _F_SETFD = (ulong)0x2UL;
        private static readonly ulong _F_SETFL = (ulong)0x4UL;
        private static readonly ulong _F_GETFD = (ulong)0x1UL;
        private static readonly ulong _F_GETFL = (ulong)0x3UL;
        private static readonly ulong _FD_CLOEXEC = (ulong)0x1UL;


        private partial struct sigset // : array<ulong>
        {
        }

        private static sigset sigset_all = new sigset(^uint64(0),^uint64(0),^uint64(0),^uint64(0));

        private partial struct siginfo
        {
            public int si_signo;
            public int si_errno;
            public int si_code;
            public int si_pid;
            public uint si_uid;
            public int si_status;
            public System.UIntPtr si_addr;
            public long si_band;
            public array<int> si_value; // [8]byte
            public int __si_flags;
            public array<int> __pad;
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

        private partial struct stackt
        {
            public System.UIntPtr ss_sp;
            public System.UIntPtr ss_size;
            public int ss_flags;
            public array<int> __pad;
            public array<byte> pas_cgo_0;
        }

        private partial struct sigcontext
        {
            public int sc_onstack;
            public array<byte> pad_cgo_0;
            public sigset sc_mask;
            public int sc_uerror;
            public context64 sc_jmpbuf;
        }

        private partial struct ucontext
        {
            public int __sc_onstack;
            public array<byte> pad_cgo_0;
            public sigset uc_sigmask;
            public int __sc_error;
            public array<byte> pad_cgo_1;
            public context64 uc_mcontext;
            public ptr<ucontext> uc_link;
            public stackt uc_stack;
            public System.UIntPtr __extctx; // pointer to struct __extctx but we don't use it
            public int __extctx_magic;
            public int __pad;
        }

        private partial struct context64
        {
            public array<ulong> gpr;
            public ulong msr;
            public ulong iar;
            public ulong lr;
            public ulong ctr;
            public uint cr;
            public uint xer;
            public uint fpscr;
            public uint fpscrx;
            public array<ulong> except;
            public array<double> fpr;
            public byte fpeu;
            public byte fpinfo;
            public byte fpscr24_31;
            public array<byte> pad;
            public int excp_type;
        }

        private partial struct sigactiont
        {
            public System.UIntPtr sa_handler; // a union of two pointer
            public sigset sa_mask;
            public int sa_flags;
            public array<byte> pad_cgo_0;
        }

        private partial struct pthread // : uint
        {
        }
        private partial struct pthread_attr // : ptr<byte>
        {
        }

        private partial struct semt // : int
        {
        }
    }
}
