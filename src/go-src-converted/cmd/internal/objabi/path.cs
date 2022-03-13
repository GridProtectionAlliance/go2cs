// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2022 March 13 05:43:22 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Program Files\Go\src\cmd\internal\objabi\path.go
namespace go.cmd.@internal;

using strings = strings_package;

public static partial class objabi_package {

// PathToPrefix converts raw string to the prefix that will be used in the
// symbol table. All control characters, space, '%' and '"', as well as
// non-7-bit clean bytes turn into %xx. The period needs escaping only in the
// last segment of the path, and it makes for happier users if we escape that as
// little as possible.
public static @string PathToPrefix(@string s) {
    var slash = strings.LastIndex(s, "/"); 
    // check for chars that need escaping
    nint n = 0;
    {
        nint r__prev1 = r;

        for (nint r = 0; r < len(s); r++) {
            {
                var c__prev1 = c;

                var c = s[r];

                if (c <= ' ' || (c == '.' && r > slash) || c == '%' || c == '"' || c >= 0x7F) {
                    n++;
                }
                c = c__prev1;

            }
        }

        r = r__prev1;
    } 

    // quick exit
    if (n == 0) {
        return s;
    }
    const @string hex = "0123456789abcdef";

    var p = make_slice<byte>(0, len(s) + 2 * n);
    {
        nint r__prev1 = r;

        for (r = 0; r < len(s); r++) {
            {
                var c__prev1 = c;

                c = s[r];

                if (c <= ' ' || (c == '.' && r > slash) || c == '%' || c == '"' || c >= 0x7F) {
                    p = append(p, '%', hex[c >> 4], hex[c & 0xF]);
                }
                else
 {
                    p = append(p, c);
                }
                c = c__prev1;

            }
        }

        r = r__prev1;
    }

    return string(p);
}

// IsRuntimePackagePath examines 'pkgpath' and returns TRUE if it
// belongs to the collection of "runtime-related" packages, including
// "runtime" itself, "reflect", "syscall", and the
// "runtime/internal/*" packages. The compiler and/or assembler in
// some cases need to be aware of when they are building such a
// package, for example to enable features such as ABI selectors in
// assembly sources.
//
// Keep in sync with cmd/dist/build.go:IsRuntimePackagePath.
public static bool IsRuntimePackagePath(@string pkgpath) {
    var rval = false;
    switch (pkgpath) {
        case "runtime": 
            rval = true;
            break;
        case "reflect": 
            rval = true;
            break;
        case "syscall": 
            rval = true;
            break;
        case "internal/bytealg": 
            rval = true;
            break;
        default: 
            rval = strings.HasPrefix(pkgpath, "runtime/internal");
            break;
    }
    return rval;
}

} // end objabi_package
