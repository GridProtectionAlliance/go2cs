// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class bytealg_package {

// The declarations below generate ABI wrappers for functions
// implemented in assembly in this package but declared in another
// package.
// The compiler generates calls to runtime.memequal and runtime.memequal_varlen.
// In addition, the runtime calls runtime.memequal explicitly.
// Those functions are implemented in this package.

//go:linkname abigen_runtime_memequal runtime.memequal
internal static partial bool abigen_runtime_memequal(@unsafe.Pointer a, @unsafe.Pointer b, uintptr size);

//go:linkname abigen_runtime_memequal_varlen runtime.memequal_varlen
internal static partial bool abigen_runtime_memequal_varlen(@unsafe.Pointer a, @unsafe.Pointer b);

} // end bytealg_package
