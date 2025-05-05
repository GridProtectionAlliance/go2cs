// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !msan
// Dummy MSan support API, used when not built with -msan.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal const bool msanenabled = false;

// Because msanenabled is false, none of these functions should be called.
internal static void msanread(@unsafe.Pointer addr, uintptr sz) {
    @throw("msan"u8);
}

internal static void msanwrite(@unsafe.Pointer addr, uintptr sz) {
    @throw("msan"u8);
}

internal static void msanmalloc(@unsafe.Pointer addr, uintptr sz) {
    @throw("msan"u8);
}

internal static void msanfree(@unsafe.Pointer addr, uintptr sz) {
    @throw("msan"u8);
}

internal static void msanmove(@unsafe.Pointer dst, @unsafe.Pointer src, uintptr sz) {
    @throw("msan"u8);
}

} // end runtime_package
