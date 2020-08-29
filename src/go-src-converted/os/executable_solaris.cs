// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:44 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_solaris.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {


        private static (@string, error) executable()
        {
            var (path, err) = syscall.Getexecname();
            if (err != null)
            {
                return (path, err);
            }
            if (len(path) > 0L && path[0L] != '/')
            {
                if (initCwdErr != null)
                {
                    return (path, initCwdErr);
                }
                if (len(path) > 2L && path[0L..2L] == "./")
                { 
                    // skip "./"
                    path = path[2L..];
                }
                return (initCwd + "/" + path, null);
            }
            return (path, null);
        }
    }
}
