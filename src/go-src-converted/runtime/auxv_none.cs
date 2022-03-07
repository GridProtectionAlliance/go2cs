// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !linux && !darwin && !dragonfly && !freebsd && !netbsd && !solaris
// +build !linux,!darwin,!dragonfly,!freebsd,!netbsd,!solaris

// package runtime -- go2cs converted at 2022 March 06 22:08:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\auxv_none.go


namespace go;

public static partial class runtime_package {

private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv) {
    ref ptr<byte> argv = ref _addr_argv.val;

}

} // end runtime_package
