// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !dragonfly && !freebsd && (!js || !wasm) && !netbsd && !openbsd && !solaris
// +build !aix
// +build !darwin
// +build !dragonfly
// +build !freebsd
// +build !js !wasm
// +build !netbsd
// +build !openbsd
// +build !solaris

// package os -- go2cs converted at 2022 March 13 05:28:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sticky_notbsd.go
namespace go;

public static partial class os_package {

private static readonly var supportsCreateWithStickyBit = true;


} // end os_package
