// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ppc64 || ppc64le
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2022 March 06 23:38:20 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_ppc64x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint cacheLineSize = 128;



private static void initOptions() {
    options = new slice<option>(new option[] { {Name:"darn",Feature:&PPC64.HasDARN}, {Name:"scv",Feature:&PPC64.HasSCV} });
}

} // end cpu_package
