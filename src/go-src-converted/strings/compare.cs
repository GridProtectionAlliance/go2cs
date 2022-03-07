// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2022 March 06 22:30:21 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Program Files\Go\src\strings\compare.go


namespace go;

public static partial class strings_package {

    // Compare returns an integer comparing two strings lexicographically.
    // The result will be 0 if a==b, -1 if a < b, and +1 if a > b.
    //
    // Compare is included only for symmetry with package bytes.
    // It is usually clearer and always faster to use the built-in
    // string comparison operators ==, <, >, and so on.
public static nint Compare(@string a, @string b) { 
    // NOTE(rsc): This function does NOT call the runtime cmpstring function,
    // because we do not want to provide any performance justification for
    // using strings.Compare. Basically no one should use strings.Compare.
    // As the comment above says, it is here only for symmetry with package bytes.
    // If performance is important, the compiler should be changed to recognize
    // the pattern so that all code doing three-way comparisons, not just code
    // using strings.Compare, can benefit.
    if (a == b) {
        return 0;
    }
    if (a < b) {
        return -1;
    }
    return +1;

}

} // end strings_package
