// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

partial class sha3_package {

[GoType("num:nint")] partial struct spongeDirection;

internal static readonly spongeDirection spongeAbsorbing = /* iota */ 0;
internal static readonly spongeDirection spongeSqueezing = 1;

internal static readonly UntypedInt maxRate = 168;

[GoType] partial struct state {
    // Generic sponge components.
    internal array<uint64> a = new(25); // main state of the hash
    internal nint rate;       // the number of bytes of state to use
    // dsbyte contains the "domain separation" bits and the first bit of
    // the padding. Sections 6.1 and 6.2 of [1] separate the outputs of the
    // SHA-3 and SHAKE functions by appending bitstrings to the message.
    // Using a little-endian bit-ordering convention, these are "01" for SHA-3
    // and "1111" for SHAKE, or 00000010b and 00001111b, respectively. Then the
    // padding rule from section 5.1 is applied to pad the message to a multiple
    // of the rate, which involves adding a "1" bit, zero or more "0" bits, and
    // a final "1" bit. We merge the first "1" bit from the padding into dsbyte,
    // giving 00000110b (0x06) and 00011111b (0x1f).
    // [1] http://csrc.nist.gov/publications/drafts/fips-202/fips_202_draft.pdf
    //     "Draft FIPS 202: SHA-3 Standard: Permutation-Based Hash and
    //      Extendable-Output Functions (May 2014)"
    internal byte dsbyte;
    internal nint i; // storage[i:n] is the buffer, i is only used while squeezing
    internal nint n;
    internal array<byte> storage = new(maxRate);
    // Specific to SHA-3 and SHAKE.
    internal nint outputLen;            // the default output size in bytes
    internal spongeDirection state; // whether the sponge is absorbing or squeezing
}

// BlockSize returns the rate of sponge underlying this hash function.
[GoRecv] internal static nint BlockSize(this ref state d) {
    return d.rate;
}

// Size returns the output size of the hash function in bytes.
[GoRecv] internal static nint Size(this ref state d) {
    return d.outputLen;
}

// Reset clears the internal state by zeroing the sponge state and
// the buffer indexes, and setting Sponge.state to absorbing.
[GoRecv] internal static void Reset(this ref state d) {
    // Zero the permutation's state.
    foreach (var (i, _) in d.a) {
        d.a[i] = 0;
    }
    d.state = spongeAbsorbing;
    (d.i, d.n) = (0, 0);
}

[GoRecv] internal static ж<state> clone(this ref state d) {
    ref var ret = ref heap<state>(out var Ꮡret);
    ret = d;
    return Ꮡret;
}

// permute applies the KeccakF-1600 permutation. It handles
// any input-output buffering.
[GoRecv] internal static void permute(this ref state d) {
    var exprᴛ1 = d.state;
    if (exprᴛ1 == spongeAbsorbing) {
        xorIn(d, // If we're absorbing, we need to xor the input into the state
 // before applying the permutation.
 d.storage[..(int)(d.rate)]);
        d.n = 0;
        keccakF1600(Ꮡ(d.a));
    }
    else if (exprᴛ1 == spongeSqueezing) {
        keccakF1600(Ꮡ(d.a));
        d.i = 0;
        copyOut(d, // If we're squeezing, we need to apply the permutation before
 // copying more output.
 d.storage[..(int)(d.rate)]);
    }

}

// pads appends the domain separation bits in dsbyte, applies
// the multi-bitrate 10..1 padding rule, and permutes the state.
[GoRecv] internal static void padAndPermute(this ref state d) {
    // Pad with this instance's domain-separator bits. We know that there's
    // at least one byte of space in d.buf because, if it were full,
    // permute would have been called to empty it. dsbyte also contains the
    // first one bit for the padding. See the comment in the state struct.
    d.storage[d.n] = d.dsbyte;
    d.n++;
    while (d.n < d.rate) {
        d.storage[d.n] = 0;
        d.n++;
    }
    // This adds the final one bit for the padding. Because of the way that
    // bits are numbered from the LSB upwards, the final bit is the MSB of
    // the last byte.
    d.storage[d.rate - 1] ^= (byte)(128);
    // Apply the permutation
    d.permute();
    d.state = spongeSqueezing;
    d.n = d.rate;
    copyOut(d, d.storage[..(int)(d.rate)]);
}

// Write absorbs more data into the hash's state. It panics if any
// output has already been read.
[GoRecv] internal static (nint written, error err) Write(this ref state d, slice<byte> p) {
    nint written = default!;
    error err = default!;

    if (d.state != spongeAbsorbing) {
        throw panic("sha3: Write after Read");
    }
    written = len(p);
    while (len(p) > 0) {
        if (d.n == 0 && len(p) >= d.rate){
            // The fast path; absorb a full "rate" bytes of input and apply the permutation.
            xorIn(d, p[..(int)(d.rate)]);
            p = p[(int)(d.rate)..];
            keccakF1600(Ꮡ(d.a));
        } else {
            // The slow path; buffer the input until we can fill the sponge, and then xor it in.
            nint todo = d.rate - d.n;
            if (todo > len(p)) {
                todo = len(p);
            }
            d.n += copy(d.storage[(int)(d.n)..], p[..(int)(todo)]);
            p = p[(int)(todo)..];
            // If the sponge is full, apply the permutation.
            if (d.n == d.rate) {
                d.permute();
            }
        }
    }
    return (written, err);
}

// Read squeezes an arbitrary number of bytes from the sponge.
[GoRecv] internal static (nint n, error err) Read(this ref state d, slice<byte> @out) {
    nint n = default!;
    error err = default!;

    // If we're still absorbing, pad and apply the permutation.
    if (d.state == spongeAbsorbing) {
        d.padAndPermute();
    }
    n = len(@out);
    // Now, do the squeezing.
    while (len(@out) > 0) {
        nint nΔ1 = copy(@out, d.storage[(int)(d.i)..(int)(d.n)]);
        d.i += nΔ1;
        @out = @out[(int)(nΔ1)..];
        // Apply the permutation if we've squeezed the sponge dry.
        if (d.i == d.rate) {
            d.permute();
        }
    }
    return (n, err);
}

// Sum applies padding to the hash state and then squeezes out the desired
// number of output bytes. It panics if any output has already been read.
[GoRecv] internal static slice<byte> Sum(this ref state d, slice<byte> @in) {
    if (d.state != spongeAbsorbing) {
        throw panic("sha3: Sum after Read");
    }
    // Make a copy of the original hash so that caller can keep writing
    // and summing.
    var dup = d.clone();
    var hash = new slice<byte>((~dup).outputLen, 64);
    // explicit cap to allow stack allocation
    dup.Read(hash);
    return append(@in, hash.ꓸꓸꓸ);
}

} // end sha3_package
