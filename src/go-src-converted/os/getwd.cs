// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:16 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\getwd.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class os_package
    {
        private static var getwdCache = default;

        // useSyscallwd determines whether to use the return value of
        // syscall.Getwd based on its error.
        private static Func<error, bool> useSyscallwd = _p0 => true;

        // Getwd returns a rooted path name corresponding to the
        // current directory. If the current directory can be
        // reached via multiple paths (due to symbolic links),
        // Getwd may return any one of them.
        public static (@string, error) Getwd()
        {
            @string dir = default;
            error err = default!;

            if (runtime.GOOS == "windows" || runtime.GOOS == "plan9")
            {
                return syscall.Getwd();
            } 

            // Clumsy but widespread kludge:
            // if $PWD is set and matches ".", use it.
            var (dot, err) = statNolog(".");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            dir = Getenv("PWD");
            if (len(dir) > 0L && dir[0L] == '/')
            {
                var (d, err) = statNolog(dir);
                if (err == null && SameFile(dot, d))
                {
                    return (dir, error.As(null!)!);
                }

            } 

            // If the operating system provides a Getwd call, use it.
            // Otherwise, we're trying to find our way back to ".".
            if (syscall.ImplementsGetwd)
            {
                var (s, e) = syscall.Getwd();
                if (useSyscallwd(e))
                {
                    return (s, error.As(NewSyscallError("getwd", e))!);
                }

            } 

            // Apply same kludge but to cached dir instead of $PWD.
            getwdCache.Lock();
            dir = getwdCache.dir;
            getwdCache.Unlock();
            if (len(dir) > 0L)
            {
                (d, err) = statNolog(dir);
                if (err == null && SameFile(dot, d))
                {
                    return (dir, error.As(null!)!);
                }

            } 

            // Root is a special case because it has no parent
            // and ends in a slash.
            var (root, err) = statNolog("/");
            if (err != null)
            { 
                // Can't stat root - no hope.
                return ("", error.As(err)!);

            }

            if (SameFile(root, dot))
            {
                return ("/", error.As(null!)!);
            } 

            // General algorithm: find name in parent
            // and then find name of parent. Each iteration
            // adds /name to the beginning of dir.
            dir = "";
            {
                @string parent = "..";

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    if (len(parent) >= 1024L)
                    { // Sanity check
                        return ("", error.As(syscall.ENAMETOOLONG)!);
                    parent = "../" + parent;
                    }

                    var (fd, err) = openFileNolog(parent, O_RDONLY, 0L);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    while (true)
                    {
                        var (names, err) = fd.Readdirnames(100L);
                        if (err != null)
                        {
                            fd.Close();
                            return ("", error.As(err)!);
                        }

                        foreach (var (_, name) in names)
                        {
                            var (d, _) = lstatNolog(parent + "/" + name);
                            if (SameFile(d, dot))
                            {
                                dir = "/" + name + dir;
                                goto Found;
                            }

                        }

                    }


Found:
                    var (pd, err) = fd.Stat();
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    fd.Close();
                    if (SameFile(pd, root))
                    {
                        break;
                    } 
                    // Set up for next round.
                    dot = pd;

                } 

                // Save answer as hint to avoid the expensive path next time.

            } 

            // Save answer as hint to avoid the expensive path next time.
            getwdCache.Lock();
            getwdCache.dir = dir;
            getwdCache.Unlock();

            return (dir, error.As(null!)!);

        }
    }
}
