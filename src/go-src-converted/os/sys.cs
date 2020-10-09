// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 09 05:07:28 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\sys.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Hostname returns the host name reported by the kernel.
        public static (@string, error) Hostname()
        {
            @string name = default;
            error err = default!;

            return hostname();
        }
    }
}
