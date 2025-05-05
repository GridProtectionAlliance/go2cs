// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

partial class atomic_package {

internal static void panicUnaligned() {
    throw panic("unaligned 64-bit atomic operation");
}

} // end atomic_package
