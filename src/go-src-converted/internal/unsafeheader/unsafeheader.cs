// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unsafeheader contains header declarations for the Go runtime's slice
// and string implementations.
//
// This package allows packages that cannot import "reflect" to use types that
// are tested to be equivalent to reflect.SliceHeader and reflect.StringHeader.
// package unsafeheader -- go2cs converted at 2022 March 06 22:12:32 UTC
// import "internal/unsafeheader" ==> using unsafeheader = go.@internal.unsafeheader_package
// Original source: C:\Program Files\Go\src\internal\unsafeheader\unsafeheader.go
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class unsafeheader_package {

    // Slice is the runtime representation of a slice.
    // It cannot be used safely or portably and its representation may
    // change in a later release.
    //
    // Unlike reflect.SliceHeader, its Data field is sufficient to guarantee the
    // data it references will not be garbage collected.
public partial struct Slice {
    public unsafe.Pointer Data;
    public nint Len;
    public nint Cap;
}

// String is the runtime representation of a string.
// It cannot be used safely or portably and its representation may
// change in a later release.
//
// Unlike reflect.StringHeader, its Data field is sufficient to guarantee the
// data it references will not be garbage collected.
public partial struct String {
    public unsafe.Pointer Data;
    public nint Len;
}

} // end unsafeheader_package
