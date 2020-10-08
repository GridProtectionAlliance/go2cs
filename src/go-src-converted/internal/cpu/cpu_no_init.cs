// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !386
// +build !amd64
// +build !arm
// +build !arm64
// +build !ppc64
// +build !ppc64le
// +build !s390x
// +build !mips64
// +build !mips64le

// package cpu -- go2cs converted at 2020 October 08 03:19:09 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu_no_init.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        private static void doinit()
        {
        }
    }
}}
