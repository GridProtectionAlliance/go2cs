// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package randutil contains internal randomness utilities for various
// crypto packages.
namespace go.crypto.@internal;

using io = io_package;
using sync = sync_package;

partial class randutil_package {

internal static sync.Once closedChanOnce;
internal static channel<EmptyStruct> closedChan;

// MaybeReadByte reads a single byte from r with ~50% probability. This is used
// to ensure that callers do not depend on non-guaranteed behaviour, e.g.
// assuming that rsa.GenerateKey is deterministic w.r.t. a given random stream.
//
// This does not affect tests that pass a stream of fixed bytes as the random
// source (e.g. a zeroReader).
public static void MaybeReadByte(io.Reader r) {
    closedChanOnce.Do(
    var closedChanʗ2 = closedChan;
    () => {
        closedChanʗ2 = new channel<EmptyStruct>(1);
        close(closedChanʗ2);
    });
    switch (select(ᐸꟷ(closedChan, ꓸꓸꓸ), ᐸꟷ(closedChan, ꓸꓸꓸ))) {
    case 0 when closedChan.ꟷᐳ(out _): {
        return;
    }
    case 1 when closedChan.ꟷᐳ(out _): {
        array<byte> buf = new(1);
        r.Read(buf[..]);
        break;
    }}
}

} // end randutil_package
