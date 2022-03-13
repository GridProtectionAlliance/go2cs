// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || (js && wasm) || netbsd || openbsd
// +build aix darwin js,wasm netbsd openbsd

// package net -- go2cs converted at 2022 March 13 05:30:04 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sendfile_stub.go
namespace go;

using io = io_package;

public static partial class net_package {

private static (long, error, bool) sendFile(ptr<netFD> _addr_c, io.Reader r) {
    long n = default;
    error err = default!;
    bool handled = default;
    ref netFD c = ref _addr_c.val;

    return (0, error.As(null!)!, false);
}

} // end net_package
