// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// package unix -- go2cs converted at 2022 March 06 23:26:37 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\ioctl_zos.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // ioctl itself should not be exposed directly, but additional get/set
    // functions for specific types are permissible.

    // IoctlSetInt performs an ioctl operation which sets an integer value
    // on fd, using the specified request number.
public static error IoctlSetInt(nint fd, nuint req, nint value) {
    return error.As(ioctl(fd, req, uintptr(value)))!;
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
// The req value is expected to be TCSETS, TCSETSW, or TCSETSF
public static error IoctlSetTermios(nint fd, nuint req, ptr<Termios> _addr_value) {
    ref Termios value = ref _addr_value.val;

    if ((req != TCSETS) && (req != TCSETSW) && (req != TCSETSF)) {
        return error.As(ENOSYS)!;
    }
    var err = Tcsetattr(fd, int(req), value);
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

// IoctlGetTermios performs an ioctl on fd with a *Termios.
//
// The req value is expected to be TCGETS
public static (ptr<Termios>, error) IoctlGetTermios(nint fd, nuint req) {
    ptr<Termios> _p0 = default!;
    error _p0 = default!;

    ref Termios value = ref heap(out ptr<Termios> _addr_value);
    if (req != TCGETS) {
        return (_addr__addr_value!, error.As(ENOSYS)!);
    }
    var err = Tcgetattr(fd, _addr_value);
    return (_addr__addr_value!, error.As(err)!);

}

} // end unix_package
