// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9
// +build plan9

// package os -- go2cs converted at 2022 March 06 22:13:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_plan9.go
using itoa = go.@internal.itoa_package;
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static (@string, error) executable() => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;

    @string fn = "/proc/" + itoa.Itoa(Getpid()) + "/text";
    var (f, err) = Open(fn);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(f.Close());
    return syscall.Fd2path(int(f.Fd()));

});

} // end os_package
