// Code generated by "go test -run=Generate -write=all"; DO NOT EDIT.
// Source: ../../cmd/compile/internal/types2/pointer.go
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

partial class types_package {

// A Pointer represents a pointer type.
[GoType] partial struct Pointer {
    internal ΔType @base; // element type
}

// NewPointer returns a new pointer type for the given element (base) type.
public static ж<Pointer> NewPointer(ΔType elem) {
    return Ꮡ(new Pointer(@base: elem));
}

// Elem returns the element type for the given pointer p.
[GoRecv] public static ΔType Elem(this ref Pointer p) {
    return p.@base;
}

[GoRecv("capture")] public static ΔType Underlying(this ref Pointer p) {
    return ~p;
}

[GoRecv] public static @string String(this ref Pointer p) {
    return TypeString(~p, default!);
}

} // end types_package
