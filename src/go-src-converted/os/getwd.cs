// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using runtime = runtime_package;
using sync = sync_package;
using syscall = syscall_package;

partial class os_package {


[GoType("dyn")] partial struct getwdCacheᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal @string dir;
}
internal static getwdCacheᴛ1 getwdCache;

// Getwd returns a rooted path name corresponding to the
// current directory. If the current directory can be
// reached via multiple paths (due to symbolic links),
// Getwd may return any one of them.
public static (@string dir, error err) Getwd() {
    @string dir = default!;
    error err = default!;

    if (runtime.GOOS == "windows"u8 || runtime.GOOS == "plan9"u8) {
        return syscall.Getwd();
    }
    // Clumsy but widespread kludge:
    // if $PWD is set and matches ".", use it.
    (dot, err) = statNolog("."u8);
    if (err != default!) {
        return ("", err);
    }
    dir = Getenv("PWD"u8);
    if (len(dir) > 0 && dir[0] == (rune)'/') {
        (d, errΔ1) = statNolog(dir);
        if (errΔ1 == default! && SameFile(dot, d)) {
            return (dir, default!);
        }
    }
    // If the operating system provides a Getwd call, use it.
    // Otherwise, we're trying to find our way back to ".".
    if (syscall.ImplementsGetwd) {
        @string s = default!;
        error e = default!;
        while (ᐧ) {
            (s, e) = syscall.Getwd();
            if (e != syscall.EINTR) {
                break;
            }
        }
        return (s, NewSyscallError("getwd"u8, e));
    }
    // Apply same kludge but to cached dir instead of $PWD.
    getwdCache.Lock();
    dir = getwdCache.dir;
    getwdCache.Unlock();
    if (len(dir) > 0) {
        (d, errΔ2) = statNolog(dir);
        if (errΔ2 == default! && SameFile(dot, d)) {
            return (dir, default!);
        }
    }
    // Root is a special case because it has no parent
    // and ends in a slash.
    (root, err) = statNolog("/"u8);
    if (err != default!) {
        // Can't stat root - no hope.
        return ("", err);
    }
    if (SameFile(root, dot)) {
        return ("/", default!);
    }
    // General algorithm: find name in parent
    // and then find name of parent. Each iteration
    // adds /name to the beginning of dir.
    dir = ""u8;
    for (@string parent = ".."u8;; ᐧ ; parent = "../"u8 + parent) {
        if (len(parent) >= 1024) {
            // Sanity check
            return ("", syscall.ENAMETOOLONG);
        }
        (fd, errΔ3) = openFileNolog(parent, O_RDONLY, 0);
        if (errΔ3 != default!) {
            return ("", errΔ3);
        }
        while (ᐧ) {
            (names, errΔ4) = fd.Readdirnames(100);
            if (errΔ4 != default!) {
                fd.Close();
                return ("", errΔ4);
            }
            foreach (var (_, name) in names) {
                (d, _) = lstatNolog(parent + "/"u8 + name);
                if (SameFile(d, dot)) {
                    dir = "/"u8 + name + dir;
                    goto Found;
                }
            }
        }
Found:
        (pd, errΔ3) = fd.Stat();
        fd.Close();
        if (errΔ3 != default!) {
            return ("", errΔ3);
        }
        if (SameFile(pd, root)) {
            break;
        }
        // Set up for next round.
        dot = pd;
    }
    // Save answer as hint to avoid the expensive path next time.
    getwdCache.Lock();
    getwdCache.dir = dir;
    getwdCache.Unlock();
    return (dir, default!);
}

} // end os_package
