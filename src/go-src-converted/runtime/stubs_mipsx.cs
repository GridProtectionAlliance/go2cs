// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips mipsle

// package runtime -- go2cs converted at 2020 October 08 03:23:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_mipsx.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Called from assembly only; declared for go vet.
        private static void load_g()
;
        private static void save_g()
;
    }
}
