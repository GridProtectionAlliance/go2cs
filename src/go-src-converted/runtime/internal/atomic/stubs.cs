// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !wasm
// +build !wasm

// package atomic -- go2cs converted at 2022 March 06 22:08:20 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Program Files\Go\src\runtime\internal\atomic\stubs.go
using @unsafe = go.@unsafe_package;

namespace go.runtime.@internal;

public static partial class atomic_package {

    //go:noescape
public static bool Cas(ptr<uint> ptr, uint old, uint @new);

// NO go:noescape annotation; see atomic_pointer.go.
public static bool Casp1(ptr<unsafe.Pointer> ptr, unsafe.Pointer old, unsafe.Pointer @new);

//go:noescape
public static bool Casint32(ptr<int> ptr, int old, int @new);

//go:noescape
public static bool Casint64(ptr<long> ptr, long old, long @new);

//go:noescape
public static bool Casuintptr(ptr<System.UIntPtr> ptr, System.UIntPtr old, System.UIntPtr @new);

//go:noescape
public static void Storeint32(ptr<int> ptr, int @new);

//go:noescape
public static void Storeint64(ptr<long> ptr, long @new);

//go:noescape
public static void Storeuintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new);

//go:noescape
public static System.UIntPtr Loaduintptr(ptr<System.UIntPtr> ptr);

//go:noescape
public static nuint Loaduint(ptr<nuint> ptr);

// TODO(matloob): Should these functions have the go:noescape annotation?

//go:noescape
public static int Loadint32(ptr<int> ptr);

//go:noescape
public static long Loadint64(ptr<long> ptr);

//go:noescape
public static int Xaddint32(ptr<int> ptr, int delta);

//go:noescape
public static long Xaddint64(ptr<long> ptr, long delta);

//go:noescape
public static int Xchgint32(ptr<int> ptr, int @new);

//go:noescape
public static long Xchgint64(ptr<long> ptr, long @new);

} // end atomic_package
