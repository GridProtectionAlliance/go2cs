// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package poll -- go2cs converted at 2020 October 08 03:32:06 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fcntl_js.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // fcntl not supported on js/wasm
        private static (long, error) fcntl(long fd, long cmd, long arg)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(syscall.ENOSYS)!);
        }
    }
}}
