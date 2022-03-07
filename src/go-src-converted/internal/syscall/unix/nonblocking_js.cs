// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package unix -- go2cs converted at 2022 March 06 22:12:55 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Program Files\Go\src\internal\syscall\unix\nonblocking_js.go


namespace go.@internal.syscall;

public static partial class unix_package {

public static (bool, error) IsNonblock(nint fd) {
    bool nonblocking = default;
    error err = default!;

    return (false, error.As(null!)!);
}

} // end unix_package
