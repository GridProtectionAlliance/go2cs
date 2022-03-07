// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The working directory in Plan 9 is effectively per P, so different
// goroutines and even the same goroutine as it's rescheduled on
// different Ps can see different working directories.
//
// Instead, track a Go process-wide intent of the current working directory,
// and switch to it at important points.

// package syscall -- go2cs converted at 2022 March 06 22:26:40 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\pwd_plan9.go
using sync = go.sync_package;

namespace go;

public static partial class syscall_package {

private static sync.Mutex wdmu = default;private static bool wdSet = default;private static @string wdStr = default;

public static void Fixwd() => func((defer, _, _) => {
    wdmu.Lock();
    defer(wdmu.Unlock());
    fixwdLocked();
});

private static void fixwdLocked() {
    if (!wdSet) {
        return ;
    }
    var (wd, _) = getwd();
    if (wd == wdStr) {
        return ;
    }
    {
        var err = chdir(wdStr);

        if (err != null) {
            return ;
        }
    }

}

private static void fixwd(params @string[] paths) {
    paths = paths.Clone();

    foreach (var (_, path) in paths) {
        if (path != "" && path[0] != '/' && path[0] != '#') {
            Fixwd();
            return ;
        }
    }
}

// goroutine-specific getwd
private static (@string, error) getwd() => func((defer, _, _) => {
    @string wd = default;
    error err = default!;

    var (fd, err) = open(".", O_RDONLY);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(Close(fd));
    return Fd2path(fd);

});

public static (@string, error) Getwd() => func((defer, _, _) => {
    @string wd = default;
    error err = default!;

    wdmu.Lock();
    defer(wdmu.Unlock());

    if (wdSet) {
        return (wdStr, error.As(null!)!);
    }
    wd, err = getwd();
    if (err != null) {
        return ;
    }
    wdSet = true;
    wdStr = wd;
    return (wd, error.As(null!)!);

});

public static error Chdir(@string path) => func((defer, _, _) => {
    fixwd(path);
    wdmu.Lock();
    defer(wdmu.Unlock());

    {
        var err = chdir(path);

        if (err != null) {
            return error.As(err)!;
        }
    }


    var (wd, err) = getwd();
    if (err != null) {
        return error.As(err)!;
    }
    wdSet = true;
    wdStr = wd;
    return error.As(null!)!;

});

} // end syscall_package
