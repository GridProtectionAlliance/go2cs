// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The toolexec program is a helper program for cmd/cover tests.
// It is used so that the go tool will call the newly built version
// of the cover program, rather than the installed one.
//
// The tests arrange to run the go tool with the argument
//    -toolexec="/path/to/toolexec /path/to/testcover"
// The go tool will invoke this program (compiled into /path/to/toolexec)
// with the arguments shown above followed by the command to run.
// This program will check whether it is expected to run the cover
// program, and if so replace it with /path/to/testcover.

// package main -- go2cs converted at 2022 March 13 06:28:41 UTC
// Original source: C:\Program Files\Go\src\cmd\cover\testdata\toolexec.go
namespace go;

using exec = @internal.execabs_package;
using os = os_package;
using strings = strings_package;

public static partial class main_package {

private static void Main() {
    if (strings.HasSuffix(strings.TrimSuffix(os.Args[2], ".exe"), "cover")) {
        os.Args[2] = os.Args[1];
    }
    var cmd = exec.Command(os.Args[2], os.Args[(int)3..]);
    cmd.Stdout = os.Stdout;
    cmd.Stderr = os.Stderr;
    {
        var err = cmd.Run();

        if (err != null) {
            os.Exit(1);
        }
    }
}

} // end main_package
