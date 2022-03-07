// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:53 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys_plan9.go


namespace go;

public static partial class os_package {

private static (@string, error) hostname() => func((defer, _, _) => {
    @string name = default;
    error err = default!;

    var (f, err) = Open("#c/sysname");
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(f.Close());

    array<byte> buf = new array<byte>(128);
    var (n, err) = f.Read(buf[..(int)len(buf) - 1]);

    if (err != null) {
        return ("", error.As(err)!);
    }
    if (n > 0) {
        buf[n] = 0;
    }
    return (string(buf[(int)0..(int)n]), error.As(null!)!);

});

} // end os_package
