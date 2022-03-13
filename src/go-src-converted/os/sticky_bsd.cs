// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm netbsd openbsd solaris

// package os -- go2cs converted at 2022 March 13 05:28:05 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sticky_bsd.go
namespace go;

public static partial class os_package {

// According to sticky(8), neither open(2) nor mkdir(2) will create
// a file with the sticky bit set.
private static readonly var supportsCreateWithStickyBit = false;


} // end os_package
