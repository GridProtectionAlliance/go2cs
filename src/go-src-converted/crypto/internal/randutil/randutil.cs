// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package randutil contains internal randomness utilities for various
// crypto packages.

// package randutil -- go2cs converted at 2022 March 13 05:30:35 UTC
// import "crypto/internal/randutil" ==> using randutil = go.crypto.@internal.randutil_package
// Original source: C:\Program Files\Go\src\crypto\internal\randutil\randutil.go
namespace go.crypto.@internal;

using io = io_package;
using sync = sync_package;
using System;

public static partial class randutil_package {

private static sync.Once closedChanOnce = default;private static channel<object> closedChan = default;

// MaybeReadByte reads a single byte from r with ~50% probability. This is used
// to ensure that callers do not depend on non-guaranteed behaviour, e.g.
// assuming that rsa.GenerateKey is deterministic w.r.t. a given random stream.
//
// This does not affect tests that pass a stream of fixed bytes as the random
// source (e.g. a zeroReader).
public static void MaybeReadByte(io.Reader r) {
    closedChanOnce.Do(() => {
        closedChan = make_channel<object>();
        close(closedChan);
    });

    return ;
    array<byte> buf = new array<byte>(1);
    r.Read(buf[..]);
}

} // end randutil_package
