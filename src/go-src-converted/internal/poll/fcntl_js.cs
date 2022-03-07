// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package poll -- go2cs converted at 2022 March 06 22:12:56 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\fcntl_js.go
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

    // fcntl not supported on js/wasm
private static (nint, error) fcntl(nint fd, nint cmd, nint arg) {
    nint _p0 = default;
    error _p0 = default!;

    return (0, error.As(syscall.ENOSYS)!);
}

} // end poll_package
