// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 22:12:50 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\at_aix.go


namespace go.@internal.syscall;

public static partial class unix_package {

    //go:cgo_import_dynamic libc_fstatat fstatat "libc.a/shr_64.o"
    //go:cgo_import_dynamic libc_openat openat "libc.a/shr_64.o"
    //go:cgo_import_dynamic libc_unlinkat unlinkat "libc.a/shr_64.o"
public static readonly nuint AT_REMOVEDIR = 0x1;
public static readonly nuint AT_SYMLINK_NOFOLLOW = 0x1;


} // end unix_package
