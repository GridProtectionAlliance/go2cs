// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_linux.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct mOS
        {
        }

        //go:noescape
        private static int futex(unsafe.Pointer addr, int op, uint val, unsafe.Pointer ts, unsafe.Pointer addr2, uint val3)
;

        // Linux futex.
        //
        //    futexsleep(uint32 *addr, uint32 val)
        //    futexwakeup(uint32 *addr)
        //
        // Futexsleep atomically checks if *addr == val and if so, sleeps on addr.
        // Futexwakeup wakes up threads sleeping on addr.
        // Futexsleep is allowed to wake up spuriously.

        private static readonly long _FUTEX_WAIT = 0L;
        private static readonly long _FUTEX_WAKE = 1L;

        // Atomically,
        //    if(*addr == val) sleep
        // Might be woken up spuriously; that's allowed.
        // Don't sleep longer than ns; ns < 0 means forever.
        //go:nosplit
        private static void futexsleep(ref uint addr, uint val, long ns)
        {
            timespec ts = default; 

            // Some Linux kernels have a bug where futex of
            // FUTEX_WAIT returns an internal error code
            // as an errno. Libpthread ignores the return value
            // here, and so can we: as it says a few lines up,
            // spurious wakeups are allowed.
            if (ns < 0L)
            {>>MARKER:FUNCTION_futex_BLOCK_PREFIX<<
                futex(@unsafe.Pointer(addr), _FUTEX_WAIT, val, null, null, 0L);
                return;
            } 

            // It's difficult to live within the no-split stack limits here.
            // On ARM and 386, a 64-bit divide invokes a general software routine
            // that needs more stack than we can afford. So we use timediv instead.
            // But on real 64-bit systems, where words are larger but the stack limit
            // is not, even timediv is too heavy, and we really need to use just an
            // ordinary machine instruction.
            if (sys.PtrSize == 8L)
            {
                ts.set_sec(ns / 1000000000L);
                ts.set_nsec(int32(ns % 1000000000L));
            }
            else
            {
                ts.tv_nsec = 0L;
                ts.set_sec(int64(timediv(ns, 1000000000L, (int32.Value)(@unsafe.Pointer(ref ts.tv_nsec)))));
            }
            futex(@unsafe.Pointer(addr), _FUTEX_WAIT, val, @unsafe.Pointer(ref ts), null, 0L);
        }

        // If any procs are sleeping on addr, wake up at most cnt.
        //go:nosplit
        private static void futexwakeup(ref uint addr, uint cnt)
        {
            var ret = futex(@unsafe.Pointer(addr), _FUTEX_WAKE, cnt, null, null, 0L);
            if (ret >= 0L)
            {
                return;
            } 

            // I don't know that futex wakeup can return
            // EAGAIN or EINTR, but if it does, it would be
            // safe to loop and call futex again.
            systemstack(() =>
            {
                print("futexwakeup addr=", addr, " returned ", ret, "\n");
            }) * (int32.Value)(@unsafe.Pointer(uintptr(0x1006UL)));

            0x1006UL;
        }

        private static int getproccount()
        { 
            // This buffer is huge (8 kB) but we are on the system stack
            // and there should be plenty of space (64 kB).
            // Also this is a leaf, so we're not holding up the memory for long.
            // See golang.org/issue/11823.
            // The suggested behavior here is to keep trying with ever-larger
            // buffers, but we don't have a dynamic memory allocator at the
            // moment, so that's a bit tricky and seems like overkill.
            const long maxCPUs = 64L * 1024L;

            array<byte> buf = new array<byte>(maxCPUs / 8L);
            var r = sched_getaffinity(0L, @unsafe.Sizeof(buf), ref buf[0L]);
            if (r < 0L)
            {
                return 1L;
            }
            var n = int32(0L);
            foreach (var (_, v) in buf[..r])
            {
                while (v != 0L)
                {
                    n += int32(v & 1L);
                    v >>= 1L;
                }

            }
            if (n == 0L)
            {
                n = 1L;
            }
            return n;
        }

        // Clone, the Linux rfork.
        private static readonly ulong _CLONE_VM = 0x100UL;
        private static readonly ulong _CLONE_FS = 0x200UL;
        private static readonly ulong _CLONE_FILES = 0x400UL;
        private static readonly ulong _CLONE_SIGHAND = 0x800UL;
        private static readonly ulong _CLONE_PTRACE = 0x2000UL;
        private static readonly ulong _CLONE_VFORK = 0x4000UL;
        private static readonly ulong _CLONE_PARENT = 0x8000UL;
        private static readonly ulong _CLONE_THREAD = 0x10000UL;
        private static readonly ulong _CLONE_NEWNS = 0x20000UL;
        private static readonly ulong _CLONE_SYSVSEM = 0x40000UL;
        private static readonly ulong _CLONE_SETTLS = 0x80000UL;
        private static readonly ulong _CLONE_PARENT_SETTID = 0x100000UL;
        private static readonly ulong _CLONE_CHILD_CLEARTID = 0x200000UL;
        private static readonly ulong _CLONE_UNTRACED = 0x800000UL;
        private static readonly ulong _CLONE_CHILD_SETTID = 0x1000000UL;
        private static readonly ulong _CLONE_STOPPED = 0x2000000UL;
        private static readonly ulong _CLONE_NEWUTS = 0x4000000UL;
        private static readonly ulong _CLONE_NEWIPC = 0x8000000UL;

        private static readonly var cloneFlags = _CLONE_VM | _CLONE_FS | _CLONE_FILES | _CLONE_SIGHAND | _CLONE_SYSVSEM | _CLONE_THREAD; /* revisit - okay for now */

        //go:noescape
        private static int clone(int flags, unsafe.Pointer stk, unsafe.Pointer mp, unsafe.Pointer gp, unsafe.Pointer fn)
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            /*
                 * note: strace gets confused if we use CLONE_PTRACE here.
                 */
            if (false)
            {>>MARKER:FUNCTION_clone_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " clone=", funcPC(clone), " id=", mp.id, " ostk=", ref mp, "\n");
            } 

            // Disable signals during clone, so that the new thread starts
            // with signals disabled. It will enable them in minit.
            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);
            var ret = clone(cloneFlags, stk, @unsafe.Pointer(mp), @unsafe.Pointer(mp.g0), @unsafe.Pointer(funcPC(mstart)));
            sigprocmask(_SIG_SETMASK, ref oset, null);

            if (ret < 0L)
            {
                print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", -ret, ")\n");
                if (ret == -_EAGAIN)
                {
                    println("runtime: may need to increase max user processes (ulimit -u)");
                }
                throw("newosproc");
            }
        }

        // Version of newosproc that doesn't require a valid G.
        //go:nosplit
        private static void newosproc0(System.UIntPtr stacksize, unsafe.Pointer fn)
        {
            var stack = sysAlloc(stacksize, ref memstats.stacks_sys);
            if (stack == null)
            {
                write(2L, @unsafe.Pointer(ref failallocatestack[0L]), int32(len(failallocatestack)));
                exit(1L);
            }
            var ret = clone(cloneFlags, @unsafe.Pointer(uintptr(stack) + stacksize), null, null, fn);
            if (ret < 0L)
            {
                write(2L, @unsafe.Pointer(ref failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }
        }

        private static slice<byte> failallocatestack = (slice<byte>)"runtime: failed to allocate stack for the new OS thread\n";
        private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

        private static readonly long _AT_NULL = 0L; // End of vector
        private static readonly long _AT_PAGESZ = 6L; // System physical page size
        private static readonly long _AT_HWCAP = 16L; // hardware capability bit vector
        private static readonly long _AT_RANDOM = 25L; // introduced in 2.6.29
        private static readonly long _AT_HWCAP2 = 26L; // hardware capability bit vector 2

        private static slice<byte> procAuxv = (slice<byte>)"/proc/self/auxv\x00";

        private static int mincore(unsafe.Pointer addr, System.UIntPtr n, ref byte dst)
;

        private static void sysargs(int argc, ptr<ptr<byte>> argv)
        {
            var n = argc + 1L; 

            // skip over argv, envp to get to auxv
            while (argv_index(argv, n) != null)
            {>>MARKER:FUNCTION_mincore_BLOCK_PREFIX<<
                n++;
            } 

            // skip NULL separator
 

            // skip NULL separator
            n++; 

            // now argv+n is auxv
            ref array<System.UIntPtr> auxv = new ptr<ref array<System.UIntPtr>>(add(@unsafe.Pointer(argv), uintptr(n) * sys.PtrSize));
            if (sysauxv(auxv[..]) != 0L)
            {
                return;
            } 
            // In some situations we don't get a loader-provided
            // auxv, such as when loaded as a library on Android.
            // Fall back to /proc/self/auxv.
            var fd = open(ref procAuxv[0L], 0L, 0L);
            if (fd < 0L)
            { 
                // On Android, /proc/self/auxv might be unreadable (issue 9229), so we fallback to
                // try using mincore to detect the physical page size.
                // mincore should return EINVAL when address is not a multiple of system page size.
                const long size = 256L << (int)(10L); // size of memory region to allocate
 // size of memory region to allocate
                var (p, err) = mmap(null, size, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
                if (err != 0L)
                {
                    return;
                }
                n = default;
                n = 4L << (int)(10L);

                while (n < size)
                {
                    var err = mincore(@unsafe.Pointer(uintptr(p) + n), 1L, ref addrspace_vec[0L]);
                    if (err == 0L)
                    {
                        physPageSize = n;
                        break;
                    n <<= 1L;
                    }
                }

                if (physPageSize == 0L)
                {
                    physPageSize = size;
                }
                munmap(p, size);
                return;
            }
            array<System.UIntPtr> buf = new array<System.UIntPtr>(128L);
            n = read(fd, noescape(@unsafe.Pointer(ref buf[0L])), int32(@unsafe.Sizeof(buf)));
            closefd(fd);
            if (n < 0L)
            {
                return;
            } 
            // Make sure buf is terminated, even if we didn't read
            // the whole file.
            buf[len(buf) - 2L] = _AT_NULL;
            sysauxv(buf[..]);
        }

        private static long sysauxv(slice<System.UIntPtr> auxv)
        {
            long i = default;
            while (auxv[i] != _AT_NULL)
            {
                var tag = auxv[i];
                var val = auxv[i + 1L];

                if (tag == _AT_RANDOM) 
                    // The kernel provides a pointer to 16-bytes
                    // worth of random data.
                    startupRandomData = new ptr<ref array<byte>>(@unsafe.Pointer(val))[..];
                else if (tag == _AT_PAGESZ) 
                    physPageSize = val;
                                archauxv(tag, val);
                i += 2L;
            }

            return i / 2L;
        }

        private static void osinit()
        {
            ncpu = getproccount();
        }

        private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

        private static void getRandomData(slice<byte> r)
        {
            if (startupRandomData != null)
            {
                var n = copy(r, startupRandomData);
                extendRandom(r, n);
                return;
            }
            var fd = open(ref urandom_dev[0L], 0L, 0L);
            n = read(fd, @unsafe.Pointer(ref r[0L]), int32(len(r)));
            closefd(fd);
            extendRandom(r, int(n));
        }

        private static void goenvs()
        {
            goenvs_unix();
        }

        // Called to do synchronous initialization of Go code built with
        // -buildmode=c-archive or -buildmode=c-shared.
        // None of the Go runtime is initialized.
        //go:nosplit
        //go:nowritebarrierrec
        private static void libpreinit()
        {
            initsig(true);
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ref m mp)
        {
            mp.gsignal = malg(32L * 1024L); // Linux wants >= 2K
            mp.gsignal.m = mp;
        }

        private static uint gettid()
;

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
            minitSignals(); 

            // for debuggers, in case cgo created the thread
            getg().m.procid = uint64(gettid());
        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        {
            unminitSignals();
        }

        private static System.UIntPtr memlimit()
        {
            /*
                    TODO: Convert to Go when something actually uses the result.

                    Rlimit rl;
                    extern byte runtime·text[], runtime·end[];
                    uintptr used;

                    if(runtime·getrlimit(RLIMIT_AS, &rl) != 0)
                        return 0;
                    if(rl.rlim_cur >= 0x7fffffff)
                        return 0;

                    // Estimate our VM footprint excluding the heap.
                    // Not an exact science: use size of binary plus
                    // some room for thread stacks.
                    used = runtime·end - runtime·text + (64<<20);
                    if(used >= rl.rlim_cur)
                        return 0;

                    // If there's not at least 16 MB left, we're probably
                    // not going to be able to do much. Treat as no limit.
                    rl.rlim_cur -= used;
                    if(rl.rlim_cur < (16<<20))
                        return 0;

                    return rl.rlim_cur - used;
                */

            return 0L;
        }

        //#ifdef GOARCH_386
        //#define sa_handler k_sa_handler
        //#endif

        private static void sigreturn()
;
        private static void sigtramp(uint sig, ref siginfo info, unsafe.Pointer ctx)
;
        private static void cgoSigtramp()
;

        //go:noescape
        private static void sigaltstack(ref stackt @new, ref stackt old)
;

        //go:noescape
        private static void setitimer(int mode, ref itimerval @new, ref itimerval old)
;

        //go:noescape
        private static void rtsigprocmask(int how, ref sigset @new, ref sigset old, int size)
;

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigprocmask(int how, ref sigset @new, ref sigset old)
        {
            rtsigprocmask(how, new, old, int32(@unsafe.Sizeof(new.Value)));
        }

        //go:noescape
        private static int getrlimit(int kind, unsafe.Pointer limit)
;
        private static void raise(uint sig)
;
        private static void raiseproc(uint sig)
;

        //go:noescape
        private static int sched_getaffinity(System.UIntPtr pid, System.UIntPtr len, ref byte buf)
;
        private static void osyield()
;

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            sigactiont sa = default;
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTORER | _SA_RESTART;
            sigfillset(ref sa.sa_mask); 
            // Although Linux manpage says "sa_restorer element is obsolete and
            // should not be used". x86_64 kernel requires it. Only use it on
            // x86.
            if (GOARCH == "386" || GOARCH == "amd64")
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                sa.sa_restorer = funcPC(sigreturn);
            }
            if (fn == funcPC(sighandler))
            {>>MARKER:FUNCTION_sched_getaffinity_BLOCK_PREFIX<<
                if (iscgo)
                {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                    fn = funcPC(cgoSigtramp);
                }
                else
                {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<<
                    fn = funcPC(sigtramp);
                }
            }
            sa.sa_handler = fn;
            rt_sigaction(uintptr(i), ref sa, null, @unsafe.Sizeof(sa.sa_mask));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            sigactiont sa = default;
            rt_sigaction(uintptr(i), null, ref sa, @unsafe.Sizeof(sa.sa_mask));
            if (sa.sa_flags & _SA_ONSTACK != 0L)
            {>>MARKER:FUNCTION_getrlimit_BLOCK_PREFIX<<
                return;
            }
            sa.sa_flags |= _SA_ONSTACK;
            rt_sigaction(uintptr(i), ref sa, null, @unsafe.Sizeof(sa.sa_mask));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            sigactiont sa = default;
            if (rt_sigaction(uintptr(i), null, ref sa, @unsafe.Sizeof(sa.sa_mask)) != 0L)
            {>>MARKER:FUNCTION_rtsigprocmask_BLOCK_PREFIX<<
                throw("rt_sigaction read failure");
            }
            return sa.sa_handler;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ref stackt s, System.UIntPtr sp)
        {
            (uintptr.Value)(@unsafe.Pointer(ref s.ss_sp)).Value;

            sp;
        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
        }
    }
}
