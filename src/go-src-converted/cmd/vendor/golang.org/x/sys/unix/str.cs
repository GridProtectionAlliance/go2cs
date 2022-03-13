// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2022 March 13 06:41:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\str.go
namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static @string itoa(nint val) { // do it here rather than with fmt to avoid dependency
    if (val < 0) {
        return "-" + uitoa(uint(-val));
    }
    return uitoa(uint(val));
}

private static @string uitoa(nuint val) {
    array<byte> buf = new array<byte>(32); // big enough for int64
    var i = len(buf) - 1;
    while (val >= 10) {
        buf[i] = byte(val % 10 + '0');
        i--;
        val /= 10;
    }
    buf[i] = byte(val + '0');
    return string(buf[(int)i..]);
}

} // end unix_package
