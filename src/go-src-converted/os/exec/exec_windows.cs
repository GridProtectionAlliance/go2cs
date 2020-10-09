// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2020 October 09 04:58:42 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\exec_windows.go
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
                // Ignore ERROR_BROKEN_PIPE and ERROR_NO_DATA errors copying
                // to stdin if the program completed successfully otherwise.
                // See Issue 20445.
                const var _ERROR_NO_DATA = syscall.Errno(0xe8UL);

                ptr<os.PathError> (pe, ok) = err._<ptr<os.PathError>>();
                return ok && pe.Op == "write" && pe.Path == "|1" && (pe.Err == syscall.ERROR_BROKEN_PIPE || pe.Err == _ERROR_NO_DATA);

            };

        }
    }
}}
