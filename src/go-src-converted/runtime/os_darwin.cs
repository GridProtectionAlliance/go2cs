// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_darwin.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct mOS
        {
            public bool initialized;
            public pthreadmutex mutex;
            public pthreadcond cond;
            public long count;
        }

        private static void unimplemented(@string name)
        {
            println(name, "not implemented") * (int.val)(@unsafe.Pointer(uintptr(1231L)));

            1231L;

        }

        //go:nosplit
        private static void semacreate(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (mp.initialized)
            {
                return ;
            }

            mp.initialized = true;
            {
                var err__prev1 = err;

                var err = pthread_mutex_init(_addr_mp.mutex, null);

                if (err != 0L)
                {
                    throw("pthread_mutex_init");
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = pthread_cond_init(_addr_mp.cond, null);

                if (err != 0L)
                {
                    throw("pthread_cond_init");
                }

                err = err__prev1;

            }

        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            long start = default;
            if (ns >= 0L)
            {
                start = nanotime();
            }

            var mp = getg().m;
            pthread_mutex_lock(_addr_mp.mutex);
            while (true)
            {
                if (mp.count > 0L)
                {
                    mp.count--;
                    pthread_mutex_unlock(_addr_mp.mutex);
                    return 0L;
                }

                if (ns >= 0L)
                {
                    var spent = nanotime() - start;
                    if (spent >= ns)
                    {
                        pthread_mutex_unlock(_addr_mp.mutex);
                        return -1L;
                    }

                    ref timespec t = ref heap(out ptr<timespec> _addr_t);
                    t.setNsec(ns - spent);
                    var err = pthread_cond_timedwait_relative_np(_addr_mp.cond, _addr_mp.mutex, _addr_t);
                    if (err == _ETIMEDOUT)
                    {
                        pthread_mutex_unlock(_addr_mp.mutex);
                        return -1L;
                    }

                }
                else
                {
                    pthread_cond_wait(_addr_mp.cond, _addr_mp.mutex);
                }

            }


        }

        //go:nosplit
        private static void semawakeup(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            pthread_mutex_lock(_addr_mp.mutex);
            mp.count++;
            if (mp.count > 0L)
            {
                pthread_cond_signal(_addr_mp.cond);
            }

            pthread_mutex_unlock(_addr_mp.mutex);

        }

        // The read and write file descriptors used by the sigNote functions.
        private static int sigNoteRead = default;        private static int sigNoteWrite = default;

        // sigNoteSetup initializes an async-signal-safe note.
        //
        // The current implementation of notes on Darwin is not async-signal-safe,
        // because the functions pthread_mutex_lock, pthread_cond_signal, and
        // pthread_mutex_unlock, called by semawakeup, are not async-signal-safe.
        // There is only one case where we need to wake up a note from a signal
        // handler: the sigsend function. The signal handler code does not require
        // all the features of notes: it does not need to do a timed wait.
        // This is a separate implementation of notes, based on a pipe, that does
        // not support timed waits but is async-signal-safe.


        // sigNoteSetup initializes an async-signal-safe note.
        //
        // The current implementation of notes on Darwin is not async-signal-safe,
        // because the functions pthread_mutex_lock, pthread_cond_signal, and
        // pthread_mutex_unlock, called by semawakeup, are not async-signal-safe.
        // There is only one case where we need to wake up a note from a signal
        // handler: the sigsend function. The signal handler code does not require
        // all the features of notes: it does not need to do a timed wait.
        // This is a separate implementation of notes, based on a pipe, that does
        // not support timed waits but is async-signal-safe.
        private static void sigNoteSetup(ptr<note> _addr__p0)
        {
            ref note _p0 = ref _addr__p0.val;

            if (sigNoteRead != 0L || sigNoteWrite != 0L)
            {
                throw("duplicate sigNoteSetup");
            }

            int errno = default;
            sigNoteRead, sigNoteWrite, errno = pipe();
            if (errno != 0L)
            {
                throw("pipe failed");
            }

            closeonexec(sigNoteRead);
            closeonexec(sigNoteWrite); 

            // Make the write end of the pipe non-blocking, so that if the pipe
            // buffer is somehow full we will not block in the signal handler.
            // Leave the read end of the pipe blocking so that we will block
            // in sigNoteSleep.
            setNonblock(sigNoteWrite);

        }

        // sigNoteWakeup wakes up a thread sleeping on a note created by sigNoteSetup.
        private static void sigNoteWakeup(ptr<note> _addr__p0)
        {
            ref note _p0 = ref _addr__p0.val;

            ref byte b = ref heap(out ptr<byte> _addr_b);
            write(uintptr(sigNoteWrite), @unsafe.Pointer(_addr_b), 1L);
        }

        // sigNoteSleep waits for a note created by sigNoteSetup to be woken.
        private static void sigNoteSleep(ptr<note> _addr__p0)
        {
            ref note _p0 = ref _addr__p0.val;

            entersyscallblock();
            ref byte b = ref heap(out ptr<byte> _addr_b);
            read(sigNoteRead, @unsafe.Pointer(_addr_b), 1L);
            exitsyscall();
        }

        // BSD interface for threading.
        private static void osinit()
        { 
            // pthread_create delayed until end of goenvs so that we
            // can look at the environment first.

            ncpu = getncpu();
            physPageSize = getPageSize();

        }

        private static readonly long _CTL_HW = (long)6L;
        private static readonly long _HW_NCPU = (long)3L;
        private static readonly long _HW_PAGESIZE = (long)7L;


        private static int getncpu()
        { 
            // Use sysctl to fetch hw.ncpu.
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            ref var @out = ref heap(uint32(0L), out ptr<var> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], 2L, (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0L);
            if (ret >= 0L && int32(out) > 0L)
            {
                return int32(out);
            }

            return 1L;

        }

        private static System.UIntPtr getPageSize()
        { 
            // Use sysctl to fetch hw.pagesize.
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
            ref var @out = ref heap(uint32(0L), out ptr<var> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], 2L, (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0L);
            if (ret >= 0L && int32(out) > 0L)
            {
                return uintptr(out);
            }

            return 0L;

        }

        private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

        //go:nosplit
        private static void getRandomData(slice<byte> r)
        {
            var fd = open(_addr_urandom_dev[0L], 0L, 0L);
            var n = read(fd, @unsafe.Pointer(_addr_r[0L]), int32(len(r)));
            closefd(fd);
            extendRandom(r, int(n));
        }

        private static void goenvs()
        {
            goenvs_unix();
        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrierrec
        private static void newosproc(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            var stk = @unsafe.Pointer(mp.g0.stack.hi);
            if (false)
            {
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
            } 

            // Initialize an attribute object.
            ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);
            int err = default;
            err = pthread_attr_init(_addr_attr);
            if (err != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            } 

            // Find out OS stack size for our own stack guard.
            ref System.UIntPtr stacksize = ref heap(out ptr<System.UIntPtr> _addr_stacksize);
            if (pthread_attr_getstacksize(_addr_attr, _addr_stacksize) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

            mp.g0.stack.hi = stacksize; // for mstart
            //mSysStatInc(&memstats.stacks_sys, stacksize) //TODO: do this?

            // Tell the pthread library we won't join with this thread.
            if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            } 

            // Finally, create the thread. It starts at mstart_stub, which does some low-level
            // setup and then calls mstart.
            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
            err = pthread_create(_addr_attr, funcPC(mstart_stub), @unsafe.Pointer(mp));
            sigprocmask(_SIG_SETMASK, _addr_oset, null);
            if (err != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

        }

        // glue code to call mstart from pthread_create.
        private static void mstart_stub()
;

        // newosproc0 is a version of newosproc that can be called before the runtime
        // is initialized.
        //
        // This function is not safe to use after initialization as it does not pass an M as fnarg.
        //
        //go:nosplit
        private static void newosproc0(System.UIntPtr stacksize, System.UIntPtr fn)
        { 
            // Initialize an attribute object.
            ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);
            int err = default;
            err = pthread_attr_init(_addr_attr);
            if (err != 0L)
            {>>MARKER:FUNCTION_mstart_stub_BLOCK_PREFIX<<
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            } 

            // The caller passes in a suggested stack size,
            // from when we allocated the stack and thread ourselves,
            // without libpthread. Now that we're using libpthread,
            // we use the OS default stack size instead of the suggestion.
            // Find out that stack size for our own stack guard.
            if (pthread_attr_getstacksize(_addr_attr, _addr_stacksize) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

            g0.stack.hi = stacksize; // for mstart
            mSysStatInc(_addr_memstats.stacks_sys, stacksize); 

            // Tell the pthread library we won't join with this thread.
            if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            } 

            // Finally, create the thread. It starts at mstart_stub, which does some low-level
            // setup and then calls mstart.
            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
            err = pthread_create(_addr_attr, fn, null);
            sigprocmask(_SIG_SETMASK, _addr_oset, null);
            if (err != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
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
        private static void mpreinit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            mp.gsignal = malg(32L * 1024L); // OS X wants >= 8K
            mp.gsignal.m = mp;

        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        { 
            // The alternate signal stack is buggy on arm64.
            // The signal handler handles it directly.
            if (GOARCH != "arm64")
            {
                minitSignalStack();
            }

            minitSignalMask();
            getg().m.procid = uint64(pthread_self());

        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        { 
            // The alternate signal stack is buggy on arm64.
            // See minit.
            if (GOARCH != "arm64")
            {
                unminitSignals();
            }

        }

        //go:nosplit
        private static void osyield()
        {
            usleep(1L);
        }

        private static readonly long _NSIG = (long)32L;
        private static readonly long _SI_USER = (long)0L; /* empirically true, but not what headers say */
        private static readonly long _SIG_BLOCK = (long)1L;
        private static readonly long _SIG_UNBLOCK = (long)2L;
        private static readonly long _SIG_SETMASK = (long)3L;
        private static readonly long _SS_DISABLE = (long)4L;


        //extern SigTabTT runtime·sigtab[];

        private partial struct sigset // : uint
        {
        }

        private static var sigset_all = ~sigset(0L);

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = ~uint32(0L);
            if (fn == funcPC(sighandler))
            {
                if (iscgo)
                {
                    fn = funcPC(cgoSigtramp);
                }
                else
                {
                    fn = funcPC(sigtramp);
                }

            }

            (uintptr.val)(@unsafe.Pointer(_addr_sa.__sigaction_u)).val;

            fn;
            sigaction(i, _addr_sa, null);

        }

        // sigtramp is the callback from libc when a signal is received.
        // It is called with the C calling convention.
        private static void sigtramp()
;
        private static void cgoSigtramp()
;

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            ref usigactiont osa = ref heap(out ptr<usigactiont> _addr_osa);
            sigaction(i, null, _addr_osa);
            ptr<ptr<System.UIntPtr>> handler = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_osa.__sigaction_u));
            if (osa.sa_flags & _SA_ONSTACK != 0L)
            {>>MARKER:FUNCTION_cgoSigtramp_BLOCK_PREFIX<<
                return ;
            }

            ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
            (uintptr.val)(@unsafe.Pointer(_addr_sa.__sigaction_u)).val;

            handler;
            sa.sa_mask = osa.sa_mask;
            sa.sa_flags = osa.sa_flags | _SA_ONSTACK;
            sigaction(i, _addr_sa, null);

        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
            sigaction(i, null, _addr_sa);
            return new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_sa.__sigaction_u));
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ptr<stackt> _addr_s, System.UIntPtr sp)
        {
            ref stackt s = ref _addr_s.val;

            (uintptr.val)(@unsafe.Pointer(_addr_s.ss_sp)).val;

            sp;

        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            mask |= 1L << (int)((uint32(i) - 1L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            mask &= 1L << (int)((uint32(i) - 1L));
        }

        //go:linkname executablePath os.executablePath
        private static @string executablePath = default;

        private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv)
        {
            ref ptr<byte> argv = ref _addr_argv.val;
 
            // skip over argv, envv and the first string will be the path
            var n = argc + 1L;
            while (argv_index(argv, n) != null)
            {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
                n++;
            }

            executablePath = gostringnocopy(argv_index(argv, n + 1L)); 

            // strip "executable_path=" prefix if available, it's added after OS X 10.11.
            const @string prefix = (@string)"executable_path=";

            if (len(executablePath) > len(prefix) && executablePath[..len(prefix)] == prefix)
            {
                executablePath = executablePath[len(prefix)..];
            }

        }

        private static void signalM(ptr<m> _addr_mp, long sig)
        {
            ref m mp = ref _addr_mp.val;

            pthread_kill(pthread(mp.procid), uint32(sig));
        }
    }
}
