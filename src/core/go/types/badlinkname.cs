// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using _ = unsafe_package;

partial class types_package {

// This should properly be in infer.go, but that file is auto-generated.

// infer should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goplus/gox
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badlinkname_Checker_infer go/types.(*Checker).infer
internal static partial slice<ΔType> badlinkname_Checker_infer(ж<Checker> _, positioner _, slice<ж<TypeParam>> _, slice<ΔType> _, ж<Tuple> _, slice<ж<operand>> _, bool _, ж<error_> _);

} // end types_package
