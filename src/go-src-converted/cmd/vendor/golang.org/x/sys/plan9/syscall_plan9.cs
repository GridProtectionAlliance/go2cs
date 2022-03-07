// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package plan9 -- go2cs converted at 2022 March 06 23:26:25 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\syscall_plan9.go
using bytes = go.bytes_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class plan9_package {

    // A Note is a string describing a process note.
    // It implements the os.Signal interface.
public partial struct Note { // : @string
}

public static void Signal(this Note n) {
}

public static @string String(this Note n) {
    return string(n);
}

public static nint Stdin = 0;public static nint Stdout = 1;public static nint Stderr = 2;

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.
public static bool SocketDisableIPv6 = default;

public static (System.UIntPtr, System.UIntPtr, syscall.ErrorString) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, syscall.ErrorString) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
public static (System.UIntPtr, System.UIntPtr, System.UIntPtr) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, System.UIntPtr) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

private static nuint atoi(slice<byte> b) {
    nuint n = default;

    n = 0;
    for (nint i = 0; i < len(b); i++) {>>MARKER:FUNCTION_RawSyscall6_BLOCK_PREFIX<<
        n = n * 10 + uint(b[i] - '0');
    }
    return ;
}

private static @string cstring(slice<byte> s) {
    var i = bytes.IndexByte(s, 0);
    if (i == -1) {>>MARKER:FUNCTION_RawSyscall_BLOCK_PREFIX<<
        i = len(s);
    }
    return string(s[..(int)i]);

}

private static @string errstr() {
    array<byte> buf = new array<byte>(ERRMAX);

    RawSyscall(SYS_ERRSTR, uintptr(@unsafe.Pointer(_addr_buf[0])), uintptr(len(buf)), 0);

    buf[len(buf) - 1] = 0;
    return cstring(buf[..]);
}

// Implemented in assembly to import from runtime.
private static void exit(nint code);

public static void Exit(nint code) {
    exit(code);
}

private static (nuint, error) readnum(@string path) => func((defer, _, _) => {
    nuint _p0 = default;
    error _p0 = default!;

    array<byte> b = new array<byte>(12);

    var (fd, e) = Open(path, O_RDONLY);
    if (e != null) {>>MARKER:FUNCTION_exit_BLOCK_PREFIX<<
        return (0, error.As(e)!);
    }
    defer(Close(fd));

    var (n, e) = Pread(fd, b[..], 0);

    if (e != null) {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
        return (0, error.As(e)!);
    }
    nint m = 0;
    while (m < n && b[m] == ' ') {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
        m++;
    }

    return (atoi(b[(int)m..(int)n - 1]), error.As(null!)!);

});

public static nint Getpid() {
    nint pid = default;

    var (n, _) = readnum("#c/pid");
    return int(n);
}

public static nint Getppid() {
    nint ppid = default;

    var (n, _) = readnum("#c/ppid");
    return int(n);
}

public static (nint, error) Read(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    return Pread(fd, p, -1);
}

public static (nint, error) Write(nint fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    return Pwrite(fd, p, -1);
}

private static long ioSync = default;

//sys    fd2path(fd int, buf []byte) (err error)
public static (@string, error) Fd2path(nint fd) {
    @string path = default;
    error err = default!;

    array<byte> buf = new array<byte>(512);

    var e = fd2path(fd, buf[..]);
    if (e != null) {
        return ("", error.As(e)!);
    }
    return (cstring(buf[..]), error.As(null!)!);

}

//sys    pipe(p *[2]int32) (err error)
public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(syscall.ErrorString("bad arg in system call"))!;
    }
    ref array<int> pp = ref heap(new array<int>(2), out ptr<array<int>> _addr_pp);
    err = pipe(_addr_pp);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return ;

}

// Underlying system call writes to newoffset via pointer.
// Implemented in assembly to avoid allocation.
private static (long, @string) seek(System.UIntPtr placeholder, nint fd, long offset, nint whence);

public static (long, error) Seek(nint fd, long offset, nint whence) {
    long newoffset = default;
    error err = default!;

    var (newoffset, e) = seek(0, fd, offset, whence);

    if (newoffset == -1) {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
        err = syscall.ErrorString(e);
    }
    return ;

}

public static error Mkdir(@string path, uint mode) {
    error err = default!;

    var (fd, err) = Create(path, O_RDONLY, DMDIR | mode);

    if (fd != -1) {
        Close(fd);
    }
    return ;

}

public partial struct Waitmsg {
    public nint Pid;
    public array<uint> Time;
    public @string Msg;
}

public static bool Exited(this Waitmsg w) {
    return true;
}
public static bool Signaled(this Waitmsg w) {
    return false;
}

public static nint ExitStatus(this Waitmsg w) {
    if (len(w.Msg) == 0) { 
        // a normal exit returns no message
        return 0;

    }
    return 1;

}

//sys    await(s []byte) (n int, err error)
public static error Await(ptr<Waitmsg> _addr_w) {
    error err = default!;
    ref Waitmsg w = ref _addr_w.val;

    array<byte> buf = new array<byte>(512);
    array<slice<byte>> f = new array<slice<byte>>(5);

    var (n, err) = await(buf[..]);

    if (err != null || w == null) {
        return ;
    }
    nint nf = 0;
    nint p = 0;
    for (nint i = 0; i < n && nf < len(f) - 1; i++) {
        if (buf[i] == ' ') {
            f[nf] = buf[(int)p..(int)i];
            p = i + 1;
            nf++;
        }
    }
    f[nf] = buf[(int)p..];
    nf++;

    if (nf != len(f)) {
        return error.As(syscall.ErrorString("invalid wait message"))!;
    }
    w.Pid = int(atoi(f[0]));
    w.Time[0] = uint32(atoi(f[1]));
    w.Time[1] = uint32(atoi(f[2]));
    w.Time[2] = uint32(atoi(f[3]));
    w.Msg = cstring(f[4]);
    if (w.Msg == "''") { 
        // await() returns '' for no error
        w.Msg = "";

    }
    return ;

}

public static error Unmount(@string name, @string old) {
    error err = default!;

    fixwd();
    var (oldp, err) = BytePtrFromString(old);
    if (err != null) {
        return error.As(err)!;
    }
    var oldptr = uintptr(@unsafe.Pointer(oldp));

    System.UIntPtr r0 = default;
    syscall.ErrorString e = default; 

    // bind(2) man page: If name is zero, everything bound or mounted upon old is unbound or unmounted.
    if (name == "") {
        r0, _, e = Syscall(SYS_UNMOUNT, _zero, oldptr, 0);
    }
    else
 {
        var (namep, err) = BytePtrFromString(name);
        if (err != null) {
            return error.As(err)!;
        }
        r0, _, e = Syscall(SYS_UNMOUNT, uintptr(@unsafe.Pointer(namep)), oldptr, 0);

    }
    if (int32(r0) == -1) {
        err = e;
    }
    return ;

}

public static error Fchdir(nint fd) {
    error err = default!;

    var (path, err) = Fd2path(fd);

    if (err != null) {
        return ;
    }
    return error.As(Chdir(path))!;

}

public partial struct Timespec {
    public int Sec;
    public int Nsec;
}

public partial struct Timeval {
    public int Sec;
    public int Usec;
}

public static Timeval NsecToTimeval(long nsec) {
    Timeval tv = default;

    nsec += 999; // round up to microsecond
    tv.Usec = int32(nsec % 1e9F / 1e3F);
    tv.Sec = int32(nsec / 1e9F);
    return ;

}

private static long nsec() {
    ref long scratch = ref heap(out ptr<long> _addr_scratch);

    var (r0, _, _) = Syscall(SYS_NSEC, uintptr(@unsafe.Pointer(_addr_scratch)), 0, 0); 
    // TODO(aram): remove hack after I fix _nsec in the pc64 kernel.
    if (r0 == 0) {
        return scratch;
    }
    return int64(r0);

}

public static error Gettimeofday(ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    var nsec = nsec();
    tv = NsecToTimeval(nsec);
    return error.As(null!)!;
}

public static nint Getpagesize() {
    return 0x1000;
}

public static nint Getegid() {
    nint egid = default;

    return -1;
}
public static nint Geteuid() {
    nint euid = default;

    return -1;
}
public static nint Getgid() {
    nint gid = default;

    return -1;
}
public static nint Getuid() {
    nint uid = default;

    return -1;
}

public static (slice<nint>, error) Getgroups() {
    slice<nint> gids = default;
    error err = default!;

    return (make_slice<nint>(0), error.As(null!)!);
}

//sys    open(path string, mode int) (fd int, err error)
public static (nint, error) Open(@string path, nint mode) {
    nint fd = default;
    error err = default!;

    fixwd();
    return open(path, mode);
}

//sys    create(path string, mode int, perm uint32) (fd int, err error)
public static (nint, error) Create(@string path, nint mode, uint perm) {
    nint fd = default;
    error err = default!;

    fixwd();
    return create(path, mode, perm);
}

//sys    remove(path string) (err error)
public static error Remove(@string path) {
    fixwd();
    return error.As(remove(path))!;
}

//sys    stat(path string, edir []byte) (n int, err error)
public static (nint, error) Stat(@string path, slice<byte> edir) {
    nint n = default;
    error err = default!;

    fixwd();
    return stat(path, edir);
}

//sys    bind(name string, old string, flag int) (err error)
public static error Bind(@string name, @string old, nint flag) {
    error err = default!;

    fixwd();
    return error.As(bind(name, old, flag))!;
}

//sys    mount(fd int, afd int, old string, flag int, aname string) (err error)
public static error Mount(nint fd, nint afd, @string old, nint flag, @string aname) {
    error err = default!;

    fixwd();
    return error.As(mount(fd, afd, old, flag, aname))!;
}

//sys    wstat(path string, edir []byte) (err error)
public static error Wstat(@string path, slice<byte> edir) {
    error err = default!;

    fixwd();
    return error.As(wstat(path, edir))!;
}

//sys    chdir(path string) (err error)
//sys    Dup(oldfd int, newfd int) (fd int, err error)
//sys    Pread(fd int, p []byte, offset int64) (n int, err error)
//sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
//sys    Close(fd int) (err error)
//sys    Fstat(fd int, edir []byte) (n int, err error)
//sys    Fwstat(fd int, edir []byte) (err error)

} // end plan9_package
