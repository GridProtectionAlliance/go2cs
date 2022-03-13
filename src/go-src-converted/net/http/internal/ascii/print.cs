// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ascii -- go2cs converted at 2022 March 13 05:30:22 UTC
// import "net/http/internal/ascii" ==> using ascii = go.net.http.@internal.ascii_package
// Original source: C:\Program Files\Go\src\net\http\internal\ascii\print.go
namespace go.net.http.@internal;

using strings = strings_package;
using unicode = unicode_package;


// EqualFold is strings.EqualFold, ASCII only. It reports whether s and t
// are equal, ASCII-case-insensitively.

public static partial class ascii_package {

public static bool EqualFold(@string s, @string t) {
    if (len(s) != len(t)) {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        if (lower(s[i]) != lower(t[i])) {
            return false;
        }
    }
    return true;
}

// lower returns the ASCII lowercase version of b.
private static byte lower(byte b) {
    if ('A' <= b && b <= 'Z') {
        return b + ('a' - 'A');
    }
    return b;
}

// IsPrint returns whether s is ASCII and printable according to
// https://tools.ietf.org/html/rfc20#section-4.2.
public static bool IsPrint(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] < ' ' || s[i] > '~') {
            return false;
        }
    }
    return true;
}

// Is returns whether s is ASCII.
public static bool Is(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] > unicode.MaxASCII) {
            return false;
        }
    }
    return true;
}

// ToLower returns the lowercase version of s if s is ASCII and printable.
public static (@string, bool) ToLower(@string s) {
    @string lower = default;
    bool ok = default;

    if (!IsPrint(s)) {
        return ("", false);
    }
    return (strings.ToLower(s), true);
}

} // end ascii_package
