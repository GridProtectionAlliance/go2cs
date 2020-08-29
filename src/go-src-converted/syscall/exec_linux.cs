// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux

// package syscall -- go2cs converted at 2020 August 29 08:36:54 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_linux.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // SysProcIDMap holds Container ID to Host ID mappings used for User Namespaces in Linux.
        // See user_namespaces(7).
        public partial struct SysProcIDMap
        {
            public long ContainerID; // Container ID.
            public long HostID; // Host ID.
            public long Size; // Size.
        }

        public partial struct SysProcAttr
        {
            public @string Chroot; // Chroot.
            public ptr<Credential> Credential; // Credential.
            public bool Ptrace; // Enable tracing.
            public bool Setsid; // Create session.
            public bool Setpgid; // Set process group ID to Pgid, or, if Pgid == 0, to new pid.
            public bool Setctty; // Set controlling terminal to fd Ctty (only meaningful if Setsid is set)
            public bool Noctty; // Detach fd 0 from controlling terminal
            public long Ctty; // Controlling TTY fd
            public bool Foreground; // Place child's process group in foreground. (Implies Setpgid. Uses Ctty as fd of controlling TTY)
            public long Pgid; // Child's process group ID if Setpgid.
            public Signal Pdeathsig; // Signal that the process will get when its parent dies (Linux only)
            public System.UIntPtr Cloneflags; // Flags for clone calls (Linux only)
            public System.UIntPtr Unshareflags; // Flags for unshare calls (Linux only)
            public slice<SysProcIDMap> UidMappings; // User ID mappings for user namespaces.
            public slice<SysProcIDMap> GidMappings; // Group ID mappings for user namespaces.
// GidMappingsEnableSetgroups enabling setgroups syscall.
// If false, then setgroups syscall will be disabled for the child process.
// This parameter is no-op if GidMappings == nil. Otherwise for unprivileged
// users this should be set to false for mappings work.
            public bool GidMappingsEnableSetgroups;
            public slice<System.UIntPtr> AmbientCaps; // Ambient capabilities (Linux only)
        }

        private static array<byte> none = new array<byte>(new byte[] { 'n', 'o', 'n', 'e', 0 });        private static array<byte> slash = new array<byte>(new byte[] { '/', 0 });

        // Implemented in runtime package.
        private static void runtime_BeforeFork()
;
        private static void runtime_AfterFork()
;
        private static void runtime_AfterForkInChild()
;

        // Fork, dup fd onto 0..len(fd), and exec(argv0, argvv, envv) in child.
        // If a dup or exec fails, write the errno error to pipe.
        // (Pipe is close-on-exec so if exec succeeds, it will be closed.)
        // In the child, this function must not acquire any locks, because
        // they might have been locked at the time of the fork. This means
        // no rescheduling, no malloc calls, and no new stack segments.
        // For the same reason compiler does not race instrument it.
        // The calls to RawSyscall are okay because they are assembly
        // functions that do not grow the stack.
        //go:norace
        private static (long, Errno) forkAndExecInChild(ref byte argv0, slice<ref byte> argv, slice<ref byte> envv, ref byte chroot, ref byte dir, ref ProcAttr attr, ref SysProcAttr sys, long pipe)
        { 
            // Set up and fork. This returns immediately in the parent or
            // if there's an error.
            var (r1, err1, p, locked) = forkAndExecInChild1(argv0, argv, envv, chroot, dir, attr, sys, pipe);
            if (locked)
            {>>MARKER:FUNCTION_runtime_AfterForkInChild_BLOCK_PREFIX<<
                runtime_AfterFork();
            }
            if (err1 != 0L)
            {>>MARKER:FUNCTION_runtime_AfterFork_BLOCK_PREFIX<<
                return (0L, err1);
            } 

            // parent; return PID
            pid = int(r1);

            if (sys.UidMappings != null || sys.GidMappings != null)
            {>>MARKER:FUNCTION_runtime_BeforeFork_BLOCK_PREFIX<<
                Close(p[0L]);
                var err = writeUidGidMappings(pid, sys);
                Errno err2 = default;
                if (err != null)
                {
                    err2 = err._<Errno>();
                }
                RawSyscall(SYS_WRITE, uintptr(p[1L]), uintptr(@unsafe.Pointer(ref err2)), @unsafe.Sizeof(err2));
                Close(p[1L]);
            }
            return (pid, 0L);
        }

        // forkAndExecInChild1 implements the body of forkAndExecInChild up to
        // the parent's post-fork path. This is a separate function so we can
        // separate the child's and parent's stack frames if we're using
        // vfork.
        //
        // This is go:noinline because the point is to keep the stack frames
        // of this and forkAndExecInChild separate.
        //
        //go:noinline
        //go:norace
        private static (System.UIntPtr, Errno, array<long>, bool) forkAndExecInChild1(ref byte argv0, slice<ref byte> argv, slice<ref byte> envv, ref byte chroot, ref byte dir, ref ProcAttr attr, ref SysProcAttr sys, long pipe)
        { 
            // Defined in linux/prctl.h starting with Linux 4.3.
            const ulong PR_CAP_AMBIENT = 0x2fUL;
            const ulong PR_CAP_AMBIENT_RAISE = 0x2UL; 

            // vfork requires that the child not touch any of the parent's
            // active stack frames. Hence, the child does all post-fork
            // processing in this stack frame and never returns, while the
            // parent returns immediately from this frame and does all
            // post-fork processing in the outer frame.
            // Declare all variables at top in case any
            // declarations require heap allocation (e.g., err1).
            Errno err2 = default;            long nextfd = default;            long i = default; 

            // Record parent PID so child can test if it has died.
            var (ppid, _, _) = RawSyscall(SYS_GETPID, 0L, 0L, 0L); 

            // Guard against side effects of shuffling fds below.
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
                    {
                        nextfd = int(ufd);
                    }
                    fd[i] = int(ufd);
                }

                i = i__prev1;
            }

            nextfd++; 

            // Allocate another pipe for parent to child communication for
            // synchronizing writing of User ID/Group ID mappings.
            if (sys.UidMappings != null || sys.GidMappings != null)
            {
                {
                    var err = forkExecPipe(p[..]);

                    if (err != null)
                    {
                        err1 = err._<Errno>();
                        return;
                    }

                }
            } 

            // About to call fork.
            // No more allocation or calls of non-assembly functions.
            runtime_BeforeFork();
            locked = true;

            if (runtime.GOARCH == "amd64" && sys.Cloneflags & CLONE_NEWUSER == 0L) 
                r1, err1 = rawVforkSyscall(SYS_CLONE, uintptr(SIGCHLD | CLONE_VFORK | CLONE_VM) | sys.Cloneflags);
            else if (runtime.GOARCH == "s390x") 
                r1, _, err1 = RawSyscall6(SYS_CLONE, 0L, uintptr(SIGCHLD) | sys.Cloneflags, 0L, 0L, 0L, 0L);
            else 
                r1, _, err1 = RawSyscall6(SYS_CLONE, uintptr(SIGCHLD) | sys.Cloneflags, 0L, 0L, 0L, 0L, 0L);
                        if (err1 != 0L || r1 != 0L)
            { 
                // If we're in the parent, we must return immediately
                // so we're not in the same stack frame as the child.
                // This can at most use the return PC, which the child
                // will not modify, and the results of
                // rawVforkSyscall, which must have been written after
                // the child was replaced.
                return;
            } 

            // Fork succeeded, now in child.
            runtime_AfterForkInChild(); 

            // Enable the "keep capabilities" flag to set ambient capabilities later.
            if (len(sys.AmbientCaps) > 0L)
            {
                _, _, err1 = RawSyscall6(SYS_PRCTL, PR_SET_KEEPCAPS, 1L, 0L, 0L, 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Wait for User ID/Group ID mappings to be written.
            if (sys.UidMappings != null || sys.GidMappings != null)
            {
                _, _, err1 = RawSyscall(SYS_CLOSE, uintptr(p[1L]), 0L, 0L);

                if (err1 != 0L)
                {
                    goto childerror;
                }
                r1, _, err1 = RawSyscall(SYS_READ, uintptr(p[0L]), uintptr(@unsafe.Pointer(ref err2)), @unsafe.Sizeof(err2));
                if (err1 != 0L)
                {
                    goto childerror;
                }
                if (r1 != @unsafe.Sizeof(err2))
                {
                    err1 = EINVAL;
                    goto childerror;
                }
                if (err2 != 0L)
                {
                    err1 = err2;
                    goto childerror;
                }
            } 

            // Session ID
            if (sys.Setsid)
            {
                _, _, err1 = RawSyscall(SYS_SETSID, 0L, 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Set process group
            if (sys.Setpgid || sys.Foreground)
            { 
                // Place child in process group.
                _, _, err1 = RawSyscall(SYS_SETPGID, 0L, uintptr(sys.Pgid), 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            }
            if (sys.Foreground)
            {
                var pgrp = int32(sys.Pgid);
                if (pgrp == 0L)
                {
                    r1, _, err1 = RawSyscall(SYS_GETPID, 0L, 0L, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                    pgrp = int32(r1);
                } 

                // Place process group in foreground.
                _, _, err1 = RawSyscall(SYS_IOCTL, uintptr(sys.Ctty), uintptr(TIOCSPGRP), uintptr(@unsafe.Pointer(ref pgrp)));
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Unshare
            if (sys.Unshareflags != 0L)
            {
                _, _, err1 = RawSyscall(SYS_UNSHARE, sys.Unshareflags, 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                } 
                // The unshare system call in Linux doesn't unshare mount points
                // mounted with --shared. Systemd mounts / with --shared. For a
                // long discussion of the pros and cons of this see debian bug 739593.
                // The Go model of unsharing is more like Plan 9, where you ask
                // to unshare and the namespaces are unconditionally unshared.
                // To make this model work we must further mark / as MS_PRIVATE.
                // This is what the standard unshare command does.
                if (sys.Unshareflags & CLONE_NEWNS == CLONE_NEWNS)
                {
                    _, _, err1 = RawSyscall6(SYS_MOUNT, uintptr(@unsafe.Pointer(ref none[0L])), uintptr(@unsafe.Pointer(ref slash[0L])), 0L, MS_REC | MS_PRIVATE, 0L, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                }
            } 

            // Chroot
            if (chroot != null)
            {
                _, _, err1 = RawSyscall(SYS_CHROOT, uintptr(@unsafe.Pointer(chroot)), 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // User and groups
            {
                var cred = sys.Credential;

                if (cred != null)
                {
                    var ngroups = uintptr(len(cred.Groups));
                    var groups = uintptr(0L);
                    if (ngroups > 0L)
                    {
                        groups = uintptr(@unsafe.Pointer(ref cred.Groups[0L]));
                    }
                    if (!(sys.GidMappings != null && !sys.GidMappingsEnableSetgroups && ngroups == 0L) && !cred.NoSetGroups)
                    {
                        _, _, err1 = RawSyscall(_SYS_setgroups, ngroups, groups, 0L);
                        if (err1 != 0L)
                        {
                            goto childerror;
                        }
                    }
                    _, _, err1 = RawSyscall(sys_SETGID, uintptr(cred.Gid), 0L, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                    _, _, err1 = RawSyscall(sys_SETUID, uintptr(cred.Uid), 0L, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                }

            }

            foreach (var (_, c) in sys.AmbientCaps)
            {
                _, _, err1 = RawSyscall6(SYS_PRCTL, PR_CAP_AMBIENT, uintptr(PR_CAP_AMBIENT_RAISE), c, 0L, 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Chdir
            if (dir != null)
            {
                _, _, err1 = RawSyscall(SYS_CHDIR, uintptr(@unsafe.Pointer(dir)), 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Parent death signal
            if (sys.Pdeathsig != 0L)
            {
                _, _, err1 = RawSyscall6(SYS_PRCTL, PR_SET_PDEATHSIG, uintptr(sys.Pdeathsig), 0L, 0L, 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                } 

                // Signal self if parent is already dead. This might cause a
                // duplicate signal in rare cases, but it won't matter when
                // using SIGKILL.
                r1, _, _ = RawSyscall(SYS_GETPPID, 0L, 0L, 0L);
                if (r1 != ppid)
                {
                    var (pid, _, _) = RawSyscall(SYS_GETPID, 0L, 0L, 0L);
                    var (_, _, err1) = RawSyscall(SYS_KILL, pid, uintptr(sys.Pdeathsig), 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                }
            } 

            // Pass 1: look for fd[i] < i and move those up above len(fd)
            // so that pass 2 won't stomp on an fd it needs later.
            if (pipe < nextfd)
            {
                _, _, err1 = RawSyscall(_SYS_dup, uintptr(pipe), uintptr(nextfd), 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
                RawSyscall(SYS_FCNTL, uintptr(nextfd), F_SETFD, FD_CLOEXEC);
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
                    _, _, err1 = RawSyscall(_SYS_dup, uintptr(fd[i]), uintptr(nextfd), 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                    RawSyscall(SYS_FCNTL, uintptr(nextfd), F_SETFD, FD_CLOEXEC);
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
                    RawSyscall(SYS_CLOSE, uintptr(i), 0L, 0L);
                    continue;
                }
                if (fd[i] == int(i))
                { 
                    // dup2(i, i) won't clear close-on-exec flag on Linux,
                    // probably not elsewhere either.
                    _, _, err1 = RawSyscall(SYS_FCNTL, uintptr(fd[i]), F_SETFD, 0L);
                    if (err1 != 0L)
                    {
                        goto childerror;
                    }
                    continue;
                } 
                // The new fd is created NOT close-on-exec,
                // which is exactly what we want.
                _, _, err1 = RawSyscall(_SYS_dup, uintptr(fd[i]), uintptr(i), 0L);
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
                RawSyscall(SYS_CLOSE, uintptr(i), 0L, 0L);
            } 

            // Detach fd 0 from tty
 

            // Detach fd 0 from tty
            if (sys.Noctty)
            {
                _, _, err1 = RawSyscall(SYS_IOCTL, 0L, uintptr(TIOCNOTTY), 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Set the controlling TTY to Ctty
            if (sys.Setctty)
            {
                _, _, err1 = RawSyscall(SYS_IOCTL, uintptr(sys.Ctty), uintptr(TIOCSCTTY), 1L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Enable tracing if requested.
            // Do this right before exec so that we don't unnecessarily trace the runtime
            // setting up after the fork. See issue #21428.
            if (sys.Ptrace)
            {
                _, _, err1 = RawSyscall(SYS_PTRACE, uintptr(PTRACE_TRACEME), 0L, 0L);
                if (err1 != 0L)
                {
                    goto childerror;
                }
            } 

            // Time to exec.
            _, _, err1 = RawSyscall(SYS_EXECVE, uintptr(@unsafe.Pointer(argv0)), uintptr(@unsafe.Pointer(ref argv[0L])), uintptr(@unsafe.Pointer(ref envv[0L])));

childerror:
            RawSyscall(SYS_WRITE, uintptr(pipe), uintptr(@unsafe.Pointer(ref err1)), @unsafe.Sizeof(err1));
            while (true)
            {
                RawSyscall(SYS_EXIT, 253L, 0L, 0L);
            }

        }

        // Try to open a pipe with O_CLOEXEC set on both file descriptors.
        private static error forkExecPipe(slice<long> p)
        {
            err = Pipe2(p, O_CLOEXEC); 
            // pipe2 was added in 2.6.27 and our minimum requirement is 2.6.23, so it
            // might not be implemented.
            if (err == ENOSYS)
            {
                err = Pipe(p);

                if (err != null)
                {
                    return;
                }
                _, err = fcntl(p[0L], F_SETFD, FD_CLOEXEC);

                if (err != null)
                {
                    return;
                }
                _, err = fcntl(p[1L], F_SETFD, FD_CLOEXEC);
            }
            return;
        }

        // writeIDMappings writes the user namespace User ID or Group ID mappings to the specified path.
        private static error writeIDMappings(@string path, slice<SysProcIDMap> idMap)
        {
            var (fd, err) = Open(path, O_RDWR, 0L);
            if (err != null)
            {
                return error.As(err);
            }
            @string data = "";
            foreach (var (_, im) in idMap)
            {
                data = data + itoa(im.ContainerID) + " " + itoa(im.HostID) + " " + itoa(im.Size) + "\n";
            }
            var (bytes, err) = ByteSliceFromString(data);
            if (err != null)
            {
                Close(fd);
                return error.As(err);
            }
            {
                var (_, err) = Write(fd, bytes);

                if (err != null)
                {
                    Close(fd);
                    return error.As(err);
                }

            }

            {
                var err = Close(fd);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            return error.As(null);
        }

        // writeSetgroups writes to /proc/PID/setgroups "deny" if enable is false
        // and "allow" if enable is true.
        // This is needed since kernel 3.19, because you can't write gid_map without
        // disabling setgroups() system call.
        private static error writeSetgroups(long pid, bool enable)
        {
            @string sgf = "/proc/" + itoa(pid) + "/setgroups";
            var (fd, err) = Open(sgf, O_RDWR, 0L);
            if (err != null)
            {
                return error.As(err);
            }
            slice<byte> data = default;
            if (enable)
            {
                data = (slice<byte>)"allow";
            }
            else
            {
                data = (slice<byte>)"deny";
            }
            {
                var (_, err) = Write(fd, data);

                if (err != null)
                {
                    Close(fd);
                    return error.As(err);
                }

            }

            return error.As(Close(fd));
        }

        // writeUidGidMappings writes User ID and Group ID mappings for user namespaces
        // for a process and it is called from the parent process.
        private static error writeUidGidMappings(long pid, ref SysProcAttr sys)
        {
            if (sys.UidMappings != null)
            {
                @string uidf = "/proc/" + itoa(pid) + "/uid_map";
                {
                    var err__prev2 = err;

                    var err = writeIDMappings(uidf, sys.UidMappings);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            if (sys.GidMappings != null)
            { 
                // If the kernel is too old to support /proc/PID/setgroups, writeSetGroups will return ENOENT; this is OK.
                {
                    var err__prev2 = err;

                    err = writeSetgroups(pid, sys.GidMappingsEnableSetgroups);

                    if (err != null && err != ENOENT)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                @string gidf = "/proc/" + itoa(pid) + "/gid_map";
                {
                    var err__prev2 = err;

                    err = writeIDMappings(gidf, sys.GidMappings);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
            }
            return error.As(null);
        }
    }
}
