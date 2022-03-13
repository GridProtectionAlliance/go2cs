// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || linux || netbsd || openbsd
// +build darwin dragonfly freebsd linux netbsd openbsd

// package poll -- go2cs converted at 2022 March 13 05:27:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\iovec_unix.go
namespace go.@internal;

using syscall = syscall_package;

public static partial class poll_package {

private static syscall.Iovec newIovecWithBase(ptr<byte> _addr_@base) {
    ref byte @base = ref _addr_@base.val;

    return new syscall.Iovec(Base:base);
}

} // end poll_package
