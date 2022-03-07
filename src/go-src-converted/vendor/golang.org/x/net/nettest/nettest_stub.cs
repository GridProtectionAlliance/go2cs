// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !dragonfly && !freebsd && !linux && !netbsd && !openbsd && !solaris && !windows && !zos
// +build !aix,!darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd,!solaris,!windows,!zos

// package nettest -- go2cs converted at 2022 March 06 23:38:10 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\nettest\nettest_stub.go


namespace go.vendor.golang.org.x.net;

public static partial class nettest_package {

private static bool supportsRawSocket() {
    return false;
}

} // end nettest_package
