// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || dragonfly || freebsd || linux || netbsd || openbsd
// +build aix dragonfly freebsd linux netbsd openbsd

// package unix -- go2cs converted at 2022 March 13 06:41:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\readdirent_getdents.go
namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

// ReadDirent reads directory entries from fd and writes them into buf.
public static (nint, error) ReadDirent(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return Getdents(fd, buf);
}

} // end unix_package
