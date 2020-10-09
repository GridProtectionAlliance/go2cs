// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package runtime -- go2cs converted at 2020 October 09 04:47:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_js.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void exit(int code)
;

        private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n)
        {
            if (fd > 2L)
            {>>MARKER:FUNCTION_exit_BLOCK_PREFIX<<
                throw("runtime.write to fd > 2 is unsupported");
            }

            wasmWrite(fd, p, n);
            return n;

        }

        // Stubs so tests can link correctly. These should never be called.
        private static int open(ptr<byte> _addr_name, int mode, int perm) => func((_, panic, __) =>
        {
            ref byte name = ref _addr_name.val;

            panic("not implemented");
        });
        private static int closefd(int fd) => func((_, panic, __) =>
        {
            panic("not implemented");
        });
        private static int read(int fd, unsafe.Pointer p, int n) => func((_, panic, __) =>
        {
            panic("not implemented");
        });

        //go:noescape
        private static void wasmWrite(System.UIntPtr fd, unsafe.Pointer p, int n)
;

        private static void usleep(uint usec)
;

        private static void exitThread(ptr<uint> wait)
;

        private partial struct mOS
        {
        }

        private static void osyield()
;

        private static readonly ulong _SIGSEGV = (ulong)0xbUL;



        private static void sigpanic()
        {
            var g = getg();
            if (!canpanic(g))
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                throw("unexpected signal during runtime execution");
            } 

            // js only invokes the exception handler for memory faults.
            g.sig = _SIGSEGV;
            panicmem();

        }

        private partial struct sigset
        {
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            mp.gsignal = malg(32L * 1024L);
            mp.gsignal.m = mp;
        }

        //go:nosplit
        private static void msigsave(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

        }

        //go:nosplit
        private static void msigrestore(sigset sigmask)
        {
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void clearSignalHandlers()
        {
        }

        //go:nosplit
        private static void sigblock()
        {
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
        }

        // Called from dropm to undo the effect of an minit.
        private static void unminit()
        {
        }

        private static void osinit()
        {
            ncpu = 1L;
            getg().m.procid = 2L;
            physPageSize = 64L * 1024L;
        }

        // wasm has no signals
        private static readonly long _NSIG = (long)0L;



        private static @string signame(uint sig)
        {
            return "";
        }

        private static void crash()
        {
            (int32.val)(null).val;

            0L;

        }

        private static void getRandomData(slice<byte> r)
;

        private static void goenvs()
        {
            goenvs_unix();
        }

        private static void initsig(bool preinit)
        {
        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ptr<m> _addr_mp) => func((_, panic, __) =>
        {
            ref m mp = ref _addr_mp.val;

            panic("newosproc: not implemented");
        });

        private static void setProcessCPUProfiler(int hz)
        {
        }
        private static void setThreadCPUProfiler(int hz)
        {
        }
        private static void sigdisable(uint _p0)
        {
        }
        private static void sigenable(uint _p0)
        {
        }
        private static void sigignore(uint _p0)
        {
        }

        //go:linkname os_sigpipe os.sigpipe
        private static void os_sigpipe()
        {
            throw("too many writes on closed pipe");
        }

        //go:nosplit
        private static long cputicks()
        { 
            // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
            // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
            return nanotime();

        }

        //go:linkname syscall_now syscall.now
        private static (long, int) syscall_now()
        {
            long sec = default;
            int nsec = default;

            sec, nsec, _ = time_now();
            return ;
        }

        // gsignalStack is unused on js.
        private partial struct gsignalStack
        {
        }

        private static readonly var preemptMSupported = false;



        private static void preemptM(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;
 
            // No threads, so nothing to do.
        }
    }
}
