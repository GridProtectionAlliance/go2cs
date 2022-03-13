// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64
// +build !386,!amd64

// package cpu -- go2cs converted at 2022 March 13 05:40:41 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_no_name.go
namespace go.@internal;

public static partial class cpu_package {

// Name returns the CPU name given by the vendor
// if it can be read directly from memory or by CPU instructions.
// If the CPU name can not be determined an empty string is returned.
//
// Implementations that use the Operating System (e.g. sysctl or /sys/)
// to gather CPU information for display should be placed in internal/sysinfo.
public static @string Name() { 
    // "A CPU has no name".
    return "";
}

} // end cpu_package
