// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin

// package poll -- go2cs converted at 2020 October 09 04:51:19 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_writev_darwin.go
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        // Implemented in syscall/syscall_darwin.go.
        //go:linkname writev syscall.writev
        private static (System.UIntPtr, error) writev(long fd, slice<syscall.Iovec> iovecs)
;
    }
}}
