// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build freebsd || netbsd
// +build freebsd netbsd

// package unix -- go2cs converted at 2022 March 06 23:27:27 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\xattr_bsd.go
using strings = go.strings_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Derive extattr namespace and attribute name
private static (nint, @string, error) xattrnamespace(@string fullattr) {
    nint ns = default;
    @string attr = default;
    error err = default!;

    var s = strings.IndexByte(fullattr, '.');
    if (s == -1) {
        return (-1, "", error.As(ENOATTR)!);
    }
    var @namespace = fullattr[(int)0..(int)s];
    attr = fullattr[(int)s + 1..];

    switch (namespace) {
        case "user": 
            return (EXTATTR_NAMESPACE_USER, attr, error.As(null!)!);
            break;
        case "system": 
            return (EXTATTR_NAMESPACE_SYSTEM, attr, error.As(null!)!);
            break;
        default: 
            return (-1, "", error.As(ENOATTR)!);
            break;
    }

}

private static unsafe.Pointer initxattrdest(slice<byte> dest, nint idx) {
    unsafe.Pointer d = default;

    if (len(dest) > idx) {
        return @unsafe.Pointer(_addr_dest[idx]);
    }
    else
 {
        return @unsafe.Pointer(_zero);
    }
}

// FreeBSD and NetBSD implement their own syscalls to handle extended attributes

public static (nint, error) Getxattr(@string file, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsize = len(dest);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    return ExtattrGetFile(file, nsid, a, uintptr(d), destsize);

}

public static (nint, error) Fgetxattr(nint fd, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsize = len(dest);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    return ExtattrGetFd(fd, nsid, a, uintptr(d), destsize);

}

public static (nint, error) Lgetxattr(@string link, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsize = len(dest);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    return ExtattrGetLink(link, nsid, a, uintptr(d), destsize);

}

// flags are unused on FreeBSD

public static error Fsetxattr(nint fd, @string attr, slice<byte> data, nint flags) {
    error err = default!;

    unsafe.Pointer d = default;
    if (len(data) > 0) {
        d = @unsafe.Pointer(_addr_data[0]);
    }
    var datasiz = len(data);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    _, err = ExtattrSetFd(fd, nsid, a, uintptr(d), datasiz);
    return ;

}

public static error Setxattr(@string file, @string attr, slice<byte> data, nint flags) {
    error err = default!;

    unsafe.Pointer d = default;
    if (len(data) > 0) {
        d = @unsafe.Pointer(_addr_data[0]);
    }
    var datasiz = len(data);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    _, err = ExtattrSetFile(file, nsid, a, uintptr(d), datasiz);
    return ;

}

public static error Lsetxattr(@string link, @string attr, slice<byte> data, nint flags) {
    error err = default!;

    unsafe.Pointer d = default;
    if (len(data) > 0) {
        d = @unsafe.Pointer(_addr_data[0]);
    }
    var datasiz = len(data);

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    _, err = ExtattrSetLink(link, nsid, a, uintptr(d), datasiz);
    return ;

}

public static error Removexattr(@string file, @string attr) {
    error err = default!;

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    err = ExtattrDeleteFile(file, nsid, a);
    return ;

}

public static error Fremovexattr(nint fd, @string attr) {
    error err = default!;

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    err = ExtattrDeleteFd(fd, nsid, a);
    return ;

}

public static error Lremovexattr(@string link, @string attr) {
    error err = default!;

    var (nsid, a, err) = xattrnamespace(attr);
    if (err != null) {
        return ;
    }
    err = ExtattrDeleteLink(link, nsid, a);
    return ;

}

public static (nint, error) Listxattr(@string file, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsiz = len(dest); 

    // FreeBSD won't allow you to list xattrs from multiple namespaces
    nint s = 0;
    foreach (var (_, nsid) in new array<nint>(new nint[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM })) {
        var (stmp, e) = ExtattrListFile(file, nsid, uintptr(d), destsiz);

        /* Errors accessing system attrs are ignored so that
                 * we can implement the Linux-like behavior of omitting errors that
                 * we don't have read permissions on
                 *
                 * Linux will still error if we ask for user attributes on a file that
                 * we don't have read permissions on, so don't ignore those errors
                 */
        if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER) {
            continue;
        }
        else if (e != null) {
            return (s, error.As(e)!);
        }
        s += stmp;
        destsiz -= s;
        if (destsiz < 0) {
            destsiz = 0;
        }
        d = initxattrdest(dest, s);

    }    return (s, error.As(null!)!);

}

public static (nint, error) Flistxattr(nint fd, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsiz = len(dest);

    nint s = 0;
    foreach (var (_, nsid) in new array<nint>(new nint[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM })) {
        var (stmp, e) = ExtattrListFd(fd, nsid, uintptr(d), destsiz);
        if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER) {
            continue;
        }
        else if (e != null) {
            return (s, error.As(e)!);
        }
        s += stmp;
        destsiz -= s;
        if (destsiz < 0) {
            destsiz = 0;
        }
        d = initxattrdest(dest, s);

    }    return (s, error.As(null!)!);

}

public static (nint, error) Llistxattr(@string link, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    var d = initxattrdest(dest, 0);
    var destsiz = len(dest);

    nint s = 0;
    foreach (var (_, nsid) in new array<nint>(new nint[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM })) {
        var (stmp, e) = ExtattrListLink(link, nsid, uintptr(d), destsiz);
        if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER) {
            continue;
        }
        else if (e != null) {
            return (s, error.As(e)!);
        }
        s += stmp;
        destsiz -= s;
        if (destsiz < 0) {
            destsiz = 0;
        }
        d = initxattrdest(dest, s);

    }    return (s, error.As(null!)!);

}

} // end unix_package
