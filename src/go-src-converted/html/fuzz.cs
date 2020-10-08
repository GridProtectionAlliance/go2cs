// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gofuzz

// package html -- go2cs converted at 2020 October 08 03:42:17 UTC
// import "html" ==> using html = go.html_package
// Original source: C:\Go\src\html\fuzz.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class html_package
    {
        public static long Fuzz(slice<byte> data) => func((_, panic, __) =>
        {
            var v = string(data);

            var e = EscapeString(v);
            var u = UnescapeString(e);
            if (v != u)
            {
                fmt.Printf("v = %q\n", v);
                fmt.Printf("e = %q\n", e);
                fmt.Printf("u = %q\n", u);
                panic("not equal");
            }
            EscapeString(UnescapeString(v));

            return 0L;

        });
    }
}
