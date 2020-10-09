// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2020 October 09 04:56:14 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Go\src\mime\type_openbsd.go

using static go.builtin;

namespace go
{
    public static partial class mime_package
    {
        private static void init()
        {
            typeFiles = append(typeFiles, "/usr/share/misc/mime.types");
        }
    }
}
