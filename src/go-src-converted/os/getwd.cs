// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\getwd.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static var getwdCache = default;

// Getwd returns a rooted path name corresponding to the
// current directory. If the current directory can be
// reached via multiple paths (due to symbolic links),
// Getwd may return any one of them.
public static (@string, error) Getwd() {
    @string dir = default;
    error err = default!;

    if (runtime.GOOS == "windows" || runtime.GOOS == "plan9") {
        return syscall.Getwd();
    }
    var (dot, err) = statNolog(".");
    if (err != null) {
        return ("", error.As(err)!);
    }
    dir = Getenv("PWD");
    if (len(dir) > 0 && dir[0] == '/') {
        var (d, err) = statNolog(dir);
        if (err == null && SameFile(dot, d)) {
            return (dir, error.As(null!)!);
        }
    }
    if (syscall.ImplementsGetwd) {
        @string s = default;        error e = default!;
        while (true) {
            s, e = syscall.Getwd();
            if (e != syscall.EINTR) {
                break;
            }
        }
        return (s, error.As(NewSyscallError("getwd", e))!);
    }
    getwdCache.Lock();
    dir = getwdCache.dir;
    getwdCache.Unlock();
    if (len(dir) > 0) {
        (d, err) = statNolog(dir);
        if (err == null && SameFile(dot, d)) {
            return (dir, error.As(null!)!);
        }
    }
    var (root, err) = statNolog("/");
    if (err != null) { 
        // Can't stat root - no hope.
        return ("", error.As(err)!);

    }
    if (SameFile(root, dot)) {
        return ("/", error.As(null!)!);
    }
    dir = "";
    {
        @string parent = "..";

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            if (len(parent) >= 1024) { // Sanity check
                return ("", error.As(syscall.ENAMETOOLONG)!);
            parent = "../" + parent;
            }

            var (fd, err) = openFileNolog(parent, O_RDONLY, 0);
            if (err != null) {
                return ("", error.As(err)!);
            }

            while (true) {
                var (names, err) = fd.Readdirnames(100);
                if (err != null) {
                    fd.Close();
                    return ("", error.As(err)!);
                }
                foreach (var (_, name) in names) {
                    var (d, _) = lstatNolog(parent + "/" + name);
                    if (SameFile(d, dot)) {
                        dir = "/" + name + dir;
                        goto Found;
                    }
                }
            }


Found:
            var (pd, err) = fd.Stat();
            fd.Close();
            if (err != null) {
                return ("", error.As(err)!);
            }

            if (SameFile(pd, root)) {
                break;
            } 
            // Set up for next round.
            dot = pd;

        }
    } 

    // Save answer as hint to avoid the expensive path next time.
    getwdCache.Lock();
    getwdCache.dir = dir;
    getwdCache.Unlock();

    return (dir, error.As(null!)!);

}

} // end os_package
