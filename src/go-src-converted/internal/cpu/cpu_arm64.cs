// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_arm64.go


namespace go.@internal;

public static partial class cpu_package {

public static readonly nint CacheLinePadSize = 64;



private static void doinit() {
    options = new slice<option>(new option[] { {Name:"aes",Feature:&ARM64.HasAES}, {Name:"pmull",Feature:&ARM64.HasPMULL}, {Name:"sha1",Feature:&ARM64.HasSHA1}, {Name:"sha2",Feature:&ARM64.HasSHA2}, {Name:"crc32",Feature:&ARM64.HasCRC32}, {Name:"atomics",Feature:&ARM64.HasATOMICS}, {Name:"cpuid",Feature:&ARM64.HasCPUID}, {Name:"isNeoverseN1",Feature:&ARM64.IsNeoverseN1}, {Name:"isZeus",Feature:&ARM64.IsZeus} }); 

    // arm64 uses different ways to detect CPU features at runtime depending on the operating system.
    osInit();

}

private static ulong getisar0();

private static ulong getMIDR();

} // end cpu_package
