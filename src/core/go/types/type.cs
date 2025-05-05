// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

partial class types_package {

// A Type represents a type of Go.
// All types implement the Type interface.
[GoType] partial interface ΔType {
    // Underlying returns the underlying type of a type.
    // Underlying types are never Named, TypeParam, or Alias types.
    //
    // See https://go.dev/ref/spec#Underlying_types.
    ΔType Underlying();
    // String returns a string representation of a type.
    @string String();
}

} // end types_package
