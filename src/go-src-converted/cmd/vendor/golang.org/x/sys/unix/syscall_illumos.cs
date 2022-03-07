// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// illumos system calls not present on Solaris.

//go:build amd64 && illumos
// +build amd64,illumos

// package unix -- go2cs converted at 2022 March 06 23:26:55 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_illumos.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static slice<Iovec> bytes2iovec(slice<slice<byte>> bs) {
    var iovecs = make_slice<Iovec>(len(bs));
    foreach (var (i, b) in bs) {
        iovecs[i].SetLen(len(b));
        if (len(b) > 0) { 
            // somehow Iovec.Base on illumos is (*int8), not (*byte)
            iovecs[i].Base = (int8.val)(@unsafe.Pointer(_addr_b[0]));

        }
        else
 {
            iovecs[i].Base = (int8.val)(@unsafe.Pointer(_addr__zero));
        }
    }    return iovecs;

}

//sys    readv(fd int, iovs []Iovec) (n int, err error)

public static (nint, error) Readv(nint fd, slice<slice<byte>> iovs) {
    nint n = default;
    error err = default!;

    var iovecs = bytes2iovec(iovs);
    n, err = readv(fd, iovecs);
    return (n, error.As(err)!);
}

//sys    preadv(fd int, iovs []Iovec, off int64) (n int, err error)

public static (nint, error) Preadv(nint fd, slice<slice<byte>> iovs, long off) {
    nint n = default;
    error err = default!;

    var iovecs = bytes2iovec(iovs);
    n, err = preadv(fd, iovecs, off);
    return (n, error.As(err)!);
}

//sys    writev(fd int, iovs []Iovec) (n int, err error)

public static (nint, error) Writev(nint fd, slice<slice<byte>> iovs) {
    nint n = default;
    error err = default!;

    var iovecs = bytes2iovec(iovs);
    n, err = writev(fd, iovecs);
    return (n, error.As(err)!);
}

//sys    pwritev(fd int, iovs []Iovec, off int64) (n int, err error)

public static (nint, error) Pwritev(nint fd, slice<slice<byte>> iovs, long off) {
    nint n = default;
    error err = default!;

    var iovecs = bytes2iovec(iovs);
    n, err = pwritev(fd, iovecs, off);
    return (n, error.As(err)!);
}

//sys    accept4(s int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (fd int, err error) = libsocket.accept4

public static (nint, Sockaddr, error) Accept4(nint fd, nint flags) => func((_, panic, _) => {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    nfd, err = accept4(fd, _addr_rsa, _addr_len, flags);
    if (err != null) {
        return ;
    }
    if (len > SizeofSockaddrAny) {
        panic("RawSockaddrAny too small");
    }
    sa, err = anyToSockaddr(fd, _addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

});

//sys    putmsg(fd int, clptr *strbuf, dataptr *strbuf, flags int) (err error)

public static error Putmsg(nint fd, slice<byte> cl, slice<byte> data, nint flags) {
    error err = default!;

    ptr<strbuf> clp;    ptr<strbuf> datap;

    if (len(cl) > 0) {
        clp = addr(new strbuf(Len:int32(len(cl)),Buf:(*int8)(unsafe.Pointer(&cl[0])),));
    }
    if (len(data) > 0) {
        datap = addr(new strbuf(Len:int32(len(data)),Buf:(*int8)(unsafe.Pointer(&data[0])),));
    }
    return error.As(putmsg(fd, clp, datap, flags))!;

}

//sys    getmsg(fd int, clptr *strbuf, dataptr *strbuf, flags *int) (err error)

public static (slice<byte>, slice<byte>, nint, error) Getmsg(nint fd, slice<byte> cl, slice<byte> data) {
    slice<byte> retCl = default;
    slice<byte> retData = default;
    nint flags = default;
    error err = default!;

    ptr<strbuf> clp;    ptr<strbuf> datap;

    if (len(cl) > 0) {
        clp = addr(new strbuf(Maxlen:int32(len(cl)),Buf:(*int8)(unsafe.Pointer(&cl[0])),));
    }
    if (len(data) > 0) {
        datap = addr(new strbuf(Maxlen:int32(len(data)),Buf:(*int8)(unsafe.Pointer(&data[0])),));
    }
    err = getmsg(fd, clp, datap, _addr_flags);

    if (err != null) {
        return (null, null, 0, error.As(err)!);
    }
    if (len(cl) > 0) {
        retCl = cl[..(int)clp.Len];
    }
    if (len(data) > 0) {
        retData = data[..(int)datap.Len];
    }
    return (retCl, retData, flags, error.As(null!)!);

}

public static (nint, error) IoctlSetIntRetInt(nint fd, nuint req, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    return ioctlRet(fd, req, uintptr(arg));
}

public static error IoctlSetString(nint fd, nuint req, @string val) {
    var bs = make_slice<byte>(len(val) + 1);
    copy(bs[..(int)len(bs) - 1], val);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_bs[0])));
    runtime.KeepAlive(_addr_bs[0]);
    return error.As(err)!;
}

// Lifreq Helpers

private static error SetName(this ptr<Lifreq> _addr_l, @string name) {
    ref Lifreq l = ref _addr_l.val;

    if (len(name) >= len(l.Name)) {
        return error.As(fmt.Errorf("name cannot be more than %d characters", len(l.Name) - 1))!;
    }
    foreach (var (i) in name) {
        l.Name[i] = int8(name[i]);
    }    return error.As(null!)!;

}

private static void SetLifruInt(this ptr<Lifreq> _addr_l, nint d) {
    ref Lifreq l = ref _addr_l.val;

    (int.val)(@unsafe.Pointer(_addr_l.Lifru[0])).val;

    d;

}

private static nint GetLifruInt(this ptr<Lifreq> _addr_l) {
    ref Lifreq l = ref _addr_l.val;

    return new ptr<ptr<ptr<nint>>>(@unsafe.Pointer(_addr_l.Lifru[0]));
}

public static error IoctlLifreq(nint fd, nuint req, ptr<Lifreq> _addr_l) {
    ref Lifreq l = ref _addr_l.val;

    return error.As(ioctl(fd, req, uintptr(@unsafe.Pointer(l))))!;
}

// Strioctl Helpers

private static void SetInt(this ptr<Strioctl> _addr_s, nint i) {
    ref Strioctl s = ref _addr_s.val;

    s.Len = int32(@unsafe.Sizeof(i));
    s.Dp = (int8.val)(@unsafe.Pointer(_addr_i));
}

public static (nint, error) IoctlSetStrioctlRetInt(nint fd, nuint req, ptr<Strioctl> _addr_s) {
    nint _p0 = default;
    error _p0 = default!;
    ref Strioctl s = ref _addr_s.val;

    return ioctlRet(fd, req, uintptr(@unsafe.Pointer(s)));
}

} // end unix_package
