// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package syscall -- go2cs converted at 2022 March 06 22:26:59 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_js.go
using itoa = go.@internal.itoa_package;
using oserror = go.@internal.oserror_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

private static readonly nint direntSize = 8 + 8 + 2 + 256;



public partial struct Dirent {
    public ushort Reclen;
    public array<byte> Name;
}

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return (1, true);
}

private static (ulong, bool) direntReclen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
}

private static (ulong, bool) direntNamlen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    var (reclen, ok) = direntReclen(buf);
    if (!ok) {
        return (0, false);
    }
    return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);

}

public static readonly nint PathMax = 256;

// An Errno is an unsigned number describing an error condition.
// It implements the error interface. The zero Errno is by convention
// a non-error, so code to convert from Errno to error should use:
//    err = nil
//    if errno != 0 {
//        err = errno
//    }
//
// Errno values can be tested against error values from the os package
// using errors.Is. For example:
//
//    _, _, err := syscall.Syscall(...)
//    if errors.Is(err, fs.ErrNotExist) ...


// An Errno is an unsigned number describing an error condition.
// It implements the error interface. The zero Errno is by convention
// a non-error, so code to convert from Errno to error should use:
//    err = nil
//    if errno != 0 {
//        err = errno
//    }
//
// Errno values can be tested against error values from the os package
// using errors.Is. For example:
//
//    _, _, err := syscall.Syscall(...)
//    if errors.Is(err, fs.ErrNotExist) ...
public partial struct Errno { // : System.UIntPtr
}

public static @string Error(this Errno e) {
    if (0 <= int(e) && int(e) < len(errorstr)) {
        var s = errorstr[e];
        if (s != "") {
            return s;
        }
    }
    return "errno " + itoa.Itoa(int(e));

}

public static bool Is(this Errno e, error target) {

    if (target == oserror.ErrPermission) 
        return e == EACCES || e == EPERM;
    else if (target == oserror.ErrExist) 
        return e == EEXIST || e == ENOTEMPTY;
    else if (target == oserror.ErrNotExist) 
        return e == ENOENT;
        return false;

}

public static bool Temporary(this Errno e) {
    return e == EINTR || e == EMFILE || e.Timeout();
}

public static bool Timeout(this Errno e) {
    return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
}

// A Signal is a number describing a process signal.
// It implements the os.Signal interface.
public partial struct Signal { // : nint
}

private static readonly Signal _ = iota;
public static readonly var SIGCHLD = 0;
public static readonly var SIGINT = 1;
public static readonly var SIGKILL = 2;
public static readonly var SIGTRAP = 3;
public static readonly var SIGQUIT = 4;
public static readonly var SIGTERM = 5;


public static void Signal(this Signal s) {
}

public static @string String(this Signal s) {
    if (0 <= s && int(s) < len(signals)) {
        var str = signals[s];
        if (str != "") {
            return str;
        }
    }
    return "signal " + itoa.Itoa(int(s));

}

private static array<@string> signals = new array<@string>(new @string[] {  });

// File system

public static readonly nint Stdin = 0;
public static readonly nint Stdout = 1;
public static readonly nint Stderr = 2;


public static readonly nint O_RDONLY = 0;
public static readonly nint O_WRONLY = 1;
public static readonly nint O_RDWR = 2;

public static readonly nint O_CREAT = 0100;
public static readonly var O_CREATE = O_CREAT;
public static readonly nint O_TRUNC = 01000;
public static readonly nint O_APPEND = 02000;
public static readonly nint O_EXCL = 0200;
public static readonly nint O_SYNC = 010000;

public static readonly nint O_CLOEXEC = 0;


public static readonly nint F_DUPFD = 0;
public static readonly nint F_GETFD = 1;
public static readonly nint F_SETFD = 2;
public static readonly nint F_GETFL = 3;
public static readonly nint F_SETFL = 4;
public static readonly nint F_GETOWN = 5;
public static readonly nint F_SETOWN = 6;
public static readonly nint F_GETLK = 7;
public static readonly nint F_SETLK = 8;
public static readonly nint F_SETLKW = 9;
public static readonly nint F_RGETLK = 10;
public static readonly nint F_RSETLK = 11;
public static readonly nint F_CNVT = 12;
public static readonly nint F_RSETLKW = 13;

public static readonly nint F_RDLCK = 1;
public static readonly nint F_WRLCK = 2;
public static readonly nint F_UNLCK = 3;
public static readonly nint F_UNLKSYS = 4;


public static readonly nint S_IFMT = 0000370000;
public static readonly nint S_IFSHM_SYSV = 0000300000;
public static readonly nint S_IFSEMA = 0000270000;
public static readonly nint S_IFCOND = 0000260000;
public static readonly nint S_IFMUTEX = 0000250000;
public static readonly nint S_IFSHM = 0000240000;
public static readonly nint S_IFBOUNDSOCK = 0000230000;
public static readonly nint S_IFSOCKADDR = 0000220000;
public static readonly nint S_IFDSOCK = 0000210000;

public static readonly nint S_IFSOCK = 0000140000;
public static readonly nint S_IFLNK = 0000120000;
public static readonly nint S_IFREG = 0000100000;
public static readonly nint S_IFBLK = 0000060000;
public static readonly nint S_IFDIR = 0000040000;
public static readonly nint S_IFCHR = 0000020000;
public static readonly nint S_IFIFO = 0000010000;

public static readonly nint S_UNSUP = 0000370000;

public static readonly nint S_ISUID = 0004000;
public static readonly nint S_ISGID = 0002000;
public static readonly nint S_ISVTX = 0001000;

public static readonly nint S_IREAD = 0400;
public static readonly nint S_IWRITE = 0200;
public static readonly nint S_IEXEC = 0100;

public static readonly nint S_IRWXU = 0700;
public static readonly nint S_IRUSR = 0400;
public static readonly nint S_IWUSR = 0200;
public static readonly nint S_IXUSR = 0100;

public static readonly nint S_IRWXG = 070;
public static readonly nint S_IRGRP = 040;
public static readonly nint S_IWGRP = 020;
public static readonly nint S_IXGRP = 010;

public static readonly nint S_IRWXO = 07;
public static readonly nint S_IROTH = 04;
public static readonly nint S_IWOTH = 02;
public static readonly nint S_IXOTH = 01;


public partial struct Stat_t {
    public long Dev;
    public ulong Ino;
    public uint Mode;
    public uint Nlink;
    public uint Uid;
    public uint Gid;
    public long Rdev;
    public long Size;
    public int Blksize;
    public int Blocks;
    public long Atime;
    public long AtimeNsec;
    public long Mtime;
    public long MtimeNsec;
    public long Ctime;
    public long CtimeNsec;
}

// Processes
// Not supported - just enough for package os.

public static sync.RWMutex ForkLock = default;

public partial struct WaitStatus { // : uint
}

public static bool Exited(this WaitStatus w) {
    return false;
}
public static nint ExitStatus(this WaitStatus w) {
    return 0;
}
public static bool Signaled(this WaitStatus w) {
    return false;
}
public static Signal Signal(this WaitStatus w) {
    return 0;
}
public static bool CoreDump(this WaitStatus w) {
    return false;
}
public static bool Stopped(this WaitStatus w) {
    return false;
}
public static bool Continued(this WaitStatus w) {
    return false;
}
public static Signal StopSignal(this WaitStatus w) {
    return 0;
}
public static nint TrapCause(this WaitStatus w) {
    return 0;
}

// XXX made up
public partial struct Rusage {
    public Timeval Utime;
    public Timeval Stime;
}

// XXX made up
public partial struct ProcAttr {
    public @string Dir;
    public slice<@string> Env;
    public slice<System.UIntPtr> Files;
    public ptr<SysProcAttr> Sys;
}

public partial struct SysProcAttr {
}

public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return (0, 0, ENOSYS);
}

public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return (0, 0, ENOSYS);
}

public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return (0, 0, ENOSYS);
}

public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    Errno err = default;

    return (0, 0, ENOSYS);
}

public static (@string, error) Sysctl(@string key) {
    @string _p0 = default;
    error _p0 = default!;

    if (key == "kern.hostname") {
        return ("js", error.As(null!)!);
    }
    return ("", error.As(ENOSYS)!);

}

public static readonly var ImplementsGetwd = true;



public static (@string, error) Getwd() {
    @string wd = default;
    error err = default!;

    array<byte> buf = new array<byte>(PathMax);
    var (n, err) = Getcwd(buf[(int)0..]);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (string(buf[..(int)n]), error.As(null!)!);

}

public static nint Getuid() {
    return jsProcess.Call("getuid").Int();
}

public static nint Getgid() {
    return jsProcess.Call("getgid").Int();
}

public static nint Geteuid() {
    return jsProcess.Call("geteuid").Int();
}

public static nint Getegid() {
    return jsProcess.Call("getegid").Int();
}

public static (slice<nint>, error) Getgroups() => func((defer, _, _) => {
    slice<nint> groups = default;
    error err = default!;

    defer(recoverErr(_addr_err));
    var array = jsProcess.Call("getgroups");
    groups = make_slice<nint>(array.Length());
    foreach (var (i) in groups) {
        groups[i] = array.Index(i).Int();
    }    return (groups, error.As(null!)!);
});

public static nint Getpid() {
    return jsProcess.Get("pid").Int();
}

public static nint Getppid() {
    return jsProcess.Get("ppid").Int();
}

public static nint Umask(nint mask) {
    nint oldmask = default;

    return jsProcess.Call("umask", mask).Int();
}

public static error Gettimeofday(ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    return error.As(ENOSYS)!;
}

public static error Kill(nint pid, Signal signum) {
    return error.As(ENOSYS)!;
}
public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    return (0, error.As(ENOSYS)!);
}
public static (nint, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr) {
    nint pid = default;
    System.UIntPtr handle = default;
    error err = default!;
    ref ProcAttr attr = ref _addr_attr.val;

    return (0, 0, error.As(ENOSYS)!);
}
public static (nint, error) Wait4(nint pid, ptr<WaitStatus> _addr_wstatus, nint options, ptr<Rusage> _addr_rusage) {
    nint wpid = default;
    error err = default!;
    ref WaitStatus wstatus = ref _addr_wstatus.val;
    ref Rusage rusage = ref _addr_rusage.val;

    return (0, error.As(ENOSYS)!);
}

public partial struct Iovec {
} // dummy

public partial struct Timespec {
    public long Sec;
    public long Nsec;
}

public partial struct Timeval {
    public long Sec;
    public long Usec;
}

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:usec);
}

} // end syscall_package
