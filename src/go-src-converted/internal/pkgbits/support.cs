// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class pkgbits_package {

internal static void assert(bool b) {
    if (!b) {
        throw panic("assertion failed");
    }
}

internal static void errorf(@string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    throw panic(fmt.Errorf(format, args.ꓸꓸꓸ));
}

} // end pkgbits_package
