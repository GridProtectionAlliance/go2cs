// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

partial class uleb128_package {

public static slice<byte> AppendUleb128(slice<byte> b, nuint v) {
    while (ᐧ) {
        var c = ((uint8)((nuint)(v & 127)));
        v >>= (UntypedInt)(7);
        if (v != 0) {
            c |= (uint8)(128);
        }
        b = append(b, c);
        if ((uint8)(c & 128) == 0) {
            break;
        }
    }
    return b;
}

} // end uleb128_package
