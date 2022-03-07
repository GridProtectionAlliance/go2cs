// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc && !purego
// +build gc,!purego

// package chacha20poly1305 -- go2cs converted at 2022 March 06 23:36:33 UTC
// import "vendor/golang.org/x/crypto/chacha20poly1305" ==> using chacha20poly1305 = go.vendor.golang.org.x.crypto.chacha20poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20poly1305\chacha20poly1305_amd64.go
using binary = go.encoding.binary_package;

using subtle = go.golang.org.x.crypto.@internal.subtle_package;
using cpu = go.golang.org.x.sys.cpu_package;

namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20poly1305_package {

    //go:noescape
private static bool chacha20Poly1305Open(slice<byte> dst, slice<uint> key, slice<byte> src, slice<byte> ad);

//go:noescape
private static void chacha20Poly1305Seal(slice<byte> dst, slice<uint> key, slice<byte> src, slice<byte> ad);

private static var useAVX2 = cpu.X86.HasAVX2 && cpu.X86.HasBMI2;

// setupState writes a ChaCha20 input matrix to state. See
// https://tools.ietf.org/html/rfc7539#section-2.3.
private static void setupState(ptr<array<uint>> _addr_state, ptr<array<byte>> _addr_key, slice<byte> nonce) {
    ref array<uint> state = ref _addr_state.val;
    ref array<byte> key = ref _addr_key.val;

    state[0] = 0x61707865;
    state[1] = 0x3320646e;
    state[2] = 0x79622d32;
    state[3] = 0x6b206574;

    state[4] = binary.LittleEndian.Uint32(key[(int)0..(int)4]);
    state[5] = binary.LittleEndian.Uint32(key[(int)4..(int)8]);
    state[6] = binary.LittleEndian.Uint32(key[(int)8..(int)12]);
    state[7] = binary.LittleEndian.Uint32(key[(int)12..(int)16]);
    state[8] = binary.LittleEndian.Uint32(key[(int)16..(int)20]);
    state[9] = binary.LittleEndian.Uint32(key[(int)20..(int)24]);
    state[10] = binary.LittleEndian.Uint32(key[(int)24..(int)28]);
    state[11] = binary.LittleEndian.Uint32(key[(int)28..(int)32]);

    state[12] = 0;
    state[13] = binary.LittleEndian.Uint32(nonce[(int)0..(int)4]);
    state[14] = binary.LittleEndian.Uint32(nonce[(int)4..(int)8]);
    state[15] = binary.LittleEndian.Uint32(nonce[(int)8..(int)12]);
}

private static slice<byte> seal(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) => func((_, panic, _) => {
    ref chacha20poly1305 c = ref _addr_c.val;

    if (!cpu.X86.HasSSSE3) {>>MARKER:FUNCTION_chacha20Poly1305Seal_BLOCK_PREFIX<<
        return c.sealGeneric(dst, nonce, plaintext, additionalData);
    }
    ref array<uint> state = ref heap(new array<uint>(16), out ptr<array<uint>> _addr_state);
    setupState(_addr_state, _addr_c.key, nonce);

    var (ret, out) = sliceForAppend(dst, len(plaintext) + 16);
    if (subtle.InexactOverlap(out, plaintext)) {>>MARKER:FUNCTION_chacha20Poly1305Open_BLOCK_PREFIX<<
        panic("chacha20poly1305: invalid buffer overlap");
    }
    chacha20Poly1305Seal(out[..], state[..], plaintext, additionalData);
    return ret;

});

private static (slice<byte>, error) open(this ptr<chacha20poly1305> _addr_c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref chacha20poly1305 c = ref _addr_c.val;

    if (!cpu.X86.HasSSSE3) {
        return c.openGeneric(dst, nonce, ciphertext, additionalData);
    }
    ref array<uint> state = ref heap(new array<uint>(16), out ptr<array<uint>> _addr_state);
    setupState(_addr_state, _addr_c.key, nonce);

    ciphertext = ciphertext[..(int)len(ciphertext) - 16];
    var (ret, out) = sliceForAppend(dst, len(ciphertext));
    if (subtle.InexactOverlap(out, ciphertext)) {
        panic("chacha20poly1305: invalid buffer overlap");
    }
    if (!chacha20Poly1305Open(out, state[..], ciphertext, additionalData)) {
        foreach (var (i) in out) {
            out[i] = 0;
        }        return (null, error.As(errOpen)!);
    }
    return (ret, error.As(null!)!);

});

} // end chacha20poly1305_package
