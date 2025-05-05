// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.boring;

using boring = crypto.@internal.boring_package;
using big = math.big_package;
using @unsafe = unsafe_package;
using crypto.@internal;
using math;

partial class bbig_package {

public static boring.BigInt Enc(ж<bigꓸInt> Ꮡb) {
    ref var b = ref Ꮡb.val;

    if (b == nil) {
        return default!;
    }
    var x = b.Bits();
    if (len(x) == 0) {
        return new boring.BigInt{nil};
    }
    return @unsafe.Slice(((ж<nuint>)(Ꮡ(x, 0))), len(x));
}

public static ж<bigꓸInt> Dec(boring.BigInt b) {
    if (b == default!) {
        return default!;
    }
    if (len(b) == 0) {
        return @new<bigꓸInt>();
    }
    var x = @unsafe.Slice((ж<big.Word>)(Ꮡ(b, 0)), len(b));
    return @new<bigꓸInt>().SetBits(x);
}

} // end bbig_package
