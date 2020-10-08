// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Fork, exec, wait, etc.

// package syscall -- go2cs converted at 2020 October 08 03:26:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_unix.go
using errorspkg = go.errors_package;
using bytealg = go.@internal.bytealg_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        // Lock synchronizing creation of new file descriptors with fork.
        //
        // We want the child in a fork/exec sequence to inherit only the
        // file descriptors we intend. To do that, we mark all file
        // descriptors close-on-exec and then, in the child, explicitly
        // unmark the ones we want the exec'ed program to keep.
        // Unix doesn't make this easy: there is, in general, no way to
        // allocate a new file descriptor close-on-exec. Instead you
        // have to allocate the descriptor and then mark it close-on-exec.
        // If a fork happens between those two events, the child's exec
        // will inherit an unwanted file descriptor.
        //
        // This lock solves that race: the create new fd/mark close-on-exec
        // operation is done holding ForkLock for reading, and the fork itself
        // is done holding ForkLock for writing. At least, that's the idea.
        // There are some complications.
        //
        // Some system calls that create new file descriptors can block
        // for arbitrarily long times: open on a hung NFS server or named
        // pipe, accept on a socket, and so on. We can't reasonably grab
        // the lock across those operations.
        //
        // It is worse to inherit some file descriptors than others.
        // If a non-malicious child accidentally inherits an open ordinary file,
        // that's not a big deal. On the other hand, if a long-lived child
        // accidentally inherits the write end of a pipe, then the reader
        // of that pipe will not see EOF until that child exits, potentially
        // causing the parent program to hang. This is a common problem
        // in threaded C programs that use popen.
        //
        // Luckily, the file descriptors that are most important not to
        // inherit are not the ones that can take an arbitrarily long time
        // to create: pipe returns instantly, and the net package uses
        // non-blocking I/O to accept on a listening socket.
        // The rules for which file descriptor-creating operations use the
        // ForkLock are as follows:
        //
        // 1) Pipe. Does not block. Use the ForkLock.
        // 2) Socket. Does not block. Use the ForkLock.
        // 3) Accept. If using non-blocking mode, use the ForkLock.
        //             Otherwise, live with the race.
        // 4) Open. Can block. Use O_CLOEXEC if available (Linux).
        //             Otherwise, live with the race.
        // 5) Dup. Does not block. Use the ForkLock.
        //             On Linux, could use fcntl F_DUPFD_CLOEXEC
        //             instead of the ForkLock, but only for dup(fd, -1).
        public static sync.RWMutex ForkLock = default;

        // StringSlicePtr converts a slice of strings to a slice of pointers
        // to NUL-terminated byte arrays. If any string contains a NUL byte
        // this function panics instead of returning an error.
        //
        // Deprecated: Use SlicePtrFromStrings instead.
        public static slice<ptr<byte>> StringSlicePtr(slice<@string> ss)
        {
            var bb = make_slice<ptr<byte>>(len(ss) + 1L);
            for (long i = 0L; i < len(ss); i++)
            {
                bb[i] = StringBytePtr(ss[i]);
            }

            bb[len(ss)] = null;
            return bb;

        }

        // SlicePtrFromStrings converts a slice of strings to a slice of
        // pointers to NUL-terminated byte arrays. If any string contains
        // a NUL byte, it returns (nil, EINVAL).
        public static (slice<ptr<byte>>, error) SlicePtrFromStrings(slice<@string> ss)
        {
            slice<ptr<byte>> _p0 = default;
            error _p0 = default!;

            long n = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ss)
                {
                    s = __s;
                    if (bytealg.IndexByteString(s, 0L) != -1L)
                    {
                        return (null, error.As(EINVAL)!);
                    }

                    n += len(s) + 1L; // +1 for NUL
                }

                s = s__prev1;
            }

            var bb = make_slice<ptr<byte>>(len(ss) + 1L);
            var b = make_slice<byte>(n);
            n = 0L;
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in ss)
                {
                    i = __i;
                    s = __s;
                    bb[i] = _addr_b[n];
                    copy(b[n..], s);
                    n += len(s) + 1L;
                }

                s = s__prev1;
            }

            return (bb, error.As(null!)!);

        }

        public static void CloseOnExec(long fd)
        {
            fcntl(fd, F_SETFD, FD_CLOEXEC);
        }

        public static error SetNonblock(long fd, bool nonblocking)
        {
            error err = default!;

            var (flag, err) = fcntl(fd, F_GETFL, 0L);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (nonblocking)
            {
                flag |= O_NONBLOCK;
            }
            else
            {
                flag &= O_NONBLOCK;
            }

            _, err = fcntl(fd, F_SETFL, flag);
            return error.As(err)!;

        }

        // Credential holds user and group identities to be assumed
        // by a child process started by StartProcess.
        public partial struct Credential
        {
            public uint Uid; // User ID.
            public uint Gid; // Group ID.
            public slice<uint> Groups; // Supplementary group IDs.
            public bool NoSetGroups; // If true, don't set supplementary groups
        }

        // ProcAttr holds attributes that will be applied to a new process started
        // by StartProcess.
        public partial struct ProcAttr
        {
            public @string Dir; // Current working directory.
            public slice<@string> Env; // Environment.
            public slice<System.UIntPtr> Files; // File descriptors.
            public ptr<SysProcAttr> Sys;
        }

        private static ProcAttr zeroProcAttr = default;
        private static SysProcAttr zeroSysProcAttr = default;

        private static (long, error) forkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            array<long> p = new array<long>(2L);
            long n = default;
            ref Errno err1 = ref heap(out ptr<Errno> _addr_err1);
            ref WaitStatus wstatus = ref heap(out ptr<WaitStatus> _addr_wstatus);

            if (attr == null)
            {
                attr = _addr_zeroProcAttr;
            }

            var sys = attr.Sys;
            if (sys == null)
            {
                sys = _addr_zeroSysProcAttr;
            }

            p[0L] = -1L;
            p[1L] = -1L; 

            // Convert args to C form.
            var (argv0p, err) = BytePtrFromString(argv0);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var (argvp, err) = SlicePtrFromStrings(argv);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            var (envvp, err) = SlicePtrFromStrings(attr.Env);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if ((runtime.GOOS == "freebsd" || runtime.GOOS == "dragonfly") && len(argv[0L]) > len(argv0))
            {
                argvp[0L] = argv0p;
            }

            ptr<byte> chroot;
            if (sys.Chroot != "")
            {
                chroot, err = BytePtrFromString(sys.Chroot);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            ptr<byte> dir;
            if (attr.Dir != "")
            {
                dir, err = BytePtrFromString(attr.Dir);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            } 

            // Both Setctty and Foreground use the Ctty field,
            // but they give it slightly different meanings.
            if (sys.Setctty && sys.Foreground)
            {
                return (0L, error.As(errorspkg.New("both Setctty and Foreground set in SysProcAttr"))!);
            }

            if (sys.Setctty && sys.Ctty >= len(attr.Files))
            {
                return (0L, error.As(errorspkg.New("Setctty set but Ctty not valid in child"))!);
            } 

            // Acquire the fork lock so that no other threads
            // create new fds that are not yet close-on-exec
            // before we fork.
            ForkLock.Lock(); 

            // Allocate child status pipe close on exec.
            err = forkExecPipe(p[..]);

            if (err != null)
            {
                goto error;
            } 

            // Kick off child.
            pid, err1 = forkAndExecInChild(argv0p, argvp, envvp, chroot, dir, attr, sys, p[1L]);
            if (err1 != 0L)
            {
                err = Errno(err1);
                goto error;
            }

            ForkLock.Unlock(); 

            // Read child error status from pipe.
            Close(p[1L]);
            while (true)
            {
                n, err = readlen(p[0L], (byte.val)(@unsafe.Pointer(_addr_err1)), int(@unsafe.Sizeof(err1)));
                if (err != EINTR)
                {
                    break;
                }

            }

            Close(p[0L]);
            if (err != null || n != 0L)
            {
                if (n == int(@unsafe.Sizeof(err1)))
                {
                    err = Errno(err1);
                }

                if (err == null)
                {
                    err = EPIPE;
                } 

                // Child failed; wait for it to exit, to make sure
                // the zombies don't accumulate.
                var (_, err1) = Wait4(pid, _addr_wstatus, 0L, null);
                while (err1 == EINTR)
                {
                    _, err1 = Wait4(pid, _addr_wstatus, 0L, null);
                }

                return (0L, error.As(err)!);

            } 

            // Read got EOF, so pipe closed on exec, so exec succeeded.
            return (pid, error.As(null!)!);

error:
            if (p[0L] >= 0L)
            {
                Close(p[0L]);
                Close(p[1L]);
            }

            ForkLock.Unlock();
            return (0L, error.As(err)!);

        }

        // Combination of fork and exec, careful to be thread safe.
        public static (long, error) ForkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            return forkExec(argv0, argv, _addr_attr);
        }

        // StartProcess wraps ForkExec for package os.
        public static (long, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr)
        {
            long pid = default;
            System.UIntPtr handle = default;
            error err = default!;
            ref ProcAttr attr = ref _addr_attr.val;

            pid, err = forkExec(argv0, argv, _addr_attr);
            return (pid, 0L, error.As(err)!);
        }

        // Implemented in runtime package.
        private static void runtime_BeforeExec()
;
        private static void runtime_AfterExec()
;

        // execveLibc is non-nil on OS using libc syscall, set to execve in exec_libc.go; this
        // avoids a build dependency for other platforms.
        private static Func<System.UIntPtr, System.UIntPtr, System.UIntPtr, Errno> execveLibc = default;
        private static Func<ptr<byte>, ptr<ptr<byte>>, ptr<ptr<byte>>, error> execveDarwin = default;

        // Exec invokes the execve(2) system call.
        public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv)
        {
            error err = default!;

            var (argv0p, err) = BytePtrFromString(argv0);
            if (err != null)
            {>>MARKER:FUNCTION_runtime_AfterExec_BLOCK_PREFIX<<
                return error.As(err)!;
            }

            var (argvp, err) = SlicePtrFromStrings(argv);
            if (err != null)
            {>>MARKER:FUNCTION_runtime_BeforeExec_BLOCK_PREFIX<<
                return error.As(err)!;
            }

            var (envvp, err) = SlicePtrFromStrings(envv);
            if (err != null)
            {
                return error.As(err)!;
            }

            runtime_BeforeExec();

            error err1 = default!;
            if (runtime.GOOS == "solaris" || runtime.GOOS == "illumos" || runtime.GOOS == "aix")
            { 
                // RawSyscall should never be used on Solaris, illumos, or AIX.
                err1 = error.As(execveLibc(uintptr(@unsafe.Pointer(argv0p)), uintptr(@unsafe.Pointer(_addr_argvp[0L])), uintptr(@unsafe.Pointer(_addr_envvp[0L]))))!;

            }
            else if (runtime.GOOS == "darwin")
            { 
                // Similarly on Darwin.
                err1 = error.As(execveDarwin(argv0p, _addr_argvp[0L], _addr_envvp[0L]))!;

            }
            else
            {
                _, _, err1 = RawSyscall(SYS_EXECVE, uintptr(@unsafe.Pointer(argv0p)), uintptr(@unsafe.Pointer(_addr_argvp[0L])), uintptr(@unsafe.Pointer(_addr_envvp[0L])));
            }

            runtime_AfterExec();
            return error.As(err1)!;

        }
    }
}
