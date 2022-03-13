// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !faketime
// +build !faketime

// package syscall -- go2cs converted at 2022 March 13 05:40:38 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\time_nofake.go
namespace go;

public static partial class syscall_package {

private static readonly var faketime = false;



private static nint faketimeWrite(nint fd, slice<byte> p) => func((_, panic, _) => { 
    // This should never be called since faketime is false.
    panic("not implemented");
});

} // end syscall_package
