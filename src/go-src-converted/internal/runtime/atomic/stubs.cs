// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !wasm
namespace go.@internal.runtime;

using @unsafe = unsafe_package;

partial class atomic_package {

//go:noescape
public static partial bool Cas(ж<uint32> ptr, uint32 old, uint32 @new);

// NO go:noescape annotation; see atomic_pointer.go.
public static partial bool Casp1(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new);

//go:noescape
public static partial bool Casint32(ж<int32> ptr, int32 old, int32 @new);

//go:noescape
public static partial bool Casint64(ж<int64> ptr, int64 old, int64 @new);

//go:noescape
public static partial bool Casuintptr(ж<uintptr> ptr, uintptr old, uintptr @new);

//go:noescape
public static partial void Storeint32(ж<int32> ptr, int32 @new);

//go:noescape
public static partial void Storeint64(ж<int64> ptr, int64 @new);

//go:noescape
public static partial void Storeuintptr(ж<uintptr> ptr, uintptr @new);

//go:noescape
public static partial uintptr Loaduintptr(ж<uintptr> ptr);

//go:noescape
public static partial nuint Loaduint(ж<nuint> ptr);

// TODO(matloob): Should these functions have the go:noescape annotation?

//go:noescape
public static partial int32 Loadint32(ж<int32> ptr);

//go:noescape
public static partial int64 Loadint64(ж<int64> ptr);

//go:noescape
public static partial int32 Xaddint32(ж<int32> ptr, int32 delta);

//go:noescape
public static partial int64 Xaddint64(ж<int64> ptr, int64 delta);

//go:noescape
public static partial int32 Xchgint32(ж<int32> ptr, int32 @new);

//go:noescape
public static partial int64 Xchgint64(ж<int64> ptr, int64 @new);

} // end atomic_package
