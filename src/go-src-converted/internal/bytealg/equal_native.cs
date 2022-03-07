// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\equal_native.go
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class bytealg_package {

    // The declarations below generate ABI wrappers for functions
    // implemented in assembly in this package but declared in another
    // package.

    // The compiler generates calls to runtime.memequal and runtime.memequal_varlen.
    // In addition, the runtime calls runtime.memequal explicitly.
    // Those functions are implemented in this package.

    //go:linkname abigen_runtime_memequal runtime.memequal
private static bool abigen_runtime_memequal(unsafe.Pointer a, unsafe.Pointer b, System.UIntPtr size);

//go:linkname abigen_runtime_memequal_varlen runtime.memequal_varlen
private static bool abigen_runtime_memequal_varlen(unsafe.Pointer a, unsafe.Pointer b);

} // end bytealg_package
