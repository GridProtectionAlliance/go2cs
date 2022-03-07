// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2022 March 06 23:10:58 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ppc64\opt.go


namespace go.cmd.compile.@internal;

public static partial class ppc64_package {

    // Many Power ISA arithmetic and logical instructions come in four
    // standard variants. These bits let us map between variants.
public static readonly nint V_CC = 1 << 0; // xCC (affect CR field 0 flags)
public static readonly nint V_V = 1 << 1; // xV (affect SO and OV flags)

} // end ppc64_package
