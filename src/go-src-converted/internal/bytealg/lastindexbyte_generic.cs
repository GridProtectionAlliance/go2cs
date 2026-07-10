// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class bytealg_package {

public static nint LastIndexByte(slice<byte> s, byte c) {
    for (nint i = len(s) - 1; i >= 0; i--) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;
}

public static nint LastIndexByteString(@string s, byte c) {
    for (nint i = len(s) - 1; i >= 0; i--) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;
}

} // end bytealg_package
