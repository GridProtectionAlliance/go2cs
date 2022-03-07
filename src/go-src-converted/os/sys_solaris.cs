// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:53 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys_solaris.go
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static (@string, error) hostname() {
    @string name = default;
    error err = default!;

    return syscall.Gethostname();
}

} // end os_package
