// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2022 March 06 22:21:15 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\grammar.go
using strings = go.strings_package;

namespace go;

public static partial class mime_package {

    // isTSpecial reports whether rune is in 'tspecials' as defined by RFC
    // 1521 and RFC 2045.
private static bool isTSpecial(int r) {
    return strings.ContainsRune("()<>@,;:\\\"/[]?=", r);
}

// isTokenChar reports whether rune is in 'token' as defined by RFC
// 1521 and RFC 2045.
private static bool isTokenChar(int r) { 
    // token := 1*<any (US-ASCII) CHAR except SPACE, CTLs,
    //             or tspecials>
    return r > 0x20 && r < 0x7f && !isTSpecial(r);

}

// isToken reports whether s is a 'token' as defined by RFC 1521
// and RFC 2045.
private static bool isToken(@string s) {
    if (s == "") {
        return false;
    }
    return strings.IndexFunc(s, isNotTokenChar) < 0;

}

} // end mime_package
