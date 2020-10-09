// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package unix -- go2cs converted at 2020 October 09 04:50:59 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\nonblocking_js.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        public static (bool, error) IsNonblock(long fd)
        {
            bool nonblocking = default;
            error err = default!;

            return (false, error.As(null!)!);
        }
    }
}}}
