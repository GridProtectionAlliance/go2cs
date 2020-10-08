// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:21 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error_plan9.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private partial struct syscallErrorType // : syscall.ErrorString
        {
        }
    }
}
