// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !race
namespace go.@internal;

using @unsafe = unsafe_package;

partial class race_package {

public const bool Enabled = false;

public static void Acquire(@unsafe.Pointer addr) {
}

public static void Release(@unsafe.Pointer addr) {
}

public static void ReleaseMerge(@unsafe.Pointer addr) {
}

public static void Disable() {
}

public static void Enable() {
}

public static void Read(@unsafe.Pointer addr) {
}

public static void Write(@unsafe.Pointer addr) {
}

public static void ReadRange(@unsafe.Pointer addr, nint len) {
}

public static void WriteRange(@unsafe.Pointer addr, nint len) {
}

public static nint Errors() {
    return 0;
}

} // end race_package
