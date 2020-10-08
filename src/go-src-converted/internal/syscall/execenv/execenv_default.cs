// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows

// package execenv -- go2cs converted at 2020 October 08 03:40:57 UTC
// import "internal/syscall/execenv" ==> using execenv = go.@internal.syscall.execenv_package
// Original source: C:\Go\src\internal\syscall\execenv\execenv_default.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class execenv_package
    {
        // Default will return the default environment
        // variables based on the process attributes
        // provided.
        //
        // Defaults to syscall.Environ() on all platforms
        // other than Windows.
        public static (slice<@string>, error) Default(ptr<syscall.SysProcAttr> _addr_sys)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref syscall.SysProcAttr sys = ref _addr_sys.val;

            return (syscall.Environ(), error.As(null!)!);
        }
    }
}}}
