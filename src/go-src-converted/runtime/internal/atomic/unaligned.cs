// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2022 March 13 05:24:09 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Program Files\Go\src\runtime\internal\atomic\unaligned.go
namespace go.runtime.@internal;

public static partial class atomic_package {

private static void panicUnaligned() => func((_, panic, _) => {
    panic("unaligned 64-bit atomic operation");
});

} // end atomic_package
