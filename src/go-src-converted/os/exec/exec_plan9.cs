// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2022 March 06 22:14:22 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Program Files\Go\src\os\exec\exec_plan9.go
using fs = go.io.fs_package;
using System;


namespace go.os;

public static partial class exec_package {

private static void init() {
    skipStdinCopyError = err => { 
        // Ignore hungup errors copying to stdin if the program
        // completed successfully otherwise.
        // See Issue 35753.
        ptr<fs.PathError> (pe, ok) = err._<ptr<fs.PathError>>();
        return ok && pe.Op == "write" && pe.Path == "|1" && pe.Err.Error() == "i/o on hungup channel";

    };

}

} // end exec_package
