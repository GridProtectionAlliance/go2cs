// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !(darwin || freebsd || linux || netbsd || openbsd)
namespace go.@internal;

partial class sysinfo_package {

internal static @string osCPUInfoName() {
    return ""u8;
}

} // end sysinfo_package
