// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2022 March 06 22:14:23 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Program Files\Go\src\os\exec\exec_windows.go
using fs = go.io.fs_package;
using syscall = go.syscall_package;
using System;


namespace go.os;

public static partial class exec_package {

private static void init() {
    skipStdinCopyError = err => { 
        // Ignore ERROR_BROKEN_PIPE and ERROR_NO_DATA errors copying
        // to stdin if the program completed successfully otherwise.
        // See Issue 20445.
        const var _ERROR_NO_DATA = syscall.Errno(0xe8);

        ptr<fs.PathError> (pe, ok) = err._<ptr<fs.PathError>>();
        return ok && pe.Op == "write" && pe.Path == "|1" && (pe.Err == syscall.ERROR_BROKEN_PIPE || pe.Err == _ERROR_NO_DATA);

    };

}

} // end exec_package
