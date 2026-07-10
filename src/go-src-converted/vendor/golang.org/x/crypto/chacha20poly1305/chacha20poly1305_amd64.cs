// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build gc && !purego
namespace go.vendor.golang.org.x.crypto;

using binary = encoding.binary_package;
using alias = go.vendor.golang.org.x.crypto.@internal.alias_package;
using cpu = go.vendor.golang.org.x.sys.cpu_package;
using encoding;
using go.vendor.golang.org.x.crypto.@internal;
using go.vendor.golang.org.x.sys;

partial class chacha20poly1305_package {

//go:noescape
internal static partial bool chacha20Poly1305Open(slice<byte> dst, slice<uint32> key, slice<byte> src, slice<byte> ad);

//go:noescape
internal static partial void chacha20Poly1305Seal(slice<byte> dst, slice<uint32> key, slice<byte> src, slice<byte> ad);

internal static bool useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI2;

// setupState writes a ChaCha20 input matrix to state. See
// https://tools.ietf.org/html/rfc7539#section-2.3.
internal static void setupState(ж<array<uint32>> Ꮡstate, ж<array<byte>> Ꮡkey, slice<byte> nonce) {
    ref var state = ref Ꮡstate.Value;
    ref var key = ref Ꮡkey.Value;

    state[0] = 0x61707865;
    state[1] = 0x3320646e;
    state[2] = 0x79622d32;
    state[3] = 0x6b206574;
    state[4] = binary.LittleEndian.Uint32(key[0..4]);
    state[5] = binary.LittleEndian.Uint32(key[4..8]);
    state[6] = binary.LittleEndian.Uint32(key[8..12]);
    state[7] = binary.LittleEndian.Uint32(key[12..16]);
    state[8] = binary.LittleEndian.Uint32(key[16..20]);
    state[9] = binary.LittleEndian.Uint32(key[20..24]);
    state[10] = binary.LittleEndian.Uint32(key[24..28]);
    state[11] = binary.LittleEndian.Uint32(key[28..32]);
    state[12] = 0;
    state[13] = binary.LittleEndian.Uint32(nonce[0..4]);
    state[14] = binary.LittleEndian.Uint32(nonce[4..8]);
    state[15] = binary.LittleEndian.Uint32(nonce[8..12]);
}

internal static slice<byte> seal(this ж<chacha20poly1305> Ꮡc, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    ref var c = ref Ꮡc.Value;

    if (!cpu.X86.HasSSSE3) {
        return c.sealGeneric(dst, nonce, plaintext, additionalData);
    }
    ref var state = ref heap(new array<uint32>(16), out var Ꮡstate);
    setupState(Ꮡstate, Ꮡc.of(chacha20poly1305.Ꮡkey), nonce);
    var (ret, @out) = sliceForAppend(dst, len(plaintext) + 16);
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("chacha20poly1305: invalid buffer overlap");
    }
    chacha20Poly1305Seal(@out[..], state[..], plaintext, additionalData);
    return ret;
}

internal static (slice<byte>, error) open(this ж<chacha20poly1305> Ꮡc, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    ref var c = ref Ꮡc.Value;

    if (!cpu.X86.HasSSSE3) {
        return c.openGeneric(dst, nonce, ciphertext, additionalData);
    }
    ref var state = ref heap(new array<uint32>(16), out var Ꮡstate);
    setupState(Ꮡstate, Ꮡc.of(chacha20poly1305.Ꮡkey), nonce);
    ciphertext = ciphertext[..(int)(len(ciphertext) - 16)];
    var (ret, @out) = sliceForAppend(dst, len(ciphertext));
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("chacha20poly1305: invalid buffer overlap");
    }
    if (!chacha20Poly1305Open(@out, state[..], ciphertext, additionalData)) {
        foreach (var (i, _) in @out) {
            @out[i] = 0;
        }
        return (default!, errOpen);
    }
    return (ret, default!);
}

} // end chacha20poly1305_package
