// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2022 March 13 06:41:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\ioctl.go
namespace go.cmd.vendor.golang.org.x.sys;

using runtime = runtime_package;
using @unsafe = @unsafe_package;


// ioctl itself should not be exposed directly, but additional get/set
// functions for specific types are permissible.

// IoctlSetInt performs an ioctl operation which sets an integer value
// on fd, using the specified request number.

public static partial class unix_package {

public static error IoctlSetInt(nint fd, nuint req, nint value) {
    return error.As(ioctl(fd, req, uintptr(value)))!;
}

// IoctlSetPointerInt performs an ioctl operation which sets an
// integer value on fd, using the specified request number. The ioctl
// argument is called with a pointer to the integer value, rather than
// passing the integer value directly.
public static error IoctlSetPointerInt(nint fd, nuint req, nint value) {
    ref var v = ref heap(int32(value), out ptr<var> _addr_v);
    return error.As(ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_v))))!;
}

// IoctlSetWinsize performs an ioctl on fd with a *Winsize argument.
//
// To change fd's window size, the req argument should be TIOCSWINSZ.
public static error IoctlSetWinsize(nint fd, nuint req, ptr<Winsize> _addr_value) {
    ref Winsize value = ref _addr_value.val;
 
    // TODO: if we get the chance, remove the req parameter and
    // hardcode TIOCSWINSZ.
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

// IoctlSetTermios performs an ioctl on fd with a *Termios.
//
// The req value will usually be TCSETA or TIOCSETA.
public static error IoctlSetTermios(nint fd, nuint req, ptr<Termios> _addr_value) {
    ref Termios value = ref _addr_value.val;
 
    // TODO: if we get the chance, remove the req parameter.
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(value)));
    runtime.KeepAlive(value);
    return error.As(err)!;
}

// IoctlGetInt performs an ioctl operation which gets an integer value
// from fd, using the specified request number.
//
// A few ioctl requests use the return value as an output parameter;
// for those, IoctlRetInt should be used instead of this function.
public static (nint, error) IoctlGetInt(nint fd, nuint req) {
    nint _p0 = default;
    error _p0 = default!;

    ref nint value = ref heap(out ptr<nint> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    return (value, error.As(err)!);
}

public static (ptr<Winsize>, error) IoctlGetWinsize(nint fd, nuint req) {
    ptr<Winsize> _p0 = default!;
    error _p0 = default!;

    ref Winsize value = ref heap(out ptr<Winsize> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

public static (ptr<Termios>, error) IoctlGetTermios(nint fd, nuint req) {
    ptr<Termios> _p0 = default!;
    error _p0 = default!;

    ref Termios value = ref heap(out ptr<Termios> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    return (_addr__addr_value!, error.As(err)!);
}

} // end unix_package
