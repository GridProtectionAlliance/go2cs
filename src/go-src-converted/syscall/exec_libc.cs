// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix solaris

// This file handles forkAndExecInChild function for OS using libc syscall like AIX or Solaris.

// package syscall -- go2cs converted at 2020 October 08 03:26:26 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_libc.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public partial struct SysProcAttr
        {
            public @string Chroot; // Chroot.
            public ptr<Credential> Credential; // Credential.
            public bool Setsid; // Create session.
// Setpgid sets the process group ID of the child to Pgid,
// or, if Pgid == 0, to the new child's process ID.
            public bool Setpgid; // Setctty sets the controlling terminal of the child to
// file descriptor Ctty. Ctty must be a descriptor number
// in the child process: an index into ProcAttr.Files.
// This is only meaningful if Setsid is true.
            public bool Setctty;
            public bool Noctty; // Detach fd 0 from controlling terminal
            public long Ctty; // Controlling TTY fd
// Foreground places the child process group in the foreground.
// This implies Setpgid. The Ctty field must be set to
// the descriptor of the controlling TTY.
// Unlike Setctty, in this case Ctty must be a descriptor
// number in the parent process.
            public bool Foreground;
            public long Pgid; // Child's process group ID if Setpgid.
        }

        // Implemented in runtime package.
        private static void runtime_BeforeFork()
;
        private static void runtime_AfterFork()
;
        private static void runtime_AfterForkInChild()
;

        private static Errno chdir(System.UIntPtr path)
;
        private static Errno chroot1(System.UIntPtr path)
;
        private static Errno close(System.UIntPtr fd)
;
        private static (System.UIntPtr, Errno) dup2child(System.UIntPtr old, System.UIntPtr @new)
;
        private static Errno execve(System.UIntPtr path, System.UIntPtr argv, System.UIntPtr envp)
;
        private static void exit(System.UIntPtr code)
;
        private static (System.UIntPtr, Errno) fcntl1(System.UIntPtr fd, System.UIntPtr cmd, System.UIntPtr arg)
;
        private static (System.UIntPtr, Errno) forkx(System.UIntPtr flags)
;
        private static (System.UIntPtr, Errno) getpid()
;
        private static Errno ioctl(System.UIntPtr fd, System.UIntPtr req, System.UIntPtr arg)
;
        private static Errno setgid(System.UIntPtr gid)
;
        private static Errno setgroups1(System.UIntPtr ngid, System.UIntPtr gid)
;
        private static (System.UIntPtr, Errno) setsid()
;
        private static Errno setuid(System.UIntPtr uid)
;
        private static Errno setpgid(System.UIntPtr pid, System.UIntPtr pgid)
;
        private static (System.UIntPtr, Errno) write1(System.UIntPtr fd, System.UIntPtr buf, System.UIntPtr nbyte)
;

        // syscall defines this global on our behalf to avoid a build dependency on other platforms
        private static void init()
        {
            execveLibc = execve;
        }

        // Fork, dup fd onto 0..len(fd), and exec(argv0, argvv, envv) in child.
        // If a dup or exec fails, write the errno error to pipe.
        // (Pipe is close-on-exec so if exec succeeds, it will be closed.)
        // In the child, this function must not acquire any locks, because
        // they might have been locked at the time of the fork. This means
        // no rescheduling, no malloc calls, and no new stack segments.
        //
        // We call hand-crafted syscalls, implemented in
        // ../runtime/syscall_solaris.go, rather than generated libc wrappers
        // because we need to avoid lazy-loading the functions (might malloc,
        // split the stack, or acquire mutexes). We can't call RawSyscall
        // because it's not safe even for BSD-subsystem calls.
        //go:norace
        private static (long, Errno) forkAndExecInChild(ptr<byte> _addr_argv0, slice<ptr<byte>> argv, slice<ptr<byte>> envv, ptr<byte> _addr_chroot, ptr<byte> _addr_dir, ptr<ProcAttr> _addr_attr, ptr<SysProcAttr> _addr_sys, long pipe)
        {
            long pid = default;
            Errno err = default;
            ref byte argv0 = ref _addr_argv0.val;
            ref byte chroot = ref _addr_chroot.val;
            ref byte dir = ref _addr_dir.val;
            ref ProcAttr attr = ref _addr_attr.val;
            ref SysProcAttr sys = ref _addr_sys.val;
 
            // Declare all variables at top in case any
            // declarations require heap allocation (e.g., err1).
            System.UIntPtr r1 = default;            ref Errno err1 = ref heap(out ptr<Errno> _addr_err1);            long nextfd = default;            long i = default; 

            // guard against side effects of shuffling fds below.
            // Make sure that nextfd is beyond any currently open files so
            // that we can't run the risk of overwriting any of them.
            var fd = make_slice<long>(len(attr.Files));
            nextfd = len(attr.Files);
            {
                long i__prev1 = i;

                foreach (var (__i, __ufd) in attr.Files)
                {
                    i = __i;
                    ufd = __ufd;
                    if (nextfd < int(ufd))
                    {>>MARKER:FUNCTION_write1_BLOCK_PREFIX<<
                        nextfd = int(ufd);
                    }

                    fd[i] = int(ufd);

                }

                i = i__prev1;
            }

            nextfd++; 

            // About to call fork.
            // No more allocation or calls of non-assembly functions.
            runtime_BeforeFork();
            r1, err1 = forkx(0x1UL); // FORK_NOSIGCHLD
            if (err1 != 0L)
            {>>MARKER:FUNCTION_setpgid_BLOCK_PREFIX<<
                runtime_AfterFork();
                return (0L, err1);
            }

            if (r1 != 0L)
            {>>MARKER:FUNCTION_setuid_BLOCK_PREFIX<< 
                // parent; return PID
                runtime_AfterFork();
                return (int(r1), 0L);

            } 

            // Fork succeeded, now in child.
            runtime_AfterForkInChild(); 

            // Session ID
            if (sys.Setsid)
            {>>MARKER:FUNCTION_setsid_BLOCK_PREFIX<<
                _, err1 = setsid();
                if (err1 != 0L)
                {>>MARKER:FUNCTION_setgroups1_BLOCK_PREFIX<<
                    goto childerror;
                }

            } 

            // Set process group
            if (sys.Setpgid || sys.Foreground)
            {>>MARKER:FUNCTION_setgid_BLOCK_PREFIX<< 
                // Place child in process group.
                err1 = setpgid(0L, uintptr(sys.Pgid));
                if (err1 != 0L)
                {>>MARKER:FUNCTION_ioctl_BLOCK_PREFIX<<
                    goto childerror;
                }

            }

            if (sys.Foreground)
            {>>MARKER:FUNCTION_getpid_BLOCK_PREFIX<<
                ref var pgrp = ref heap(_Pid_t(sys.Pgid), out ptr<var> _addr_pgrp);
                if (pgrp == 0L)
                {>>MARKER:FUNCTION_forkx_BLOCK_PREFIX<<
                    r1, err1 = getpid();
                    if (err1 != 0L)
                    {>>MARKER:FUNCTION_fcntl1_BLOCK_PREFIX<<
                        goto childerror;
                    }

                    pgrp = _Pid_t(r1);

                } 

                // Place process group in foreground.
                err1 = ioctl(uintptr(sys.Ctty), uintptr(TIOCSPGRP), uintptr(@unsafe.Pointer(_addr_pgrp)));
                if (err1 != 0L)
                {>>MARKER:FUNCTION_exit_BLOCK_PREFIX<<
                    goto childerror;
                }

            } 

            // Chroot
            if (chroot != null)
            {>>MARKER:FUNCTION_execve_BLOCK_PREFIX<<
                err1 = chroot1(uintptr(@unsafe.Pointer(chroot)));
                if (err1 != 0L)
                {>>MARKER:FUNCTION_dup2child_BLOCK_PREFIX<<
                    goto childerror;
                }

            } 

            // User and groups
            {
                var cred = sys.Credential;

                if (cred != null)
                {>>MARKER:FUNCTION_close_BLOCK_PREFIX<<
                    var ngroups = uintptr(len(cred.Groups));
                    var groups = uintptr(0L);
                    if (ngroups > 0L)
                    {>>MARKER:FUNCTION_chroot1_BLOCK_PREFIX<<
                        groups = uintptr(@unsafe.Pointer(_addr_cred.Groups[0L]));
                    }

                    if (!cred.NoSetGroups)
                    {>>MARKER:FUNCTION_chdir_BLOCK_PREFIX<<
                        err1 = setgroups1(ngroups, groups);
                        if (err1 != 0L)
                        {>>MARKER:FUNCTION_runtime_AfterForkInChild_BLOCK_PREFIX<<
                            goto childerror;
                        }

                    }

                    err1 = setgid(uintptr(cred.Gid));
                    if (err1 != 0L)
                    {>>MARKER:FUNCTION_runtime_AfterFork_BLOCK_PREFIX<<
                        goto childerror;
                    }

                    err1 = setuid(uintptr(cred.Uid));
                    if (err1 != 0L)
                    {>>MARKER:FUNCTION_runtime_BeforeFork_BLOCK_PREFIX<<
                        goto childerror;
                    }

                } 

                // Chdir

            } 

            // Chdir
            if (dir != null)
            {
                err1 = chdir(uintptr(@unsafe.Pointer(dir)));
                if (err1 != 0L)
                {
                    goto childerror;
                }

            } 

            // Pass 1: look for fd[i] < i and move those up above len(fd)
            // so that pass 2 won't stomp on an fd it needs later.
            if (pipe < nextfd)
            {
                _, err1 = dup2child(uintptr(pipe), uintptr(nextfd));
                if (err1 != 0L)
                {
                    goto childerror;
                }

                fcntl1(uintptr(nextfd), F_SETFD, FD_CLOEXEC);
                pipe = nextfd;
                nextfd++;

            }

            for (i = 0L; i < len(fd); i++)
            {
                if (fd[i] >= 0L && fd[i] < int(i))
                {
                    if (nextfd == pipe)
                    { // don't stomp on pipe
                        nextfd++;

                    }

                    _, err1 = dup2child(uintptr(fd[i]), uintptr(nextfd));
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }

                    _, err1 = fcntl1(uintptr(nextfd), F_SETFD, FD_CLOEXEC);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }

                    fd[i] = nextfd;
                    nextfd++;

                }

            } 

            // Pass 2: dup fd[i] down onto i.
 

            // Pass 2: dup fd[i] down onto i.
            for (i = 0L; i < len(fd); i++)
            {
                if (fd[i] == -1L)
                {
                    close(uintptr(i));
                    continue;
                }

                if (fd[i] == int(i))
                { 
                    // dup2(i, i) won't clear close-on-exec flag on Linux,
                    // probably not elsewhere either.
                    _, err1 = fcntl1(uintptr(fd[i]), F_SETFD, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }

                    continue;

                } 
                // The new fd is created NOT close-on-exec,
                // which is exactly what we want.
                _, err1 = dup2child(uintptr(fd[i]), uintptr(i));
                if (err1 != 0L)
                {
                    goto childerror;
                }

            } 

            // By convention, we don't close-on-exec the fds we are
            // started with, so if len(fd) < 3, close 0, 1, 2 as needed.
            // Programs that know they inherit fds >= 3 will need
            // to set them close-on-exec.
 

            // By convention, we don't close-on-exec the fds we are
            // started with, so if len(fd) < 3, close 0, 1, 2 as needed.
            // Programs that know they inherit fds >= 3 will need
            // to set them close-on-exec.
            for (i = len(fd); i < 3L; i++)
            {
                close(uintptr(i));
            } 

            // Detach fd 0 from tty
 

            // Detach fd 0 from tty
            if (sys.Noctty)
            {
                err1 = ioctl(0L, uintptr(TIOCNOTTY), 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }

            } 

            // Set the controlling TTY to Ctty
            if (sys.Setctty)
            { 
                // On AIX, TIOCSCTTY is undefined
                if (TIOCSCTTY == 0L)
                {
                    err1 = ENOSYS;
                    goto childerror;
                }

                err1 = ioctl(uintptr(sys.Ctty), uintptr(TIOCSCTTY), 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }

            } 

            // Time to exec.
            err1 = execve(uintptr(@unsafe.Pointer(argv0)), uintptr(@unsafe.Pointer(_addr_argv[0L])), uintptr(@unsafe.Pointer(_addr_envv[0L])));

childerror:
            write1(uintptr(pipe), uintptr(@unsafe.Pointer(_addr_err1)), @unsafe.Sizeof(err1));
            while (true)
            {
                exit(253L);
            }


        }
    }
}
