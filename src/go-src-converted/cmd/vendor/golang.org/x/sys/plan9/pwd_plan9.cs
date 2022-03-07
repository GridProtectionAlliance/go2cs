// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.5

// package plan9 -- go2cs converted at 2022 March 06 23:26:24 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\pwd_plan9.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class plan9_package {

private static void fixwd() {
}

public static (@string, error) Getwd() => func((defer, _, _) => {
    @string wd = default;
    error err = default!;

    var (fd, err) = open(".", O_RDONLY);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(Close(fd));
    return Fd2path(fd);

});

public static error Chdir(@string path) {
    return error.As(chdir(path))!;
}

} // end plan9_package
