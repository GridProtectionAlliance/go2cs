// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// Package windows contains an interface to the low-level operating system
// primitives. OS details vary depending on the underlying system, and
// by default, godoc will display the OS-specific documentation for the current
// system. If you want godoc to display syscall documentation for another
// system, set $GOOS and $GOARCH to the desired system. For example, if
// you want to view documentation for freebsd/arm on linux/amd64, set $GOOS
// to freebsd and $GOARCH to arm.
//
// The primary use of this package is inside other packages that provide a more
// portable interface to the system, such as "os", "time" and "net".  Use
// those packages rather than this one if you can.
//
// For details of the functions and data types in this package consult
// the manuals for the appropriate operating system.
//
// These calls return err == nil to indicate success; otherwise
// err represents an operating system error describing the failure and
// holds a value of type syscall.Errno.

// package windows -- go2cs converted at 2022 March 13 06:41:29 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\syscall.go
namespace go.cmd.vendor.golang.org.x.sys;
// import "golang.org/x/sys/windows"


using bytes = bytes_package;
using strings = strings_package;
using syscall = syscall_package;
using @unsafe = @unsafe_package;

using unsafeheader = golang.org.x.sys.@internal.unsafeheader_package;


// ByteSliceFromString returns a NUL-terminated slice of bytes
// containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, syscall.EINVAL).

public static partial class windows_package {

public static (slice<byte>, error) ByteSliceFromString(@string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (strings.IndexByte(s, 0) != -1) {
        return (null, error.As(syscall.EINVAL)!);
    }
    var a = make_slice<byte>(len(s) + 1);
    copy(a, s);
    return (a, error.As(null!)!);
}

// BytePtrFromString returns a pointer to a NUL-terminated array of
// bytes containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, syscall.EINVAL).
public static (ptr<byte>, error) BytePtrFromString(@string s) {
    ptr<byte> _p0 = default!;
    error _p0 = default!;

    var (a, err) = ByteSliceFromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_a[0]!, error.As(null!)!);
}

// ByteSliceToString returns a string form of the text represented by the slice s, with a terminating NUL and any
// bytes after the NUL removed.
public static @string ByteSliceToString(slice<byte> s) {
    {
        var i = bytes.IndexByte(s, 0);

        if (i != -1) {
            s = s[..(int)i];
        }
    }
    return string(s);
}

// BytePtrToString takes a pointer to a sequence of text and returns the corresponding string.
// If the pointer is nil, it returns the empty string. It assumes that the text sequence is terminated
// at a zero byte; if the zero byte is not present, the program may crash.
public static @string BytePtrToString(ptr<byte> _addr_p) {
    ref byte p = ref _addr_p.val;

    if (p == null) {
        return "";
    }
    if (p == 0.val) {
        return "";
    }
    nint n = 0;
    for (var ptr = @unsafe.Pointer(p); new ptr<ptr<ptr<byte>>>(ptr) != 0; n++) {
        ptr = @unsafe.Pointer(uintptr(ptr) + 1);
    }

    ref slice<byte> s = ref heap(out ptr<slice<byte>> _addr_s);
    var h = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
    h.Data = @unsafe.Pointer(p);
    h.Len = n;
    h.Cap = n;

    return string(s);
}

// Single-word zero for use when we need a valid pointer to 0 bytes.
// See mksyscall.pl.
private static System.UIntPtr _zero = default;

private static (long, long) Unix(this ptr<Timespec> _addr_ts) {
    long sec = default;
    long nsec = default;
    ref Timespec ts = ref _addr_ts.val;

    return (int64(ts.Sec), int64(ts.Nsec));
}

private static (long, long) Unix(this ptr<Timeval> _addr_tv) {
    long sec = default;
    long nsec = default;
    ref Timeval tv = ref _addr_tv.val;

    return (int64(tv.Sec), int64(tv.Usec) * 1000);
}

private static long Nano(this ptr<Timespec> _addr_ts) {
    ref Timespec ts = ref _addr_ts.val;

    return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
}

private static long Nano(this ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    return int64(tv.Sec) * 1e9F + int64(tv.Usec) * 1000;
}

} // end windows_package
