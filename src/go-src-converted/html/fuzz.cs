// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gofuzz
// +build gofuzz

// package html -- go2cs converted at 2022 March 13 05:38:49 UTC
// import "html" ==> using html = go.html_package
// Original source: C:\Program Files\Go\src\html\fuzz.go
namespace go;

using fmt = fmt_package;

public static partial class html_package {

public static nint Fuzz(slice<byte> data) => func((_, panic, _) => {
    var v = string(data);

    var e = EscapeString(v);
    var u = UnescapeString(e);
    if (v != u) {
        fmt.Printf("v = %q\n", v);
        fmt.Printf("e = %q\n", e);
        fmt.Printf("u = %q\n", u);
        panic("not equal");
    }
    EscapeString(UnescapeString(v));

    return 0;
});

} // end html_package
