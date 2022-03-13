// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Fork, exec, wait, etc.

// package syscall -- go2cs converted at 2022 March 13 05:40:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\exec_unix.go
namespace go;

using errorspkg = errors_package;
using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using sync = sync_package;
using @unsafe = @unsafe_package;


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


using System;public static partial class syscall_package {

public static sync.RWMutex ForkLock = default;

// StringSlicePtr converts a slice of strings to a slice of pointers
// to NUL-terminated byte arrays. If any string contains a NUL byte
// this function panics instead of returning an error.
//
// Deprecated: Use SlicePtrFromStrings instead.
public static slice<ptr<byte>> StringSlicePtr(slice<@string> ss) {
    var bb = make_slice<ptr<byte>>(len(ss) + 1);
    for (nint i = 0; i < len(ss); i++) {
        bb[i] = StringBytePtr(ss[i]);
    }
    bb[len(ss)] = null;
    return bb;
}

// SlicePtrFromStrings converts a slice of strings to a slice of
// pointers to NUL-terminated byte arrays. If any string contains
// a NUL byte, it returns (nil, EINVAL).
public static (slice<ptr<byte>>, error) SlicePtrFromStrings(slice<@string> ss) {
    slice<ptr<byte>> _p0 = default;
    error _p0 = default!;

    nint n = 0;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ss) {
            s = __s;
            if (bytealg.IndexByteString(s, 0) != -1) {
                return (null, error.As(EINVAL)!);
            }
            n += len(s) + 1; // +1 for NUL
        }
        s = s__prev1;
    }

    var bb = make_slice<ptr<byte>>(len(ss) + 1);
    var b = make_slice<byte>(n);
    n = 0;
    {
        var s__prev1 = s;

        foreach (var (__i, __s) in ss) {
            i = __i;
            s = __s;
            bb[i] = _addr_b[n];
            copy(b[(int)n..], s);
            n += len(s) + 1;
        }
        s = s__prev1;
    }

    return (bb, error.As(null!)!);
}

public static void CloseOnExec(nint fd) {
    fcntl(fd, F_SETFD, FD_CLOEXEC);
}

public static error SetNonblock(nint fd, bool nonblocking) {
    error err = default!;

    var (flag, err) = fcntl(fd, F_GETFL, 0);
    if (err != null) {
        return error.As(err)!;
    }
    if (nonblocking) {
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
public partial struct Credential {
    public uint Uid; // User ID.
    public uint Gid; // Group ID.
    public slice<uint> Groups; // Supplementary group IDs.
    public bool NoSetGroups; // If true, don't set supplementary groups
}

// ProcAttr holds attributes that will be applied to a new process started
// by StartProcess.
public partial struct ProcAttr {
    public @string Dir; // Current working directory.
    public slice<@string> Env; // Environment.
    public slice<System.UIntPtr> Files; // File descriptors.
    public ptr<SysProcAttr> Sys;
}

private static ProcAttr zeroProcAttr = default;
private static SysProcAttr zeroSysProcAttr = default;

private static (nint, error) forkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr) {
    nint pid = default;
    error err = default!;
    ref ProcAttr attr = ref _addr_attr.val;

    array<nint> p = new array<nint>(2);
    nint n = default;
    ref Errno err1 = ref heap(out ptr<Errno> _addr_err1);
    ref WaitStatus wstatus = ref heap(out ptr<WaitStatus> _addr_wstatus);

    if (attr == null) {
        attr = _addr_zeroProcAttr;
    }
    var sys = attr.Sys;
    if (sys == null) {
        sys = _addr_zeroSysProcAttr;
    }
    p[0] = -1;
    p[1] = -1; 

    // Convert args to C form.
    var (argv0p, err) = BytePtrFromString(argv0);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (argvp, err) = SlicePtrFromStrings(argv);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (envvp, err) = SlicePtrFromStrings(attr.Env);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if ((runtime.GOOS == "freebsd" || runtime.GOOS == "dragonfly") && len(argv[0]) > len(argv0)) {
        argvp[0] = argv0p;
    }
    ptr<byte> chroot;
    if (sys.Chroot != "") {
        chroot, err = BytePtrFromString(sys.Chroot);
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    ptr<byte> dir;
    if (attr.Dir != "") {
        dir, err = BytePtrFromString(attr.Dir);
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    if (sys.Setctty && sys.Foreground) {
        return (0, error.As(errorspkg.New("both Setctty and Foreground set in SysProcAttr"))!);
    }
    if (sys.Setctty && sys.Ctty >= len(attr.Files)) {
        return (0, error.As(errorspkg.New("Setctty set but Ctty not valid in child"))!);
    }
    ForkLock.Lock(); 

    // Allocate child status pipe close on exec.
    err = forkExecPipe(p[..]);

    if (err != null) {
        goto error;
    }
    pid, err1 = forkAndExecInChild(argv0p, argvp, envvp, chroot, dir, attr, sys, p[1]);
    if (err1 != 0) {
        err = Errno(err1);
        goto error;
    }
    ForkLock.Unlock(); 

    // Read child error status from pipe.
    Close(p[1]);
    while (true) {
        n, err = readlen(p[0], (byte.val)(@unsafe.Pointer(_addr_err1)), int(@unsafe.Sizeof(err1)));
        if (err != EINTR) {
            break;
        }
    }
    Close(p[0]);
    if (err != null || n != 0) {
        if (n == int(@unsafe.Sizeof(err1))) {
            err = Errno(err1);
        }
        if (err == null) {
            err = EPIPE;
        }
        var (_, err1) = Wait4(pid, _addr_wstatus, 0, null);
        while (err1 == EINTR) {
            _, err1 = Wait4(pid, _addr_wstatus, 0, null);
        }
        return (0, error.As(err)!);
    }
    return (pid, error.As(null!)!);

error:
    if (p[0] >= 0) {
        Close(p[0]);
        Close(p[1]);
    }
    ForkLock.Unlock();
    return (0, error.As(err)!);
}

// Combination of fork and exec, careful to be thread safe.
public static (nint, error) ForkExec(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr) {
    nint pid = default;
    error err = default!;
    ref ProcAttr attr = ref _addr_attr.val;

    return forkExec(argv0, argv, _addr_attr);
}

// StartProcess wraps ForkExec for package os.
public static (nint, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr) {
    nint pid = default;
    System.UIntPtr handle = default;
    error err = default!;
    ref ProcAttr attr = ref _addr_attr.val;

    pid, err = forkExec(argv0, argv, _addr_attr);
    return (pid, 0, error.As(err)!);
}

// Implemented in runtime package.
private static void runtime_BeforeExec();
private static void runtime_AfterExec();

// execveLibc is non-nil on OS using libc syscall, set to execve in exec_libc.go; this
// avoids a build dependency for other platforms.
private static Func<System.UIntPtr, System.UIntPtr, System.UIntPtr, Errno> execveLibc = default;
private static Func<ptr<byte>, ptr<ptr<byte>>, ptr<ptr<byte>>, error> execveDarwin = default;
private static Func<ptr<byte>, ptr<ptr<byte>>, ptr<ptr<byte>>, error> execveOpenBSD = default;

// Exec invokes the execve(2) system call.
public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    var (argv0p, err) = BytePtrFromString(argv0);
    if (err != null) {>>MARKER:FUNCTION_runtime_AfterExec_BLOCK_PREFIX<<
        return error.As(err)!;
    }
    var (argvp, err) = SlicePtrFromStrings(argv);
    if (err != null) {>>MARKER:FUNCTION_runtime_BeforeExec_BLOCK_PREFIX<<
        return error.As(err)!;
    }
    var (envvp, err) = SlicePtrFromStrings(envv);
    if (err != null) {
        return error.As(err)!;
    }
    runtime_BeforeExec();

    error err1 = default!;
    if (runtime.GOOS == "solaris" || runtime.GOOS == "illumos" || runtime.GOOS == "aix") { 
        // RawSyscall should never be used on Solaris, illumos, or AIX.
        err1 = error.As(execveLibc(uintptr(@unsafe.Pointer(argv0p)), uintptr(@unsafe.Pointer(_addr_argvp[0])), uintptr(@unsafe.Pointer(_addr_envvp[0]))))!;
    }
    else if (runtime.GOOS == "darwin" || runtime.GOOS == "ios") { 
        // Similarly on Darwin.
        err1 = error.As(execveDarwin(argv0p, _addr_argvp[0], _addr_envvp[0]))!;
    }
    else if (runtime.GOOS == "openbsd" && (runtime.GOARCH == "386" || runtime.GOARCH == "amd64" || runtime.GOARCH == "arm" || runtime.GOARCH == "arm64")) { 
        // Similarly on OpenBSD.
        err1 = error.As(execveOpenBSD(argv0p, _addr_argvp[0], _addr_envvp[0]))!;
    }
    else
 {
        _, _, err1 = RawSyscall(SYS_EXECVE, uintptr(@unsafe.Pointer(argv0p)), uintptr(@unsafe.Pointer(_addr_argvp[0])), uintptr(@unsafe.Pointer(_addr_envvp[0])));
    }
    runtime_AfterExec();
    return error.As(err1)!;
}

} // end syscall_package
