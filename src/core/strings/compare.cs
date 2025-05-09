// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytealg = @internal.bytealg_package;
using @internal;

partial class strings_package {

// Compare returns an integer comparing two strings lexicographically.
// The result will be 0 if a == b, -1 if a < b, and +1 if a > b.
//
// Use Compare when you need to perform a three-way comparison (with
// [slices.SortFunc], for example). It is usually clearer and always faster
// to use the built-in string comparison operators ==, <, >, and so on.
public static nint Compare(@string a, @string b) {
    return bytealg.CompareString(a, b);
}

} // end strings_package
