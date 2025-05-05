// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !compiler_bootstrap
namespace go;

using bytealg = @internal.bytealg_package;
using @internal;

partial class strconv_package {

// index returns the index of the first instance of c in s, or -1 if missing.
internal static nint index(@string s, byte c) {
    return bytealg.IndexByteString(s, c);
}

} // end strconv_package
