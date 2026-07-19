// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !compiler_bootstrap
namespace go.math;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class bits_package {

//go:linkname overflowError runtime.overflowError
internal static error overflowError { get => go.runtime_package.overflowError; set => go.runtime_package.overflowError = value; }

//go:linkname divideError runtime.divideError
internal static error divideError { get => go.runtime_package.divideError; set => go.runtime_package.divideError = value; }

} // end bits_package
