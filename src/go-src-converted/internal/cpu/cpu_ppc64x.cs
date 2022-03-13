// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ppc64 || ppc64le
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2022 March 13 05:40:41 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_ppc64x.go
namespace go.@internal;

public static partial class cpu_package {

public static readonly nint CacheLinePadSize = 128;



private static void doinit() {
    options = new slice<option>(new option[] { {Name:"darn",Feature:&PPC64.HasDARN}, {Name:"scv",Feature:&PPC64.HasSCV}, {Name:"power9",Feature:&PPC64.IsPOWER9} });

    osinit();
}

private static bool isSet(nuint hwc, nuint value) {
    return hwc & value != 0;
}

} // end cpu_package
