// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !openbsd
namespace go;

partial class runtime_package {

// osStackAlloc performs OS-specific initialization before s is used
// as stack memory.
internal static void osStackAlloc(ж<mspan> Ꮡs) {
}

// osStackFree undoes the effect of osStackAlloc before s is returned
// to the heap.
internal static void osStackFree(ж<mspan> Ꮡs) {
}

} // end runtime_package
