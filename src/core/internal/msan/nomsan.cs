// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !msan
namespace go.@internal;

using @unsafe = unsafe_package;

partial class msan_package {

public const bool Enabled = false;

public static void Read(@unsafe.Pointer addr, uintptr sz) {
}

public static void Write(@unsafe.Pointer addr, uintptr sz) {
}

public static void Malloc(@unsafe.Pointer addr, uintptr sz) {
}

public static void Free(@unsafe.Pointer addr, uintptr sz) {
}

public static void Move(@unsafe.Pointer dst, @unsafe.Pointer src, uintptr sz) {
}

} // end msan_package
