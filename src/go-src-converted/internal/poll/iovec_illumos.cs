// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build illumos
// +build illumos

// package poll -- go2cs converted at 2022 March 06 22:13:18 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\iovec_illumos.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class poll_package {

private static syscall.Iovec newIovecWithBase(ptr<byte> _addr_@base) {
    ref byte @base = ref _addr_@base.val;

    return new syscall.Iovec(Base:(*int8)(unsafe.Pointer(base)));
}

} // end poll_package
