// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package syscall contains an interface to the low-level operating system
// primitives. The details vary depending on the underlying system, and
// by default, godoc will display the syscall documentation for the current
// system. If you want godoc to display syscall documentation for another
// system, set $GOOS and $GOARCH to the desired system. For example, if
// you want to view documentation for freebsd/arm on linux/amd64, set $GOOS
// to freebsd and $GOARCH to arm.
// The primary use of syscall is inside other packages that provide a more
// portable interface to the system, such as "os", "time" and "net".  Use
// those packages rather than this one if you can.
// For details of the functions and data types in this package consult
// the manuals for the appropriate operating system.
// These calls return err == nil to indicate success; otherwise
// err is an operating system error describing the failure.
// On most systems, that error has type syscall.Errno.
//
// Deprecated: this package is locked down. Callers should use the
// corresponding package in the golang.org/x/sys repository instead.
// That is also where updates required by new systems or versions
// should be applied. See https://golang.org/s/go1.4-syscall for more
// information.
//

// package syscall -- go2cs converted at 2022 March 13 05:40:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall.go
namespace go;

public static partial class syscall_package {

//go:generate go run ./mksyscall_windows.go -systemdll -output zsyscall_windows.go syscall_windows.go security_windows.go

// StringByteSlice converts a string to a NUL-terminated []byte,
// If s contains a NUL byte this function panics instead of
// returning an error.
//
// Deprecated: Use ByteSliceFromString instead.
public static slice<byte> StringByteSlice(@string s) => func((_, panic, _) => {
    var (a, err) = ByteSliceFromString(s);
    if (err != null) {
        panic("syscall: string with NUL passed to StringByteSlice");
    }
    return a;
});

// ByteSliceFromString returns a NUL-terminated slice of bytes
// containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, EINVAL).
public static (slice<byte>, error) ByteSliceFromString(@string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    for (nint i = 0; i < len(s); i++) {
        if (s[i] == 0) {
            return (null, error.As(EINVAL)!);
        }
    }
    var a = make_slice<byte>(len(s) + 1);
    copy(a, s);
    return (a, error.As(null!)!);
}

// StringBytePtr returns a pointer to a NUL-terminated array of bytes.
// If s contains a NUL byte this function panics instead of returning
// an error.
//
// Deprecated: Use BytePtrFromString instead.
public static ptr<byte> StringBytePtr(@string s) {
    return _addr__addr_StringByteSlice(s)[0]!;
}

// BytePtrFromString returns a pointer to a NUL-terminated array of
// bytes containing the text of s. If s contains a NUL byte at any
// location, it returns (nil, EINVAL).
public static (ptr<byte>, error) BytePtrFromString(@string s) {
    ptr<byte> _p0 = default!;
    error _p0 = default!;

    var (a, err) = ByteSliceFromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_a[0]!, error.As(null!)!);
}

// Single-word zero for use when we need a valid pointer to 0 bytes.
// See mksyscall.pl.
private static System.UIntPtr _zero = default;

// Unix returns the time stored in ts as seconds plus nanoseconds.
private static (long, long) Unix(this ptr<Timespec> _addr_ts) {
    long sec = default;
    long nsec = default;
    ref Timespec ts = ref _addr_ts.val;

    return (int64(ts.Sec), int64(ts.Nsec));
}

// Unix returns the time stored in tv as seconds plus nanoseconds.
private static (long, long) Unix(this ptr<Timeval> _addr_tv) {
    long sec = default;
    long nsec = default;
    ref Timeval tv = ref _addr_tv.val;

    return (int64(tv.Sec), int64(tv.Usec) * 1000);
}

// Nano returns the time stored in ts as nanoseconds.
private static long Nano(this ptr<Timespec> _addr_ts) {
    ref Timespec ts = ref _addr_ts.val;

    return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
}

// Nano returns the time stored in tv as nanoseconds.
private static long Nano(this ptr<Timeval> _addr_tv) {
    ref Timeval tv = ref _addr_tv.val;

    return int64(tv.Sec) * 1e9F + int64(tv.Usec) * 1000;
}

// Getpagesize and Exit are provided by the runtime.

public static nint Getpagesize();
public static void Exit(nint code);

} // end syscall_package
