// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 amd64p32

// package math -- go2cs converted at 2020 August 29 08:44:48 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\exp_asm.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        private static var useFMA = cpu.X86.HasAVX && cpu.X86.HasFMA;
    }
}
