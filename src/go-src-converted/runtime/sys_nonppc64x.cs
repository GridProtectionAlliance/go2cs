// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !ppc64 && !ppc64le
namespace go;

partial class runtime_package {

internal static void prepGoExitFrame(uintptr sp) {
}

} // end runtime_package
