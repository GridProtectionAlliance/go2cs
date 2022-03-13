// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build mips64 || mips64le
// +build mips64 mips64le

// package cpu -- go2cs converted at 2022 March 13 06:46:34 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_mips64x.go
namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint cacheLineSize = 32;



private static void initOptions() {
    options = new slice<option>(new option[] { {Name:"msa",Feature:&MIPS64X.HasMSA} });
}

} // end cpu_package
