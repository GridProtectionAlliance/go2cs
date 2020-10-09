// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package randutil contains internal randomness utilities for various
// crypto packages.
// package randutil -- go2cs converted at 2020 October 09 04:52:54 UTC
// import "crypto/internal/randutil" ==> using randutil = go.crypto.@internal.randutil_package
// Original source: C:\Go\src\crypto\internal\randutil\randutil.go
using io = go.io_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace crypto {
namespace @internal
{
    public static partial class randutil_package
    {
        private static sync.Once closedChanOnce = default;        private static channel<object> closedChan = default;

        // MaybeReadByte reads a single byte from r with ~50% probability. This is used
        // to ensure that callers do not depend on non-guaranteed behaviour, e.g.
        // assuming that rsa.GenerateKey is deterministic w.r.t. a given random stream.
        //
        // This does not affect tests that pass a stream of fixed bytes as the random
        // source (e.g. a zeroReader).
        public static void MaybeReadByte(io.Reader r)
        {
            closedChanOnce.Do(() =>
            {
                closedChan = make_channel<object>();
                close(closedChan);
            });

            return ;
            array<byte> buf = new array<byte>(1L);
            r.Read(buf[..]);

        }
    }
}}}
