// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains main runtime AIX syscalls.
// Pollset syscalls are in netpoll_aix.go.
// The implementation is based on Solaris and Windows.
// Each syscall is made by calling its libc symbol using asmcgocall and asmsyscall6
// assembly functions.

// package runtime -- go2cs converted at 2020 October 08 03:21:43 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os2_aix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Symbols imported for __start function.

        //go:cgo_import_dynamic libc___n_pthreads __n_pthreads "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libc___mod_init __mod_init "libc.a/shr_64.o"
        //go:linkname libc___n_pthreads libc___n_pthread
        //go:linkname libc___mod_init libc___mod_init
        private static libFunc libc___n_pthread = default;        private static libFunc libc___mod_init = default;


        // Syscalls

        //go:cgo_import_dynamic libc__Errno _Errno "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_clock_gettime clock_gettime "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_close close "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_exit exit "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_getpid getpid "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_getsystemcfg getsystemcfg "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_kill kill "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_madvise madvise "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_malloc malloc "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_mmap mmap "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_mprotect mprotect "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_munmap munmap "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_open open "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_pipe pipe "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_raise raise "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_read read "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sched_yield sched_yield "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sem_init sem_init "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sem_post sem_post "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sem_timedwait sem_timedwait "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sem_wait sem_wait "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_setitimer setitimer "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sigaction sigaction "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sigaltstack sigaltstack "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_sysconf sysconf "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_usleep usleep "libc.a/shr_64.o"
        //go:cgo_import_dynamic libc_write write "libc.a/shr_64.o"

        //go:cgo_import_dynamic libpthread___pth_init __pth_init "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_destroy pthread_attr_destroy "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_init pthread_attr_init "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_getstacksize pthread_attr_getstacksize "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_setstacksize pthread_attr_setstacksize "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_setdetachstate pthread_attr_setdetachstate "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_attr_setstackaddr pthread_attr_setstackaddr "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_create pthread_create "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_sigthreadmask sigthreadmask "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_self pthread_self "libpthread.a/shr_xpg5_64.o"
        //go:cgo_import_dynamic libpthread_kill pthread_kill "libpthread.a/shr_xpg5_64.o"

        //go:linkname libc__Errno libc__Errno
        //go:linkname libc_clock_gettime libc_clock_gettime
        //go:linkname libc_close libc_close
        //go:linkname libc_exit libc_exit
        //go:linkname libc_getpid libc_getpid
        //go:linkname libc_getsystemcfg libc_getsystemcfg
        //go:linkname libc_kill libc_kill
        //go:linkname libc_madvise libc_madvise
        //go:linkname libc_malloc libc_malloc
        //go:linkname libc_mmap libc_mmap
        //go:linkname libc_mprotect libc_mprotect
        //go:linkname libc_munmap libc_munmap
        //go:linkname libc_open libc_open
        //go:linkname libc_pipe libc_pipe
        //go:linkname libc_raise libc_raise
        //go:linkname libc_read libc_read
        //go:linkname libc_sched_yield libc_sched_yield
        //go:linkname libc_sem_init libc_sem_init
        //go:linkname libc_sem_post libc_sem_post
        //go:linkname libc_sem_timedwait libc_sem_timedwait
        //go:linkname libc_sem_wait libc_sem_wait
        //go:linkname libc_setitimer libc_setitimer
        //go:linkname libc_sigaction libc_sigaction
        //go:linkname libc_sigaltstack libc_sigaltstack
        //go:linkname libc_sysconf libc_sysconf
        //go:linkname libc_usleep libc_usleep
        //go:linkname libc_write libc_write

        //go:linkname libpthread___pth_init libpthread___pth_init
        //go:linkname libpthread_attr_destroy libpthread_attr_destroy
        //go:linkname libpthread_attr_init libpthread_attr_init
        //go:linkname libpthread_attr_getstacksize libpthread_attr_getstacksize
        //go:linkname libpthread_attr_setstacksize libpthread_attr_setstacksize
        //go:linkname libpthread_attr_setdetachstate libpthread_attr_setdetachstate
        //go:linkname libpthread_attr_setstackaddr libpthread_attr_setstackaddr
        //go:linkname libpthread_create libpthread_create
        //go:linkname libpthread_sigthreadmask libpthread_sigthreadmask
        //go:linkname libpthread_self libpthread_self
        //go:linkname libpthread_kill libpthread_kill

 
        //libc
        private static libFunc libc__Errno = default;        private static libFunc libc_clock_gettime = default;        private static libFunc libc_close = default;        private static libFunc libc_exit = default;        private static libFunc libc_getpid = default;        private static libFunc libc_getsystemcfg = default;        private static libFunc libc_kill = default;        private static libFunc libc_madvise = default;        private static libFunc libc_malloc = default;        private static libFunc libc_mmap = default;        private static libFunc libc_mprotect = default;        private static libFunc libc_munmap = default;        private static libFunc libc_open = default;        private static libFunc libc_pipe = default;        private static libFunc libc_raise = default;        private static libFunc libc_read = default;        private static libFunc libc_sched_yield = default;        private static libFunc libc_sem_init = default;        private static libFunc libc_sem_post = default;        private static libFunc libc_sem_timedwait = default;        private static libFunc libc_sem_wait = default;        private static libFunc libc_setitimer = default;        private static libFunc libc_sigaction = default;        private static libFunc libc_sigaltstack = default;        private static libFunc libc_sysconf = default;        private static libFunc libc_usleep = default;        private static libFunc libc_write = default;        private static libFunc libpthread___pth_init = default;        private static libFunc libpthread_attr_destroy = default;        private static libFunc libpthread_attr_init = default;        private static libFunc libpthread_attr_getstacksize = default;        private static libFunc libpthread_attr_setstacksize = default;        private static libFunc libpthread_attr_setdetachstate = default;        private static libFunc libpthread_attr_setstackaddr = default;        private static libFunc libpthread_create = default;        private static libFunc libpthread_sigthreadmask = default;        private static libFunc libpthread_self = default;        private static libFunc libpthread_kill = default;


        private partial struct libFunc // : System.UIntPtr
        {
        }

        // asmsyscall6 calls the libc symbol using a C convention.
        // It's defined in sys_aix_ppc64.go.
        private static libFunc asmsyscall6 = default;

        // syscallX functions must always be called with g != nil and m != nil,
        // as it relies on g.m.libcall to pass arguments to asmcgocall.
        // The few cases where syscalls haven't a g or a m must call their equivalent
        // function in sys_aix_ppc64.s to handle them.

        //go:nowritebarrier
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall0(ptr<libFunc> _addr_fn)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:0,args:uintptr(unsafe.Pointer(&fn)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall1(ptr<libFunc> _addr_fn, System.UIntPtr a0)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:1,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        //go:cgo_unsafe_args
        private static (System.UIntPtr, System.UIntPtr) syscall2(ptr<libFunc> _addr_fn, System.UIntPtr a0, System.UIntPtr a1)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:2,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        //go:cgo_unsafe_args
        private static (System.UIntPtr, System.UIntPtr) syscall3(ptr<libFunc> _addr_fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:3,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        //go:cgo_unsafe_args
        private static (System.UIntPtr, System.UIntPtr) syscall4(ptr<libFunc> _addr_fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:4,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        //go:cgo_unsafe_args
        private static (System.UIntPtr, System.UIntPtr) syscall5(ptr<libFunc> _addr_fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:5,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        //go:nowritebarrier
        //go:nosplit
        //go:cgo_unsafe_args
        private static (System.UIntPtr, System.UIntPtr) syscall6(ptr<libFunc> _addr_fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            System.UIntPtr r = default;
            System.UIntPtr err = default;
            ref libFunc fn = ref _addr_fn.val;

            var gp = getg();
            var mp = gp.m;
            var resetLibcall = true;
            if (mp.libcallsp == 0L)
            {
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();

            }
            else
            {
                resetLibcall = false; // See comment in sys_darwin.go:libcCall
            }

            ref libcall c = ref heap(new libcall(fn:uintptr(unsafe.Pointer(fn)),n:6,args:uintptr(unsafe.Pointer(&a0)),), out ptr<libcall> _addr_c);

            asmcgocall(@unsafe.Pointer(_addr_asmsyscall6), @unsafe.Pointer(_addr_c));

            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return (c.r1, c.err);

        }

        private static void exit1(int code)
;

        //go:nosplit
        private static void exit(int code)
        {
            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_exit1_BLOCK_PREFIX<<
                syscall1(_addr_libc_exit, uintptr(code));
                return ;
            }

            exit1(code);

        }

        private static int write2(System.UIntPtr fd, System.UIntPtr p, int n)
;

        //go:nosplit
        private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n)
        {
            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_write2_BLOCK_PREFIX<<
                var (r, errno) = syscall3(_addr_libc_write, uintptr(fd), uintptr(p), uintptr(n));
                if (int32(r) < 0L)
                {
                    return -int32(errno);
                }

                return int32(r);

            } 
            // Note that in this case we can't return a valid errno value.
            return write2(fd, uintptr(p), n);


        }

        //go:nosplit
        private static int read(int fd, unsafe.Pointer p, int n)
        {
            var (r, errno) = syscall3(_addr_libc_read, uintptr(fd), uintptr(p), uintptr(n));
            if (int32(r) < 0L)
            {
                return -int32(errno);
            }

            return int32(r);

        }

        //go:nosplit
        private static int open(ptr<byte> _addr_name, int mode, int perm)
        {
            ref byte name = ref _addr_name.val;

            var (r, _) = syscall3(_addr_libc_open, uintptr(@unsafe.Pointer(name)), uintptr(mode), uintptr(perm));
            return int32(r);
        }

        //go:nosplit
        private static int closefd(int fd)
        {
            var (r, _) = syscall1(_addr_libc_close, uintptr(fd));
            return int32(r);
        }

        //go:nosplit
        private static (int, int, int) pipe()
        {
            int r = default;
            int w = default;
            int errno = default;

            array<int> p = new array<int>(2L);
            var (_, err) = syscall1(_addr_libc_pipe, uintptr(noescape(@unsafe.Pointer(_addr_p[0L]))));
            return (p[0L], p[1L], int32(err));
        }

        // mmap calls the mmap system call.
        // We only pass the lower 32 bits of file offset to the
        // assembly routine; the higher bits (if required), should be provided
        // by the assembly routine as 0.
        // The err result is an OS error code such as ENOMEM.
        //go:nosplit
        private static (unsafe.Pointer, long) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
        {
            unsafe.Pointer _p0 = default;
            long _p0 = default;

            var (r, err0) = syscall6(_addr_libc_mmap, uintptr(addr), uintptr(n), uintptr(prot), uintptr(flags), uintptr(fd), uintptr(off));
            if (r == ~uintptr(0L))
            {
                return (null, int(err0));
            }

            return (@unsafe.Pointer(r), int(err0));

        }

        //go:nosplit
        private static (unsafe.Pointer, long) mprotect(unsafe.Pointer addr, System.UIntPtr n, int prot)
        {
            unsafe.Pointer _p0 = default;
            long _p0 = default;

            var (r, err0) = syscall3(_addr_libc_mprotect, uintptr(addr), uintptr(n), uintptr(prot));
            if (r == ~uintptr(0L))
            {
                return (null, int(err0));
            }

            return (@unsafe.Pointer(r), int(err0));

        }

        //go:nosplit
        private static void munmap(unsafe.Pointer addr, System.UIntPtr n)
        {
            var (r, err) = syscall2(_addr_libc_munmap, uintptr(addr), uintptr(n));
            if (int32(r) == -1L)
            {
                println("syscall munmap failed: ", hex(err));
                throw("syscall munmap");
            }

        }

        //go:nosplit
        private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags)
        {
            var (r, err) = syscall3(_addr_libc_madvise, uintptr(addr), uintptr(n), uintptr(flags));
            if (int32(r) == -1L)
            {
                println("syscall madvise failed: ", hex(err));
                throw("syscall madvise");
            }

        }

        private static void sigaction1(System.UIntPtr sig, System.UIntPtr @new, System.UIntPtr old)
;

        //go:nosplit
        private static void sigaction(System.UIntPtr sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old)
        {
            ref sigactiont @new = ref _addr_@new.val;
            ref sigactiont old = ref _addr_old.val;

            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // runtime.libpreinit.
            if (_g_ != null)
            {>>MARKER:FUNCTION_sigaction1_BLOCK_PREFIX<<
                var (r, err) = syscall3(_addr_libc_sigaction, sig, uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));
                if (int32(r) == -1L)
                {
                    println("Sigaction failed for sig: ", sig, " with error:", hex(err));
                    throw("syscall sigaction");
                }

                return ;

            }

            sigaction1(sig, uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));

        }

        //go:nosplit
        private static void sigaltstack(ptr<stackt> _addr_@new, ptr<stackt> _addr_old)
        {
            ref stackt @new = ref _addr_@new.val;
            ref stackt old = ref _addr_old.val;

            var (r, err) = syscall2(_addr_libc_sigaltstack, uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));
            if (int32(r) == -1L)
            {
                println("syscall sigaltstack failed: ", hex(err));
                throw("syscall sigaltstack");
            }

        }

        //go:nosplit
        private static System.UIntPtr getsystemcfg(ulong label)
        {
            var (r, _) = syscall1(_addr_libc_getsystemcfg, uintptr(label));
            return r;
        }

        private static void usleep1(uint us)
;

        //go:nosplit
        private static void usleep(uint us)
        {
            var _g_ = getg(); 

            // Check the validity of m because we might be called in cgo callback
            // path early enough where there isn't a g or a m available yet.
            if (_g_ != null && _g_.m != null)
            {>>MARKER:FUNCTION_usleep1_BLOCK_PREFIX<<
                var (r, err) = syscall1(_addr_libc_usleep, uintptr(us));
                if (int32(r) == -1L)
                {
                    println("syscall usleep failed: ", hex(err));
                    throw("syscall usleep");
                }

                return ;

            }

            usleep1(us);

        }

        //go:nosplit
        private static int clock_gettime(int clockid, ptr<timespec> _addr_tp)
        {
            ref timespec tp = ref _addr_tp.val;

            var (r, _) = syscall2(_addr_libc_clock_gettime, uintptr(clockid), uintptr(@unsafe.Pointer(tp)));
            return int32(r);
        }

        //go:nosplit
        private static void setitimer(int mode, ptr<itimerval> _addr_@new, ptr<itimerval> _addr_old)
        {
            ref itimerval @new = ref _addr_@new.val;
            ref itimerval old = ref _addr_old.val;

            var (r, err) = syscall3(_addr_libc_setitimer, uintptr(mode), uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));
            if (int32(r) == -1L)
            {
                println("syscall setitimer failed: ", hex(err));
                throw("syscall setitimer");
            }

        }

        //go:nosplit
        private static unsafe.Pointer malloc(System.UIntPtr size)
        {
            var (r, _) = syscall1(_addr_libc_malloc, size);
            return @unsafe.Pointer(r);
        }

        //go:nosplit
        private static int sem_init(ptr<semt> _addr_sem, int pshared, uint value)
        {
            ref semt sem = ref _addr_sem.val;

            var (r, _) = syscall3(_addr_libc_sem_init, uintptr(@unsafe.Pointer(sem)), uintptr(pshared), uintptr(value));
            return int32(r);
        }

        //go:nosplit
        private static (int, int) sem_wait(ptr<semt> _addr_sem)
        {
            int _p0 = default;
            int _p0 = default;
            ref semt sem = ref _addr_sem.val;

            var (r, err) = syscall1(_addr_libc_sem_wait, uintptr(@unsafe.Pointer(sem)));
            return (int32(r), int32(err));
        }

        //go:nosplit
        private static int sem_post(ptr<semt> _addr_sem)
        {
            ref semt sem = ref _addr_sem.val;

            var (r, _) = syscall1(_addr_libc_sem_post, uintptr(@unsafe.Pointer(sem)));
            return int32(r);
        }

        //go:nosplit
        private static (int, int) sem_timedwait(ptr<semt> _addr_sem, ptr<timespec> _addr_timeout)
        {
            int _p0 = default;
            int _p0 = default;
            ref semt sem = ref _addr_sem.val;
            ref timespec timeout = ref _addr_timeout.val;

            var (r, err) = syscall2(_addr_libc_sem_timedwait, uintptr(@unsafe.Pointer(sem)), uintptr(@unsafe.Pointer(timeout)));
            return (int32(r), int32(err));
        }

        //go:nosplit
        private static void raise(uint sig)
        {
            var (r, err) = syscall1(_addr_libc_raise, uintptr(sig));
            if (int32(r) == -1L)
            {
                println("syscall raise failed: ", hex(err));
                throw("syscall raise");
            }

        }

        //go:nosplit
        private static void raiseproc(uint sig)
        {
            var (pid, err) = syscall0(_addr_libc_getpid);
            if (int32(pid) == -1L)
            {
                println("syscall getpid failed: ", hex(err));
                throw("syscall raiseproc");
            }

            syscall2(_addr_libc_kill, pid, uintptr(sig));

        }

        private static void osyield1()
;

        //go:nosplit
        private static void osyield()
        {
            var _g_ = getg(); 

            // Check the validity of m because it might be called during a cgo
            // callback early enough where m isn't available yet.
            if (_g_ != null && _g_.m != null)
            {>>MARKER:FUNCTION_osyield1_BLOCK_PREFIX<<
                var (r, err) = syscall0(_addr_libc_sched_yield);
                if (int32(r) == -1L)
                {
                    println("syscall osyield failed: ", hex(err));
                    throw("syscall osyield");
                }

                return ;

            }

            osyield1();

        }

        //go:nosplit
        private static System.UIntPtr sysconf(int name)
        {
            var (r, _) = syscall1(_addr_libc_sysconf, uintptr(name));
            if (int32(r) == -1L)
            {
                throw("syscall sysconf");
            }

            return r;


        }

        // pthread functions returns its error code in the main return value
        // Therefore, err returns by syscall means nothing and must not be used

        //go:nosplit
        private static int pthread_attr_destroy(ptr<pthread_attr> _addr_attr)
        {
            ref pthread_attr attr = ref _addr_attr.val;

            var (r, _) = syscall1(_addr_libpthread_attr_destroy, uintptr(@unsafe.Pointer(attr)));
            return int32(r);
        }

        private static int pthread_attr_init1(System.UIntPtr attr)
;

        //go:nosplit
        private static int pthread_attr_init(ptr<pthread_attr> _addr_attr)
        {
            ref pthread_attr attr = ref _addr_attr.val;

            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_pthread_attr_init1_BLOCK_PREFIX<<
                var (r, _) = syscall1(_addr_libpthread_attr_init, uintptr(@unsafe.Pointer(attr)));
                return int32(r);
            }

            return pthread_attr_init1(uintptr(@unsafe.Pointer(attr)));

        }

        private static int pthread_attr_setdetachstate1(System.UIntPtr attr, int state)
;

        //go:nosplit
        private static int pthread_attr_setdetachstate(ptr<pthread_attr> _addr_attr, int state)
        {
            ref pthread_attr attr = ref _addr_attr.val;

            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_pthread_attr_setdetachstate1_BLOCK_PREFIX<<
                var (r, _) = syscall2(_addr_libpthread_attr_setdetachstate, uintptr(@unsafe.Pointer(attr)), uintptr(state));
                return int32(r);
            }

            return pthread_attr_setdetachstate1(uintptr(@unsafe.Pointer(attr)), state);

        }

        //go:nosplit
        private static int pthread_attr_setstackaddr(ptr<pthread_attr> _addr_attr, unsafe.Pointer stk)
        {
            ref pthread_attr attr = ref _addr_attr.val;

            var (r, _) = syscall2(_addr_libpthread_attr_setstackaddr, uintptr(@unsafe.Pointer(attr)), uintptr(stk));
            return int32(r);
        }

        //go:nosplit
        private static int pthread_attr_getstacksize(ptr<pthread_attr> _addr_attr, ptr<ulong> _addr_size)
        {
            ref pthread_attr attr = ref _addr_attr.val;
            ref ulong size = ref _addr_size.val;

            var (r, _) = syscall2(_addr_libpthread_attr_getstacksize, uintptr(@unsafe.Pointer(attr)), uintptr(@unsafe.Pointer(size)));
            return int32(r);
        }

        private static int pthread_attr_setstacksize1(System.UIntPtr attr, ulong size)
;

        //go:nosplit
        private static int pthread_attr_setstacksize(ptr<pthread_attr> _addr_attr, ulong size)
        {
            ref pthread_attr attr = ref _addr_attr.val;

            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_pthread_attr_setstacksize1_BLOCK_PREFIX<<
                var (r, _) = syscall2(_addr_libpthread_attr_setstacksize, uintptr(@unsafe.Pointer(attr)), uintptr(size));
                return int32(r);
            }

            return pthread_attr_setstacksize1(uintptr(@unsafe.Pointer(attr)), size);

        }

        private static int pthread_create1(System.UIntPtr tid, System.UIntPtr attr, System.UIntPtr fn, System.UIntPtr arg)
;

        //go:nosplit
        private static int pthread_create(ptr<pthread> _addr_tid, ptr<pthread_attr> _addr_attr, ptr<funcDescriptor> _addr_fn, unsafe.Pointer arg)
        {
            ref pthread tid = ref _addr_tid.val;
            ref pthread_attr attr = ref _addr_attr.val;
            ref funcDescriptor fn = ref _addr_fn.val;

            var _g_ = getg(); 

            // Check the validity of g because without a g during
            // newosproc0.
            if (_g_ != null)
            {>>MARKER:FUNCTION_pthread_create1_BLOCK_PREFIX<<
                var (r, _) = syscall4(_addr_libpthread_create, uintptr(@unsafe.Pointer(tid)), uintptr(@unsafe.Pointer(attr)), uintptr(@unsafe.Pointer(fn)), uintptr(arg));
                return int32(r);
            }

            return pthread_create1(uintptr(@unsafe.Pointer(tid)), uintptr(@unsafe.Pointer(attr)), uintptr(@unsafe.Pointer(fn)), uintptr(arg));

        }

        // On multi-thread program, sigprocmask must not be called.
        // It's replaced by sigthreadmask.
        private static void sigprocmask1(System.UIntPtr how, System.UIntPtr @new, System.UIntPtr old)
;

        //go:nosplit
        private static void sigprocmask(int how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old)
        {
            ref sigset @new = ref _addr_@new.val;
            ref sigset old = ref _addr_old.val;

            var _g_ = getg(); 

            // Check the validity of m because it might be called during a cgo
            // callback early enough where m isn't available yet.
            if (_g_ != null && _g_.m != null)
            {>>MARKER:FUNCTION_sigprocmask1_BLOCK_PREFIX<<
                var (r, err) = syscall3(_addr_libpthread_sigthreadmask, uintptr(how), uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));
                if (int32(r) != 0L)
                {
                    println("syscall sigthreadmask failed: ", hex(err));
                    throw("syscall sigthreadmask");
                }

                return ;

            }

            sigprocmask1(uintptr(how), uintptr(@unsafe.Pointer(new)), uintptr(@unsafe.Pointer(old)));


        }

        //go:nosplit
        private static pthread pthread_self()
        {
            var (r, _) = syscall0(_addr_libpthread_self);
            return pthread(r);
        }

        //go:nosplit
        private static void signalM(ptr<m> _addr_mp, long sig)
        {
            ref m mp = ref _addr_mp.val;

            syscall2(_addr_libpthread_kill, uintptr(pthread(mp.procid)), uintptr(sig));
        }
    }
}
