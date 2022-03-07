// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Berkeley packet filter for Darwin

// package syscall -- go2cs converted at 2022 March 06 22:08:07 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\bpf_darwin.go
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
    var err = ioctlPtr(fd, BIOCGBLEN, @unsafe.Pointer(_addr_l));
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (l, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfBuflen(nint fd, nint l) {
    nint _p0 = default;
    error _p0 = default!;

    var err = ioctlPtr(fd, BIOCSBLEN, @unsafe.Pointer(_addr_l));
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (l, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfDatalink(nint fd) {
    nint _p0 = default;
    error _p0 = default!;

    ref nint t = ref heap(out ptr<nint> _addr_t);
    var err = ioctlPtr(fd, BIOCGDLT, @unsafe.Pointer(_addr_t));
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (t, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfDatalink(nint fd, nint t) {
    nint _p0 = default;
    error _p0 = default!;

    var err = ioctlPtr(fd, BIOCSDLT, @unsafe.Pointer(_addr_t));
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (t, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfPromisc(nint fd, nint m) {
    var err = ioctlPtr(fd, BIOCPROMISC, @unsafe.Pointer(_addr_m));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error FlushBpf(nint fd) {
    var err = ioctlPtr(fd, BIOCFLUSH, null);
    if (err != null) {
        return error.As(err)!;
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
    var err = ioctlPtr(fd, BIOCGETIF, @unsafe.Pointer(_addr_iv));
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (name, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfInterface(nint fd, @string name) {
    ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
    copy(iv.name[..], (slice<byte>)name);
    var err = ioctlPtr(fd, BIOCSETIF, @unsafe.Pointer(_addr_iv));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ptr<Timeval>, error) BpfTimeout(nint fd) {
    ptr<Timeval> _p0 = default!;
    error _p0 = default!;

    ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
    var err = ioctlPtr(fd, BIOCGRTIMEOUT, @unsafe.Pointer(_addr_tv));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_tv!, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfTimeout(nint fd, ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    var err = ioctlPtr(fd, BIOCSRTIMEOUT, @unsafe.Pointer(tv));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ptr<BpfStat>, error) BpfStats(nint fd) {
    ptr<BpfStat> _p0 = default!;
    error _p0 = default!;

    ref BpfStat s = ref heap(out ptr<BpfStat> _addr_s);
    var err = ioctlPtr(fd, BIOCGSTATS, @unsafe.Pointer(_addr_s));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_s!, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfImmediate(nint fd, nint m) {
    var err = ioctlPtr(fd, BIOCIMMEDIATE, @unsafe.Pointer(_addr_m));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpf(nint fd, slice<BpfInsn> i) {
    ref BpfProgram p = ref heap(out ptr<BpfProgram> _addr_p);
    p.Len = uint32(len(i));
    p.Insns = (BpfInsn.val)(@unsafe.Pointer(_addr_i[0]));
    var err = ioctlPtr(fd, BIOCSETF, @unsafe.Pointer(_addr_p));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error CheckBpfVersion(nint fd) {
    ref BpfVersion v = ref heap(out ptr<BpfVersion> _addr_v);
    var err = ioctlPtr(fd, BIOCVERSION, @unsafe.Pointer(_addr_v));
    if (err != null) {
        return error.As(err)!;
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
    var err = ioctlPtr(fd, BIOCGHDRCMPLT, @unsafe.Pointer(_addr_f));
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (f, error.As(null!)!);

}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfHeadercmpl(nint fd, nint f) {
    var err = ioctlPtr(fd, BIOCSHDRCMPLT, @unsafe.Pointer(_addr_f));
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;

}

} // end syscall_package
