// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || netbsd || openbsd
// +build dragonfly freebsd netbsd openbsd

// Berkeley packet filter for BSD variants

// package syscall -- go2cs converted at 2022 March 06 22:08:07 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\bpf_bsd.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // Deprecated: Use golang.org/x/net/bpf instead.
public static ptr<BpfInsn> BpfStmt(nint code, nint k) {
    return addr(new BpfInsn(Code:uint16(code),K:uint32(k)));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static ptr<BpfInsn> BpfJump(nint code, nint k, nint jt, nint jf) {
    return addr(new BpfInsn(Code:uint16(code),Jt:uint8(jt),Jf:uint8(jf),K:uint32(k)));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfBuflen(nint fd) {
    nint _p0 = default;
    error _p0 = default!;

    ref nint l = ref heap(out ptr<nint> _addr_l);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGBLEN, uintptr(@unsafe.Pointer(_addr_l)));
    if (err != 0) {
        return (0, error.As(Errno(err))!);
    }
    return (l, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfBuflen(nint fd, nint l) {
    nint _p0 = default;
    error _p0 = default!;

    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSBLEN, uintptr(@unsafe.Pointer(_addr_l)));
    if (err != 0) {
        return (0, error.As(Errno(err))!);
    }
    return (l, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfDatalink(nint fd) {
    nint _p0 = default;
    error _p0 = default!;

    ref nint t = ref heap(out ptr<nint> _addr_t);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGDLT, uintptr(@unsafe.Pointer(_addr_t)));
    if (err != 0) {
        return (0, error.As(Errno(err))!);
    }
    return (t, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfDatalink(nint fd, nint t) {
    nint _p0 = default;
    error _p0 = default!;

    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSDLT, uintptr(@unsafe.Pointer(_addr_t)));
    if (err != 0) {
        return (0, error.As(Errno(err))!);
    }
    return (t, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfPromisc(nint fd, nint m) {
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCPROMISC, uintptr(@unsafe.Pointer(_addr_m)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error FlushBpf(nint fd) {
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCFLUSH, 0);
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

private partial struct ivalue {
    public array<byte> name;
    public short value;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (@string, error) BpfInterface(nint fd, @string name) {
    @string _p0 = default;
    error _p0 = default!;

    ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGETIF, uintptr(@unsafe.Pointer(_addr_iv)));
    if (err != 0) {
        return ("", error.As(Errno(err))!);
    }
    return (name, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfInterface(nint fd, @string name) {
    ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
    copy(iv.name[..], (slice<byte>)name);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETIF, uintptr(@unsafe.Pointer(_addr_iv)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ptr<Timeval>, error) BpfTimeout(nint fd) {
    ptr<Timeval> _p0 = default!;
    error _p0 = default!;

    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGRTIMEOUT, uintptr(@unsafe.Pointer(_addr_tv)));
    if (err != 0) {
        return (_addr_null!, error.As(Errno(err))!);
    }
    return (_addr__addr_tv!, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfTimeout(nint fd, ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSRTIMEOUT, uintptr(@unsafe.Pointer(tv)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ptr<BpfStat>, error) BpfStats(nint fd) {
    ptr<BpfStat> _p0 = default!;
    error _p0 = default!;

    ref BpfStat s = ref heap(out ptr<BpfStat> _addr_s);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGSTATS, uintptr(@unsafe.Pointer(_addr_s)));
    if (err != 0) {
        return (_addr_null!, error.As(Errno(err))!);
    }
    return (_addr__addr_s!, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfImmediate(nint fd, nint m) {
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCIMMEDIATE, uintptr(@unsafe.Pointer(_addr_m)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpf(nint fd, slice<BpfInsn> i) {
    ref BpfProgram p = ref heap(out ptr<BpfProgram> _addr_p);
    p.Len = uint32(len(i));
    p.Insns = (BpfInsn.val)(@unsafe.Pointer(_addr_i[0]));
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETF, uintptr(@unsafe.Pointer(_addr_p)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error CheckBpfVersion(nint fd) {
    ref BpfVersion v = ref heap(out ptr<BpfVersion> _addr_v);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCVERSION, uintptr(@unsafe.Pointer(_addr_v)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    if (v.Major != BPF_MAJOR_VERSION || v.Minor != BPF_MINOR_VERSION) {
        return error.As(EINVAL)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfHeadercmpl(nint fd) {
    nint _p0 = default;
    error _p0 = default!;

    ref nint f = ref heap(out ptr<nint> _addr_f);
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGHDRCMPLT, uintptr(@unsafe.Pointer(_addr_f)));
    if (err != 0) {
        return (0, error.As(Errno(err))!);
    }
    return (f, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfHeadercmpl(nint fd, nint f) {
    var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSHDRCMPLT, uintptr(@unsafe.Pointer(_addr_f)));
    if (err != 0) {
        return error.As(Errno(err))!;
    }
    return error.As(null!)!;

}

} // end syscall_package
