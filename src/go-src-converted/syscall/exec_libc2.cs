// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || (openbsd && !mips64)
// +build darwin openbsd,!mips64

// package syscall -- go2cs converted at 2022 March 13 05:40:30 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\exec_libc2.go
namespace go;

using abi = @internal.abi_package;
using @unsafe = @unsafe_package;

public static partial class syscall_package {

public partial struct SysProcAttr {
    public @string Chroot; // Chroot.
    public ptr<Credential> Credential; // Credential.
    public bool Ptrace; // Enable tracing.
    public bool Setsid; // Create session.
// Setpgid sets the process group ID of the child to Pgid,
// or, if Pgid == 0, to the new child's process ID.
    public bool Setpgid; // Setctty sets the controlling terminal of the child to
// file descriptor Ctty. Ctty must be a descriptor number
// in the child process: an index into ProcAttr.Files.
// This is only meaningful if Setsid is true.
    public bool Setctty;
    public bool Noctty; // Detach fd 0 from controlling terminal
    public nint Ctty; // Controlling TTY fd
// Foreground places the child process group in the foreground.
// This implies Setpgid. The Ctty field must be set to
// the descriptor of the controlling TTY.
// Unlike Setctty, in this case Ctty must be a descriptor
// number in the parent process.
    public bool Foreground;
    public nint Pgid; // Child's process group ID if Setpgid.
}

// Implemented in runtime package.
private static void runtime_BeforeFork();
private static void runtime_AfterFork();
private static void runtime_AfterForkInChild();

// Fork, dup fd onto 0..len(fd), and exec(argv0, argvv, envv) in child.
// If a dup or exec fails, write the errno error to pipe.
// (Pipe is close-on-exec so if exec succeeds, it will be closed.)
// In the child, this function must not acquire any locks, because
// they might have been locked at the time of the fork. This means
// no rescheduling, no malloc calls, and no new stack segments.
// For the same reason compiler does not race instrument it.
// The calls to rawSyscall are okay because they are assembly
// functions that do not grow the stack.
//go:norace
private static (nint, Errno) forkAndExecInChild(ptr<byte> _addr_argv0, slice<ptr<byte>> argv, slice<ptr<byte>> envv, ptr<byte> _addr_chroot, ptr<byte> _addr_dir, ptr<ProcAttr> _addr_attr, ptr<SysProcAttr> _addr_sys, nint pipe) {
    nint pid = default;
    Errno err = default;
    ref byte argv0 = ref _addr_argv0.val;
    ref byte chroot = ref _addr_chroot.val;
    ref byte dir = ref _addr_dir.val;
    ref ProcAttr attr = ref _addr_attr.val;
    ref SysProcAttr sys = ref _addr_sys.val;
 
    // Declare all variables at top in case any
    // declarations require heap allocation (e.g., err1).
    System.UIntPtr r1 = default;    ref Errno err1 = ref heap(out ptr<Errno> _addr_err1);    nint nextfd = default;    nint i = default; 

    // guard against side effects of shuffling fds below.
    // Make sure that nextfd is beyond any currently open files so
    // that we can't run the risk of overwriting any of them.
    var fd = make_slice<nint>(len(attr.Files));
    nextfd = len(attr.Files);
    {
        nint i__prev1 = i;

        foreach (var (__i, __ufd) in attr.Files) {
            i = __i;
            ufd = __ufd;
            if (nextfd < int(ufd)) {>>MARKER:FUNCTION_runtime_AfterForkInChild_BLOCK_PREFIX<<
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
    r1, _, err1 = rawSyscall(abi.FuncPCABI0(libc_fork_trampoline), 0, 0, 0);
    if (err1 != 0) {>>MARKER:FUNCTION_runtime_AfterFork_BLOCK_PREFIX<<
        runtime_AfterFork();
        return (0, err1);
    }
    if (r1 != 0) {>>MARKER:FUNCTION_runtime_BeforeFork_BLOCK_PREFIX<< 
        // parent; return PID
        runtime_AfterFork();
        return (int(r1), 0);
    }
    if (sys.Ptrace) {
        {
            var err = ptrace(PTRACE_TRACEME, 0, 0, 0);

            if (err != null) {
                err1 = err._<Errno>();
                goto childerror;
            }

        }
    }
    if (sys.Setsid) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_setsid_trampoline), 0, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (sys.Setpgid || sys.Foreground) { 
        // Place child in process group.
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_setpgid_trampoline), 0, uintptr(sys.Pgid), 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (sys.Foreground) {
        ref var pgrp = ref heap(sys.Pgid, out ptr<var> _addr_pgrp);
        if (pgrp == 0) {
            r1, _, err1 = rawSyscall(abi.FuncPCABI0(libc_getpid_trampoline), 0, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
            pgrp = int(r1);
        }
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), uintptr(sys.Ctty), uintptr(TIOCSPGRP), uintptr(@unsafe.Pointer(_addr_pgrp)));
        if (err1 != 0) {
            goto childerror;
        }
    }
    runtime_AfterForkInChild(); 

    // Chroot
    if (chroot != null) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_chroot_trampoline), uintptr(@unsafe.Pointer(chroot)), 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    {
        var cred = sys.Credential;

        if (cred != null) {
            var ngroups = uintptr(len(cred.Groups));
            var groups = uintptr(0);
            if (ngroups > 0) {
                groups = uintptr(@unsafe.Pointer(_addr_cred.Groups[0]));
            }
            if (!cred.NoSetGroups) {
                _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_setgroups_trampoline), ngroups, groups, 0);
                if (err1 != 0) {
                    goto childerror;
                }
            }
            _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_setgid_trampoline), uintptr(cred.Gid), 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
            _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_setuid_trampoline), uintptr(cred.Uid), 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
        }
    } 

    // Chdir
    if (dir != null) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_chdir_trampoline), uintptr(@unsafe.Pointer(dir)), 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (pipe < nextfd) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_dup2_trampoline), uintptr(pipe), uintptr(nextfd), 0);
        if (err1 != 0) {
            goto childerror;
        }
        rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), uintptr(nextfd), F_SETFD, FD_CLOEXEC);
        pipe = nextfd;
        nextfd++;
    }
    for (i = 0; i < len(fd); i++) {
        if (fd[i] >= 0 && fd[i] < int(i)) {
            if (nextfd == pipe) { // don't stomp on pipe
                nextfd++;
            }
            _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_dup2_trampoline), uintptr(fd[i]), uintptr(nextfd), 0);
            if (err1 != 0) {
                goto childerror;
            }
            rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), uintptr(nextfd), F_SETFD, FD_CLOEXEC);
            fd[i] = nextfd;
            nextfd++;
        }
    } 

    // Pass 2: dup fd[i] down onto i.
    for (i = 0; i < len(fd); i++) {
        if (fd[i] == -1) {
            rawSyscall(abi.FuncPCABI0(libc_close_trampoline), uintptr(i), 0, 0);
            continue;
        }
        if (fd[i] == int(i)) { 
            // dup2(i, i) won't clear close-on-exec flag on Linux,
            // probably not elsewhere either.
            _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), uintptr(fd[i]), F_SETFD, 0);
            if (err1 != 0) {
                goto childerror;
            }
            continue;
        }
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_dup2_trampoline), uintptr(fd[i]), uintptr(i), 0);
        if (err1 != 0) {
            goto childerror;
        }
    } 

    // By convention, we don't close-on-exec the fds we are
    // started with, so if len(fd) < 3, close 0, 1, 2 as needed.
    // Programs that know they inherit fds >= 3 will need
    // to set them close-on-exec.
    for (i = len(fd); i < 3; i++) {
        rawSyscall(abi.FuncPCABI0(libc_close_trampoline), uintptr(i), 0, 0);
    } 

    // Detach fd 0 from tty
    if (sys.Noctty) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), 0, uintptr(TIOCNOTTY), 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (sys.Setctty) {
        _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), uintptr(sys.Ctty), uintptr(TIOCSCTTY), 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    _, _, err1 = rawSyscall(abi.FuncPCABI0(libc_execve_trampoline), uintptr(@unsafe.Pointer(argv0)), uintptr(@unsafe.Pointer(_addr_argv[0])), uintptr(@unsafe.Pointer(_addr_envv[0])));

childerror:
    rawSyscall(abi.FuncPCABI0(libc_write_trampoline), uintptr(pipe), uintptr(@unsafe.Pointer(_addr_err1)), @unsafe.Sizeof(err1));
    while (true) {
        rawSyscall(abi.FuncPCABI0(libc_exit_trampoline), 253, 0, 0);
    }
}

} // end syscall_package
