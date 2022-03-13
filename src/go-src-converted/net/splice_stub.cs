// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !linux
// +build !linux

// package net -- go2cs converted at 2022 March 13 05:30:07 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\splice_stub.go
namespace go;

using io = io_package;

public static partial class net_package {

private static (long, error, bool) splice(ptr<netFD> _addr_c, io.Reader r) {
    long _p0 = default;
    error _p0 = default!;
    bool _p0 = default;
    ref netFD c = ref _addr_c.val;

    return (0, error.As(null!)!, false);
}

} // end net_package
