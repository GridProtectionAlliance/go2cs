// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package exec -- go2cs converted at 2020 October 08 03:41:17 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\exec_unix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go {
namespace os
{
    public static partial class exec_package
    {
        private static void init()
        {
            skipStdinCopyError = err =>
            { 
                // Ignore EPIPE errors copying to stdin if the program
                // completed successfully otherwise.
                // See Issue 9173.
                ptr<os.PathError> (pe, ok) = err._<ptr<os.PathError>>();
                return ok && pe.Op == "write" && pe.Path == "|1" && pe.Err == syscall.EPIPE;

            };

        }
    }
}}
