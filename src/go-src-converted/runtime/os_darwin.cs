// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_darwin.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private partial struct mOS
        {
            public uint machport; // return address for mach ipc
            public uint waitsema; // semaphore for parking on locks
        }

        private static long darwinVersion = default;

        private static int bsdthread_create(unsafe.Pointer stk, unsafe.Pointer arg, System.UIntPtr fn)
;
        private static int bsdthread_register()
;

        //go:noescape
        private static int mach_msg_trap(unsafe.Pointer h, int op, uint send_size, uint rcv_size, uint rcv_name, uint timeout, uint notify)
;

        private static uint mach_reply_port()
;
        private static uint mach_task_self()
;
        private static uint mach_thread_self()
;

        //go:noescape
        private static int sysctl(ref uint mib, uint miblen, ref byte @out, ref System.UIntPtr size, ref byte dst, System.UIntPtr ndst)
;

        private static void unimplemented(@string name)
        {
            println(name, "not implemented") * (int.Value)(@unsafe.Pointer(uintptr(1231L)));

            1231L;
        }

        //go:nosplit
        private static void semawakeup(ref m mp)
        {
            mach_semrelease(mp.waitsema);
        }

        //go:nosplit
        private static void semacreate(ref m mp)
        {
            if (mp.waitsema != 0L)
            {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                return;
            }
            systemstack(() =>
            {>>MARKER:FUNCTION_mach_thread_self_BLOCK_PREFIX<<
                mp.waitsema = mach_semcreate();
            });
        }

        // BSD interface for threading.
        private static void osinit()
        { 
            // bsdthread_register delayed until end of goenvs so that we
            // can look at the environment first.

            ncpu = getncpu();
            physPageSize = getPageSize();
            darwinVersion = getDarwinVersion();
        }

        private static readonly long _CTL_KERN = 1L;
        private static readonly long _CTL_HW = 6L;
        private static readonly long _KERN_OSRELEASE = 2L;
        private static readonly long _HW_NCPU = 3L;
        private static readonly long _HW_PAGESIZE = 7L;

        private static long getDarwinVersion()
        { 
            // Use sysctl to fetch kern.osrelease
            array<uint> mib = new array<uint>(new uint[] { _CTL_KERN, _KERN_OSRELEASE });
            array<byte> @out = new array<byte>(32L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_mach_task_self_BLOCK_PREFIX<<
                long ver = 0L;
                for (long i = 0L; i < int(nout) && out[i] >= '0' && out[i] <= '9'; i++)
                {>>MARKER:FUNCTION_mach_reply_port_BLOCK_PREFIX<<
                    ver *= 10L;
                    ver += int(out[i] - '0');
                }

                return ver;
            }
            return 17L; // should not happen: default to a newish version
        }

        private static int getncpu()
        { 
            // Use sysctl to fetch hw.ncpu.
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L && int32(out) > 0L)
            {>>MARKER:FUNCTION_mach_msg_trap_BLOCK_PREFIX<<
                return int32(out);
            }
            return 1L;
        }

        private static System.UIntPtr getPageSize()
        { 
            // Use sysctl to fetch hw.pagesize.
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L && int32(out) > 0L)
            {>>MARKER:FUNCTION_bsdthread_register_BLOCK_PREFIX<<
                return uintptr(out);
            }
            return 0L;
        }

        private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

        //go:nosplit
        private static void getRandomData(slice<byte> r)
        {
            var fd = open(ref urandom_dev[0L], 0L, 0L);
            var n = read(fd, @unsafe.Pointer(ref r[0L]), int32(len(r)));
            closefd(fd);
            extendRandom(r, int(n));
        }

        private static void goenvs()
        {
            goenvs_unix(); 

            // Register our thread-creation callback (see sys_darwin_{amd64,386}.s)
            // but only if we're not using cgo. If we are using cgo we need
            // to let the C pthread library install its own thread-creation callback.
            if (!iscgo)
            {>>MARKER:FUNCTION_bsdthread_create_BLOCK_PREFIX<<
                if (bsdthread_register() != 0L)
                {
                    if (gogetenv("DYLD_INSERT_LIBRARIES") != "")
                    {
                        throw("runtime: bsdthread_register error (unset DYLD_INSERT_LIBRARIES)");
                    }
                    throw("runtime: bsdthread_register error");
                }
            }
        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            if (false)
            {
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", ref mp, "\n");
            }
            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);
            var errno = bsdthread_create(stk, @unsafe.Pointer(mp), funcPC(mstart));
            sigprocmask(_SIG_SETMASK, ref oset, null);

            if (errno < 0L)
            {
                print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", -errno, ")\n");
                throw("runtime.newosproc");
            }
        }

        // newosproc0 is a version of newosproc that can be called before the runtime
        // is initialized.
        //
        // As Go uses bsdthread_register when running without cgo, this function is
        // not safe to use after initialization as it does not pass an M as fnarg.
        //
        //go:nosplit
        private static void newosproc0(System.UIntPtr stacksize, System.UIntPtr fn)
        {
            var stack = sysAlloc(stacksize, ref memstats.stacks_sys);
            if (stack == null)
            {
                write(2L, @unsafe.Pointer(ref failallocatestack[0L]), int32(len(failallocatestack)));
                exit(1L);
            }
            var stk = @unsafe.Pointer(uintptr(stack) + stacksize);

            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);
            var errno = bsdthread_create(stk, null, fn);
            sigprocmask(_SIG_SETMASK, ref oset, null);

            if (errno < 0L)
            {
                write(2L, @unsafe.Pointer(ref failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }
        }

        private static slice<byte> failallocatestack = (slice<byte>)"runtime: failed to allocate stack for the new OS thread\n";
        private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

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
            mp.gsignal = malg(32L * 1024L); // OS X wants >= 8K
            mp.gsignal.m = mp;
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        { 
            // The alternate signal stack is buggy on arm and arm64.
            // The signal handler handles it directly.
            // The sigaltstack assembly function does nothing.
            if (GOARCH != "arm" && GOARCH != "arm64")
            {
                minitSignalStack();
            }
            minitSignalMask();
        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        { 
            // The alternate signal stack is buggy on arm and arm64.
            // See minit.
            if (GOARCH != "arm" && GOARCH != "arm64")
            {
                unminitSignals();
            }
        }

        // Mach IPC, to get at semaphores
        // Definitions are in /usr/include/mach on a Mac.

        private static void macherror(int r, @string fn)
        {
            print("mach error ", fn, ": ", r, "\n");
            throw("mach error");
        }

        private static readonly var _DebugMach = false;



        private static machndr zerondr = default;

        private static uint mach_msgh_bits(uint a, uint b)
        {
            return a | b << (int)(8L);
        }

        private static int mach_msg(ref machheader h, int op, uint send_size, uint rcv_size, uint rcv_name, uint timeout, uint notify)
        { 
            // TODO: Loop on interrupt.
            return mach_msg_trap(@unsafe.Pointer(h), op, send_size, rcv_size, rcv_name, timeout, notify);
        }

        // Mach RPC (MIG)
        private static readonly long _MinMachMsg = 48L;
        private static readonly long _MachReply = 100L;

        private partial struct codemsg
        {
            public machheader h;
            public machndr ndr;
            public int code;
        }

        private static int machcall(ref machheader h, int maxsize, int rxsize)
        {
            var _g_ = getg();
            var port = _g_.m.machport;
            if (port == 0L)
            {
                port = mach_reply_port();
                _g_.m.machport = port;
            }
            h.msgh_bits |= mach_msgh_bits(_MACH_MSG_TYPE_COPY_SEND, _MACH_MSG_TYPE_MAKE_SEND_ONCE);
            h.msgh_local_port = port;
            h.msgh_reserved = 0L;
            var id = h.msgh_id;

            if (_DebugMach)
            {
                ref array<unsafe.Pointer> p = new ptr<ref array<unsafe.Pointer>>(@unsafe.Pointer(h));
                print("send:\t");
                uint i = default;
                for (i = 0L; i < h.msgh_size / uint32(@unsafe.Sizeof(p[0L])); i++)
                {
                    print(" ", p[i]);
                    if (i % 8L == 7L)
                    {
                        print("\n\t");
                    }
                }

                if (i % 8L != 0L)
                {
                    print("\n");
                }
            }
            var ret = mach_msg(h, _MACH_SEND_MSG | _MACH_RCV_MSG, h.msgh_size, uint32(maxsize), port, 0L, 0L);
            if (ret != 0L)
            {
                if (_DebugMach)
                {
                    print("mach_msg error ", ret, "\n");
                }
                return ret;
            }
            if (_DebugMach)
            {
                p = new ptr<ref array<unsafe.Pointer>>(@unsafe.Pointer(h));
                i = default;
                for (i = 0L; i < h.msgh_size / uint32(@unsafe.Sizeof(p[0L])); i++)
                {
                    print(" ", p[i]);
                    if (i % 8L == 7L)
                    {
                        print("\n\t");
                    }
                }

                if (i % 8L != 0L)
                {
                    print("\n");
                }
            }
            if (h.msgh_id != id + _MachReply)
            {
                if (_DebugMach)
                {
                    print("mach_msg _MachReply id mismatch ", h.msgh_id, " != ", id + _MachReply, "\n");
                }
                return -303L; // MIG_REPLY_MISMATCH
            } 
            // Look for a response giving the return value.
            // Any call can send this back with an error,
            // and some calls only have return values so they
            // send it back on success too. I don't quite see how
            // you know it's one of these and not the full response
            // format, so just look if the message is right.
            var c = (codemsg.Value)(@unsafe.Pointer(h));
            if (uintptr(h.msgh_size) == @unsafe.Sizeof(c.Value) && h.msgh_bits & _MACH_MSGH_BITS_COMPLEX == 0L)
            {
                if (_DebugMach)
                {
                    print("mig result ", c.code, "\n");
                }
                return c.code;
            }
            if (h.msgh_size != uint32(rxsize))
            {
                if (_DebugMach)
                {
                    print("mach_msg _MachReply size mismatch ", h.msgh_size, " != ", rxsize, "\n");
                }
                return -307L; // MIG_ARRAY_TOO_LARGE
            }
            return 0L;
        }

        // Semaphores!

        private static readonly long tmach_semcreate = 3418L;
        private static readonly var rmach_semcreate = tmach_semcreate + _MachReply;

        private static readonly long tmach_semdestroy = 3419L;
        private static readonly var rmach_semdestroy = tmach_semdestroy + _MachReply;

        private static readonly long _KERN_ABORTED = 14L;
        private static readonly long _KERN_OPERATION_TIMED_OUT = 49L;

        private partial struct tmach_semcreatemsg
        {
            public machheader h;
            public machndr ndr;
            public int policy;
            public int value;
        }

        private partial struct rmach_semcreatemsg
        {
            public machheader h;
            public machbody body;
            public machport semaphore;
        }

        private partial struct tmach_semdestroymsg
        {
            public machheader h;
            public machbody body;
            public machport semaphore;
        }

        private static uint mach_semcreate()
        {
            array<byte> m = new array<byte>(256L);
            var tx = (tmach_semcreatemsg.Value)(@unsafe.Pointer(ref m));
            var rx = (rmach_semcreatemsg.Value)(@unsafe.Pointer(ref m));

            tx.h.msgh_bits = 0L;
            tx.h.msgh_size = uint32(@unsafe.Sizeof(tx.Value));
            tx.h.msgh_remote_port = mach_task_self();
            tx.h.msgh_id = tmach_semcreate;
            tx.ndr = zerondr;

            tx.policy = 0L; // 0 = SYNC_POLICY_FIFO
            tx.value = 0L;

            while (true)
            {
                var r = machcall(ref tx.h, int32(@unsafe.Sizeof(m)), int32(@unsafe.Sizeof(rx.Value)));
                if (r == 0L)
                {
                    break;
                }
                if (r == _KERN_ABORTED)
                { // interrupted
                    continue;
                }
                macherror(r, "semaphore_create");
            }

            if (rx.body.msgh_descriptor_count != 1L)
            {
                unimplemented("mach_semcreate desc count");
            }
            return rx.semaphore.name;
        }

        private static void mach_semdestroy(uint sem)
        {
            array<byte> m = new array<byte>(256L);
            var tx = (tmach_semdestroymsg.Value)(@unsafe.Pointer(ref m));

            tx.h.msgh_bits = _MACH_MSGH_BITS_COMPLEX;
            tx.h.msgh_size = uint32(@unsafe.Sizeof(tx.Value));
            tx.h.msgh_remote_port = mach_task_self();
            tx.h.msgh_id = tmach_semdestroy;
            tx.body.msgh_descriptor_count = 1L;
            tx.semaphore.name = sem;
            tx.semaphore.disposition = _MACH_MSG_TYPE_MOVE_SEND;
            tx.semaphore._type = 0L;

            while (true)
            {
                var r = machcall(ref tx.h, int32(@unsafe.Sizeof(m)), 0L);
                if (r == 0L)
                {
                    break;
                }
                if (r == _KERN_ABORTED)
                { // interrupted
                    continue;
                }
                macherror(r, "semaphore_destroy");
            }

        }

        // The other calls have simple system call traps in sys_darwin_{amd64,386}.s

        private static int mach_semaphore_wait(uint sema)
;
        private static int mach_semaphore_timedwait(uint sema, uint sec, uint nsec)
;
        private static int mach_semaphore_signal(uint sema)
;
        private static int mach_semaphore_signal_all(uint sema)
;

        private static int semasleep1(long ns)
        {
            var _g_ = getg();

            if (ns >= 0L)
            {>>MARKER:FUNCTION_mach_semaphore_signal_all_BLOCK_PREFIX<<
                int nsecs = default;
                var secs = timediv(ns, 1000000000L, ref nsecs);
                var r = mach_semaphore_timedwait(_g_.m.waitsema, uint32(secs), uint32(nsecs));
                if (r == _KERN_ABORTED || r == _KERN_OPERATION_TIMED_OUT)
                {>>MARKER:FUNCTION_mach_semaphore_signal_BLOCK_PREFIX<<
                    return -1L;
                }
                if (r != 0L)
                {>>MARKER:FUNCTION_mach_semaphore_timedwait_BLOCK_PREFIX<<
                    macherror(r, "semaphore_wait");
                }
                return 0L;
            }
            while (true)
            {>>MARKER:FUNCTION_mach_semaphore_wait_BLOCK_PREFIX<<
                r = mach_semaphore_wait(_g_.m.waitsema);
                if (r == 0L)
                {
                    break;
                } 
                // Note: We don't know how this call (with no timeout) can get _KERN_OPERATION_TIMED_OUT,
                // but it does reliably, though at a very low rate, on OS X 10.8, 10.9, 10.10, and 10.11.
                // See golang.org/issue/17161.
                if (r == _KERN_ABORTED || r == _KERN_OPERATION_TIMED_OUT)
                { // interrupted
                    continue;
                }
                macherror(r, "semaphore_wait");
            }

            return 0L;
        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            int r = default;
            systemstack(() =>
            {
                r = semasleep1(ns);
            });
            return r;
        }

        //go:nosplit
        private static void mach_semrelease(uint sem)
        {
            while (true)
            {
                var r = mach_semaphore_signal(sem);
                if (r == 0L)
                {
                    break;
                }
                if (r == _KERN_ABORTED)
                { // interrupted
                    continue;
                } 

                // mach_semrelease must be completely nosplit,
                // because it is called from Go code.
                // If we're going to die, start that process on the system stack
                // to avoid a Go stack split.
                systemstack(() =>
                {
                    macherror(r, "semaphore_signal");

                });
            }

        }

        //go:nosplit
        private static void osyield()
        {
            usleep(1L);
        }

        private static System.UIntPtr memlimit()
        { 
            // NOTE(rsc): Could use getrlimit here,
            // like on FreeBSD or Linux, but Darwin doesn't enforce
            // ulimit -v, so it's unclear why we'd try to stay within
            // the limit.
            return 0L;
        }

        private static readonly long _NSIG = 32L;
        private static readonly long _SI_USER = 0L; /* empirically true, but not what headers say */
        private static readonly long _SIG_BLOCK = 1L;
        private static readonly long _SIG_UNBLOCK = 2L;
        private static readonly long _SIG_SETMASK = 3L;
        private static readonly long _SS_DISABLE = 4L;

        //go:noescape
        private static void sigprocmask(int how, ref sigset @new, ref sigset old)
;

        //go:noescape
        private static void sigaction(uint mode, ref sigactiont @new, ref usigactiont old)
;

        //go:noescape
        private static void sigaltstack(ref stackt @new, ref stackt old)
;

        // darwin/arm64 uses registers instead of stack-based arguments.
        // TODO: does this matter?
        private static void sigtramp(System.UIntPtr fn, uint infostyle, uint sig, ref siginfo info, unsafe.Pointer ctx)
;

        //go:noescape
        private static void setitimer(int mode, ref itimerval @new, ref itimerval old)
;

        private static void raise(uint sig)
;
        private static void raiseproc(uint sig)
;

        //extern SigTabTT runtime·sigtab[];

        private partial struct sigset // : uint
        {
        }

        private static var sigset_all = ~sigset(0L);

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            sigactiont sa = default;
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = ~uint32(0L);
            sa.sa_tramp = @unsafe.Pointer(funcPC(sigtramp)) * (uintptr.Value)(@unsafe.Pointer(ref sa.__sigaction_u));

            fn;
            sigaction(i, ref sa, null);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            usigactiont osa = default;
            sigaction(i, null, ref osa);
            *(*System.UIntPtr) handler = @unsafe.Pointer(ref osa.__sigaction_u).Value;
            if (osa.sa_flags & _SA_ONSTACK != 0L)
            {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                return;
            }
            sigactiont sa = default;
            (uintptr.Value)(@unsafe.Pointer(ref sa.__sigaction_u)).Value;

            handler;
            sa.sa_tramp = @unsafe.Pointer(funcPC(sigtramp));
            sa.sa_mask = osa.sa_mask;
            sa.sa_flags = osa.sa_flags | _SA_ONSTACK;
            sigaction(i, ref sa, null);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            usigactiont sa = default;
            sigaction(i, null, ref sa);
            return @unsafe.Pointer(ref sa.__sigaction_u).Value;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ref stackt s, System.UIntPtr sp)
        {
            (uintptr.Value)(@unsafe.Pointer(ref s.ss_sp)).Value;

            sp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ref sigset mask, long i)
        {
            mask.Value |= 1L << (int)((uint32(i) - 1L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            mask.Value &= 1L << (int)((uint32(i) - 1L));
        }

        //go:linkname executablePath os.executablePath
        private static @string executablePath = default;

        private static void sysargs(int argc, ptr<ptr<byte>> argv)
        { 
            // skip over argv, envv and the first string will be the path
            var n = argc + 1L;
            while (argv_index(argv, n) != null)
            {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<<
                n++;
            }

            executablePath = gostringnocopy(argv_index(argv, n + 1L)); 

            // strip "executable_path=" prefix if available, it's added after OS X 10.11.
            const @string prefix = "executable_path=";

            if (len(executablePath) > len(prefix) && executablePath[..len(prefix)] == prefix)
            {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
                executablePath = executablePath[len(prefix)..];
            }
        }
    }
}
