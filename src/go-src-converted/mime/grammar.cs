// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;

partial class mime_package {

// isTSpecial reports whether rune is in 'tspecials' as defined by RFC
// 1521 and RFC 2045.
internal static bool isTSpecial(rune r) {
    return strings.ContainsRune(@"()<>@,;:\""/[]?="u8, r);
}

// isTokenChar reports whether rune is in 'token' as defined by RFC
// 1521 and RFC 2045.
internal static bool isTokenChar(rune r) {
    // token := 1*<any (US-ASCII) CHAR except SPACE, CTLs,
    //             or tspecials>
    return r > 32 && r < 127 && !isTSpecial(r);
}

// isToken reports whether s is a 'token' as defined by RFC 1521
// and RFC 2045.
internal static bool isToken(@string s) {
    if (s == ""u8) {
        return false;
    }
    return strings.IndexFunc(s, isNotTokenChar) < 0;
}

} // end mime_package
