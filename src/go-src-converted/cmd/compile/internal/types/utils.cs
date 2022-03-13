// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:17 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\utils.go
namespace go.cmd.compile.@internal;

public static partial class types_package {

public static readonly nint BADWIDTH = -1000000000;



private partial struct bitset8 { // : byte
}

private static void set(this ptr<bitset8> _addr_f, byte mask, bool b) {
    ref bitset8 f = ref _addr_f.val;

    if (b) {
        (uint8.val).val;

        (f) |= mask;
    }
    else
 {
        (uint8.val).val;

        (f) &= mask;
    }
}

} // end types_package
