// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:41 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\executable_darwin.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static @string executablePath = default; // set by ../runtime/os_darwin.go



        private static (@string, error) executable()
        {
            var ep = executablePath;
            if (ep[0L] != '/')
            {
                if (initCwdErr != null)
                {
                    return (ep, initCwdErr);
                }
                if (len(ep) > 2L && ep[0L..2L] == "./")
                { 
                    // skip "./"
                    ep = ep[2L..];
                }
                ep = initCwd + "/" + ep;
            }
            return (ep, null);
        }
    }
}
