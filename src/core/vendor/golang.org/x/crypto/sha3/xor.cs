// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using subtle = crypto.subtle_package;
using binary = encoding.binary_package;
using @unsafe = unsafe_package;
using cpu = golang.org.x.sys.cpu_package;
using crypto;
using encoding;
using golang.org.x.sys;

partial class sha3_package {

// xorIn xors the bytes in buf into the state.
internal static void xorIn(ж<state> Ꮡd, slice<byte> buf) {
    ref var d = ref Ꮡd.val;

    if (cpu.IsBigEndian){
        for (nint i = 0; len(buf) >= 8; i++) {
            var a = binary.LittleEndian.Uint64(buf);
            d.a[i] ^= (uint64)(a);
            buf = buf[8..];
        }
    } else {
        var ab = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ(d.a)));
        subtle.XORBytes(ab[..], ab[..], buf);
    }
}

// copyOut copies uint64s to a byte buffer.
internal static void copyOut(ж<state> Ꮡd, slice<byte> b) {
    ref var d = ref Ꮡd.val;

    if (cpu.IsBigEndian){
        for (nint i = 0; len(b) >= 8; i++) {
            binary.LittleEndian.PutUint64(b, d.a[i]);
            b = b[8..];
        }
    } else {
        var ab = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ(d.a)));
        copy(b, ab[..]);
    }
}

} // end sha3_package
