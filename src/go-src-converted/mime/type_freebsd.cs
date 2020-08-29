// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2020 August 29 08:32:39 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Go\src\mime\type_freebsd.go

using static go.builtin;

namespace go
{
    public static partial class mime_package
    {
        private static void init()
        {
            typeFiles = append(typeFiles, "/usr/local/etc/mime.types");
        }
    }
}
