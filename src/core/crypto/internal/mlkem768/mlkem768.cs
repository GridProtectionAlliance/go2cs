// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mlkem768 implements the quantum-resistant key encapsulation method
// ML-KEM (formerly known as Kyber).
//
// Only the recommended ML-KEM-768 parameter set is provided.
//
// The version currently implemented is the one specified by [NIST FIPS 203 ipd],
// with the unintentional transposition of the matrix A reverted to match the
// behavior of [Kyber version 3.0]. Future versions of this package might
// introduce backwards incompatible changes to implement changes to FIPS 203.
//
// [Kyber version 3.0]: https://pq-crystals.org/kyber/data/kyber-specification-round3-20210804.pdf
// [NIST FIPS 203 ipd]: https://doi.org/10.6028/NIST.FIPS.203.ipd
namespace go.crypto.@internal;

// This package targets security, correctness, simplicity, readability, and
// reviewability as its primary goals. All critical operations are performed in
// constant time.
//
// Variable and function names, as well as code layout, are selected to
// facilitate reviewing the implementation against the NIST FIPS 203 ipd
// document.
//
// Reviewers unfamiliar with polynomials or linear algebra might find the
// background at https://words.filippo.io/kyber-math/ useful.
using rand = crypto.rand_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using byteorder = @internal.byteorder_package;
using sha3 = golang.org.x.crypto.sha3_package;
using @internal;
using crypto;
using golang.org.x.crypto;

partial class mlkem768_package {

internal static readonly UntypedInt n = 256;
internal static readonly UntypedInt q = 3329;
internal static readonly UntypedInt log2q = 12;
internal static readonly UntypedInt k = 3;
internal static readonly UntypedInt η = 2;
internal static readonly UntypedInt du = 10;
internal static readonly UntypedInt dv = 4;
internal static readonly UntypedInt encodingSize12 = /* n * log2q / 8 */ 384;
internal static readonly UntypedInt encodingSize10 = /* n * du / 8 */ 320;
internal static readonly UntypedInt encodingSize4 = /* n * dv / 8 */ 128;
internal static readonly UntypedInt encodingSize1 = /* n * 1 / 8 */ 32;
internal static readonly UntypedInt messageSize = /* encodingSize1 */ 32;
internal static readonly UntypedInt decryptionKeySize = /* k * encodingSize12 */ 1152;
internal static readonly UntypedInt encryptionKeySize = /* k*encodingSize12 + 32 */ 1184;
public static readonly UntypedInt CiphertextSize = /* k*encodingSize10 + encodingSize4 */ 1088;
public static readonly UntypedInt EncapsulationKeySize = /* encryptionKeySize */ 1184;
public static readonly UntypedInt DecapsulationKeySize = /* decryptionKeySize + encryptionKeySize + 32 + 32 */ 2400;
public static readonly UntypedInt SharedKeySize = 32;
public static readonly UntypedInt SeedSize = /* 32 + 32 */ 64;

// A DecapsulationKey is the secret key used to decapsulate a shared key from a
// ciphertext. It includes various precomputed values.
[GoType] partial struct DecapsulationKey {
    internal array<byte> dk = new(DecapsulationKeySize);
    internal partial ref encryptionKey encryptionKey { get; }
    internal partial ref decryptionKey decryptionKey { get; }
}

// Bytes returns the extended encoding of the decapsulation key, according to
// FIPS 203 (DRAFT).
[GoRecv] public static slice<byte> Bytes(this ref DecapsulationKey dk) {
    array<byte> b = new(2400); /* DecapsulationKeySize */
    copy(b[..], dk.dk[..]);
    return b[..];
}

// EncapsulationKey returns the public encapsulation key necessary to produce
// ciphertexts.
[GoRecv] public static slice<byte> EncapsulationKey(this ref DecapsulationKey dk) {
    array<byte> b = new(1184); /* EncapsulationKeySize */
    copy(b[..], dk.dk[(int)(decryptionKeySize)..]);
    return b[..];
}

// encryptionKey is the parsed and expanded form of a PKE encryption key.
[GoType] partial struct encryptionKey {
    internal array<nttElement> t = new(k); // ByteDecode₁₂(ek[:384k])
    public array<nttElement> A = new(k * k); // A[i*k+j] = sampleNTT(ρ, j, i)
}

// decryptionKey is the parsed and expanded form of a PKE decryption key.
[GoType] partial struct decryptionKey {
    internal array<nttElement> s = new(k); // ByteDecode₁₂(dk[:decryptionKeySize])
}

// GenerateKey generates a new decapsulation key, drawing random bytes from
// crypto/rand. The decapsulation key must be kept secret.
public static (ж<DecapsulationKey>, error) GenerateKey() {
    // The actual logic is in a separate function to outline this allocation.
    var dk = Ꮡ(new DecapsulationKey(nil));
    return generateKey(dk);
}

internal static (ж<DecapsulationKey>, error) generateKey(ж<DecapsulationKey> Ꮡdk) {
    ref var dk = ref Ꮡdk.val;

    ref var d = ref heap(new array<byte>(32), out var Ꮡd);
    {
        var (_, err) = rand.Read(d[..]); if (err != default!) {
            return (default!, errors.New("mlkem768: crypto/rand Read failed: "u8 + err.Error()));
        }
    }
    ref var z = ref heap(new array<byte>(32), out var Ꮡz);
    {
        var (_, err) = rand.Read(z[..]); if (err != default!) {
            return (default!, errors.New("mlkem768: crypto/rand Read failed: "u8 + err.Error()));
        }
    }
    return (kemKeyGen(Ꮡdk, Ꮡd, Ꮡz), default!);
}

// NewKeyFromSeed deterministically generates a decapsulation key from a 64-byte
// seed in the "d || z" form. The seed must be uniformly random.
public static (ж<DecapsulationKey>, error) NewKeyFromSeed(slice<byte> seed) {
    // The actual logic is in a separate function to outline this allocation.
    var dk = Ꮡ(new DecapsulationKey(nil));
    return newKeyFromSeed(dk, seed);
}

internal static (ж<DecapsulationKey>, error) newKeyFromSeed(ж<DecapsulationKey> Ꮡdk, slice<byte> seed) {
    ref var dk = ref Ꮡdk.val;

    if (len(seed) != SeedSize) {
        return (default!, errors.New("mlkem768: invalid seed length"u8));
    }
    var d = (ж<array<byte>>)(seed[..32]);
    var z = (ж<array<byte>>)(seed[32..]);
    return (kemKeyGen(Ꮡdk, d, z), default!);
}

// NewKeyFromExtendedEncoding parses a decapsulation key from its FIPS 203
// (DRAFT) extended encoding.
public static (ж<DecapsulationKey>, error) NewKeyFromExtendedEncoding(slice<byte> decapsulationKey) {
    // The actual logic is in a separate function to outline this allocation.
    var dk = Ꮡ(new DecapsulationKey(nil));
    return newKeyFromExtendedEncoding(dk, decapsulationKey);
}

internal static (ж<DecapsulationKey>, error) newKeyFromExtendedEncoding(ж<DecapsulationKey> Ꮡdk, slice<byte> dkBytes) {
    ref var dk = ref Ꮡdk.val;

    if (len(dkBytes) != DecapsulationKeySize) {
        return (default!, errors.New("mlkem768: invalid decapsulation key length"u8));
    }
    // Note that we don't check that H(ek) matches ekPKE, as that's not
    // specified in FIPS 203 (DRAFT). This is one reason to prefer the seed
    // private key format.
    dk.dk = array<byte>(dkBytes);
    var dkPKE = dkBytes[..(int)(decryptionKeySize)];
    {
        var err = parseDK(Ꮡ(dk.decryptionKey), dkPKE); if (err != default!) {
            return (default!, err);
        }
    }
    var ekPKE = dkBytes[(int)(decryptionKeySize)..(int)(decryptionKeySize + encryptionKeySize)];
    {
        var err = parseEK(Ꮡ(dk.encryptionKey), ekPKE); if (err != default!) {
            return (default!, err);
        }
    }
    return (Ꮡdk, default!);
}

// kemKeyGen generates a decapsulation key.
//
// It implements ML-KEM.KeyGen according to FIPS 203 (DRAFT), Algorithm 15, and
// K-PKE.KeyGen according to FIPS 203 (DRAFT), Algorithm 12. The two are merged
// to save copies and allocations.
internal static ж<DecapsulationKey> kemKeyGen(ж<DecapsulationKey> Ꮡdk, ж<array<byte>> Ꮡd, ж<array<byte>> Ꮡz) {
    ref var dk = ref Ꮡdk.val;
    ref var d = ref Ꮡd.val;
    ref var z = ref Ꮡz.val;

    if (dk == nil) {
        Ꮡdk = Ꮡ(new DecapsulationKey(nil)); dk = ref Ꮡdk.val;
    }
    var G = sha3.Sum512(d[..]);
    var ρ = G[..32];
    var σ = G[32..];
    var A = Ꮡ(dk.A);
    for (var i = ((byte)0); i < k; i++) {
        for (var j = ((byte)0); j < k; j++) {
            // Note that this is consistent with Kyber round 3, rather than with
            // the initial draft of FIPS 203, because NIST signaled that the
            // change was involuntary and will be reverted.
            A[i * k + j] = sampleNTT(ρ, j, i);
        }
    }
    byte N = default!;
    var s = Ꮡ(dk.s);
    /* for i := range s {
	s[i] = ntt(samplePolyCBD(σ, N))
	N++
} */
    var e = new slice<nttElement>(k);
    foreach (var (i, _) in e) {
        e[i] = ntt(samplePolyCBD(σ, N));
        N++;
    }
    var t = Ꮡ(dk.t);
    /* for i := range t {
	t[i] = e[i]
	for j := range s {
		t[i] = polyAdd(t[i], nttMul(A[i*k+j], s[j]))
	}
} */
    // t = A ◦ s + e
    // dkPKE ← ByteEncode₁₂(s)
    // ekPKE ← ByteEncode₁₂(t) || ρ
    // ek ← ekPKE
    // dk ← dkPKE || ek || H(ek) || z
    var dkB = dk.dk[..0];
    /* for i := range s {
	dkB = polyByteEncode(dkB, s[i])
} */
    /* for i := range t {
	dkB = polyByteEncode(dkB, t[i])
} */
    dkB = append(dkB, ρ.ꓸꓸꓸ);
    var H = sha3.New256();
    H.Write(dkB[(int)(decryptionKeySize)..]);
    dkB = H.Sum(dkB);
    dkB = append(dkB, z[..].ꓸꓸꓸ);
    if (len(dkB) != len(dk.dk)) {
        throw panic("mlkem768: internal error: invalid decapsulation key size");
    }
    return Ꮡdk;
}

// Encapsulate generates a shared key and an associated ciphertext from an
// encapsulation key, drawing random bytes from crypto/rand.
// If the encapsulation key is not valid, Encapsulate returns an error.
//
// The shared key must be kept secret.
public static (slice<byte> ciphertext, slice<byte> sharedKey, error err) Encapsulate(slice<byte> encapsulationKey) {
    slice<byte> ciphertext = default!;
    slice<byte> sharedKey = default!;
    error err = default!;

    // The actual logic is in a separate function to outline this allocation.
    ref var cc = ref heap(new array<byte>(1088), out var Ꮡcc);
    return encapsulate(Ꮡcc, encapsulationKey);
}

internal static (slice<byte> ciphertext, slice<byte> sharedKey, error err) encapsulate(ж<array<byte>> Ꮡcc, slice<byte> encapsulationKey) {
    slice<byte> ciphertext = default!;
    slice<byte> sharedKey = default!;
    error err = default!;

    ref var cc = ref Ꮡcc.val;
    if (len(encapsulationKey) != EncapsulationKeySize) {
        return (default!, default!, errors.New("mlkem768: invalid encapsulation key length"u8));
    }
    ref var m = ref heap(new array<byte>(32), out var Ꮡm);
    {
        var (_, errΔ1) = rand.Read(m[..]); if (errΔ1 != default!) {
            return (default!, default!, errors.New("mlkem768: crypto/rand Read failed: "u8 + errΔ1.Error()));
        }
    }
    return kemEncaps(Ꮡcc, encapsulationKey, Ꮡm);
}

// kemEncaps generates a shared key and an associated ciphertext.
//
// It implements ML-KEM.Encaps according to FIPS 203 (DRAFT), Algorithm 16.
internal static (slice<byte> c, slice<byte> K, error err) kemEncaps(ж<array<byte>> Ꮡcc, slice<byte> ek, ж<array<byte>> Ꮡm) {
    slice<byte> c = default!;
    slice<byte> K = default!;
    error err = default!;

    ref var cc = ref Ꮡcc.val;
    ref var m = ref Ꮡm.val;
    if (cc == nil) {
        Ꮡcc = Ꮡ(new byte[]{}.array()); cc = ref Ꮡcc.val;
    }
    var H = sha3.Sum256(ek[..]);
    var g = sha3.New512();
    g.Write(m[..]);
    g.Write(H[..]);
    var G = g.Sum(default!);
    K = G[..(int)(SharedKeySize)];
    var r = G[(int)(SharedKeySize)..];
    ref var ex = ref heap(new encryptionKey(), out var Ꮡex);
    {
        var errΔ1 = parseEK(Ꮡex, ek[..]); if (errΔ1 != default!) {
            return (default!, default!, errΔ1);
        }
    }
    c = pkeEncrypt(Ꮡcc, Ꮡex, Ꮡm, r);
    return (c, K, default!);
}

// parseEK parses an encryption key from its encoded form.
//
// It implements the initial stages of K-PKE.Encrypt according to FIPS 203
// (DRAFT), Algorithm 13.
internal static error parseEK(ж<encryptionKey> Ꮡex, slice<byte> ekPKE) {
    ref var ex = ref Ꮡex.val;

    if (len(ekPKE) != encryptionKeySize) {
        return errors.New("mlkem768: invalid encryption key length"u8);
    }
    foreach (var (iΔ1, _) in ex.t) {
        error err = default!;
        (ex.t[iΔ1], err) = polyByteDecode<nttElement>(ekPKE[..(int)(encodingSize12)]);
        if (err != default!) {
            return err;
        }
        ekPKE = ekPKE[(int)(encodingSize12)..];
    }
    var ρ = ekPKE;
    for (var i = ((byte)0); i < k; i++) {
        for (var j = ((byte)0); j < k; j++) {
            // See the note in pkeKeyGen about the order of the indices being
            // consistent with Kyber round 3.
            ex.A[i * k + j] = sampleNTT(ρ, j, i);
        }
    }
    return default!;
}

// pkeEncrypt encrypt a plaintext message.
//
// It implements K-PKE.Encrypt according to FIPS 203 (DRAFT), Algorithm 13,
// although the computation of t and AT is done in parseEK.
internal static slice<byte> pkeEncrypt(ж<array<byte>> Ꮡcc, ж<encryptionKey> Ꮡex, ж<array<byte>> Ꮡm, slice<byte> rnd) {
    ref var cc = ref Ꮡcc.val;
    ref var ex = ref Ꮡex.val;
    ref var m = ref Ꮡm.val;

    byte N = default!;
    var r = new slice<nttElement>(k);
    var e1 = new slice<ringElement>(k);
    foreach (var (i, _) in r) {
        r[i] = ntt(samplePolyCBD(rnd, N));
        N++;
    }
    foreach (var (i, _) in e1) {
        e1[i] = samplePolyCBD(rnd, N);
        N++;
    }
    var e2 = samplePolyCBD(rnd, N);
    var u = new slice<ringElement>(k);
    // NTT⁻¹(AT ◦ r) + e1
    foreach (var (i, _) in u) {
        u[i] = e1[i];
        foreach (var (j, _) in r) {
            // Note that i and j are inverted, as we need the transposed of A.
            u[i] = polyAdd(u[i], inverseNTT(nttMul(ex.A[j * k + i], r[j])));
        }
    }
    var μ = ringDecodeAndDecompress1(Ꮡm);
    nttElement vNTT = default!;                  // t⊺ ◦ r
    foreach (var (i, _) in ex.t) {
        vNTT = polyAdd(vNTT, nttMul(ex.t[i], r[i]));
    }
    var v = polyAdd(polyAdd(inverseNTT(vNTT), e2), μ);
    var c = cc[..0];
    foreach (var (_, f) in u) {
        c = ringCompressAndEncode10(c, f);
    }
    c = ringCompressAndEncode4(c, v);
    return c;
}

// Decapsulate generates a shared key from a ciphertext and a decapsulation key.
// If the ciphertext is not valid, Decapsulate returns an error.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, error err) Decapsulate(ж<DecapsulationKey> Ꮡdk, slice<byte> ciphertext) {
    slice<byte> sharedKey = default!;
    error err = default!;

    ref var dk = ref Ꮡdk.val;
    if (len(ciphertext) != CiphertextSize) {
        return (default!, errors.New("mlkem768: invalid ciphertext length"u8));
    }
    var c = (ж<array<byte>>)(ciphertext);
    return (kemDecaps(Ꮡdk, c), default!);
}

// kemDecaps produces a shared key from a ciphertext.
//
// It implements ML-KEM.Decaps according to FIPS 203 (DRAFT), Algorithm 17.
internal static slice<byte> /*K*/ kemDecaps(ж<DecapsulationKey> Ꮡdk, ж<array<byte>> Ꮡc) {
    slice<byte> K = default!;

    ref var dk = ref Ꮡdk.val;
    ref var c = ref Ꮡc.val;
    var h = dk.dk[(int)(decryptionKeySize + encryptionKeySize)..(int)(decryptionKeySize + encryptionKeySize + 32)];
    var z = dk.dk[(int)(decryptionKeySize + encryptionKeySize + 32)..];
    var m = pkeDecrypt(Ꮡ(dk.decryptionKey), Ꮡc);
    var g = sha3.New512();
    g.Write(m[..]);
    g.Write(h);
    var G = g.Sum(default!);
    var Kprime = G[..(int)(SharedKeySize)];
    var r = G[(int)(SharedKeySize)..];
    var J = sha3.NewShake256();
    J.Write(z);
    J.Write(c[..]);
    var Kout = new slice<byte>(SharedKeySize);
    J.Read(Kout);
    ref var cc = ref heap(new array<byte>(1088), out var Ꮡcc);
    var c1 = pkeEncrypt(Ꮡcc, Ꮡ(dk.encryptionKey), (ж<array<byte>>)(m), r);
    subtle.ConstantTimeCopy(subtle.ConstantTimeCompare(c[..], c1), Kout, Kprime);
    return Kout;
}

// parseDK parses a decryption key from its encoded form.
//
// It implements the computation of s from K-PKE.Decrypt according to FIPS 203
// (DRAFT), Algorithm 14.
internal static error parseDK(ж<decryptionKey> Ꮡdx, slice<byte> dkPKE) {
    ref var dx = ref Ꮡdx.val;

    if (len(dkPKE) != decryptionKeySize) {
        return errors.New("mlkem768: invalid decryption key length"u8);
    }
    foreach (var (i, _) in dx.s) {
        var (f, err) = polyByteDecode<nttElement>(dkPKE[..(int)(encodingSize12)]);
        if (err != default!) {
            return err;
        }
        dx.s[i] = f;
        dkPKE = dkPKE[(int)(encodingSize12)..];
    }
    return default!;
}

// pkeDecrypt decrypts a ciphertext.
//
// It implements K-PKE.Decrypt according to FIPS 203 (DRAFT), Algorithm 14,
// although the computation of s is done in parseDK.
internal static slice<byte> pkeDecrypt(ж<decryptionKey> Ꮡdx, ж<array<byte>> Ꮡc) {
    ref var dx = ref Ꮡdx.val;
    ref var c = ref Ꮡc.val;

    var u = new slice<ringElement>(k);
    foreach (var (i, _) in u) {
        var bΔ1 = (ж<array<byte>>)(c[(int)(encodingSize10 * i)..(int)(encodingSize10 * (i + 1))]);
        u[i] = ringDecodeAndDecompress10(bΔ1);
    }
    var b = (ж<array<byte>>)(c[(int)(encodingSize10 * k)..]);
    var v = ringDecodeAndDecompress4(b);
    nttElement mask = default!;                  // s⊺ ◦ NTT(u)
    foreach (var (i, _) in dx.s) {
        mask = polyAdd(mask, nttMul(dx.s[i], ntt(u[i])));
    }
    var w = polySub(v, inverseNTT(mask));
    return ringCompressAndEncode1(default!, w);
}

[GoType("num:uint16")] partial struct fieldElement;

// fieldCheckReduced checks that a value a is < q.
internal static (fieldElement, error) fieldCheckReduced(uint16 a) {
    if (a >= q) {
        return (0, errors.New("unreduced field element"u8));
    }
    return (((fieldElement)a), default!);
}

// fieldReduceOnce reduces a value a < 2q.
internal static fieldElement fieldReduceOnce(uint16 a) {
    var x = a - q;
    // If x underflowed, then x >= 2¹⁶ - q > 2¹⁵, so the top bit is set.
    x += (x >> (int)(15)) * q;
    return ((fieldElement)x);
}

internal static fieldElement fieldAdd(fieldElement a, fieldElement b) {
    var x = ((uint16)(a + b));
    return fieldReduceOnce(x);
}

internal static fieldElement fieldSub(fieldElement a, fieldElement b) {
    var x = ((uint16)(a - b + q));
    return fieldReduceOnce(x);
}

internal static readonly UntypedInt barrettMultiplier = 5039; // 2¹² * 2¹² / q
internal static readonly UntypedInt barrettShift = 24; // log₂(2¹² * 2¹²)

// fieldReduce reduces a value a < 2q² using Barrett reduction, to avoid
// potentially variable-time division.
internal static fieldElement fieldReduce(uint32 a) {
    var quotient = ((uint32)((((uint64)a) * barrettMultiplier) >> (int)(barrettShift)));
    return fieldReduceOnce(((uint16)(a - quotient * q)));
}

internal static fieldElement fieldMul(fieldElement a, fieldElement b) {
    var x = ((uint32)a) * ((uint32)b);
    return fieldReduce(x);
}

// fieldMulSub returns a * (b - c). This operation is fused to save a
// fieldReduceOnce after the subtraction.
internal static fieldElement fieldMulSub(fieldElement a, fieldElement b, fieldElement c) {
    var x = ((uint32)a) * ((uint32)(b - c + q));
    return fieldReduce(x);
}

// fieldAddMul returns a * b + c * d. This operation is fused to save a
// fieldReduceOnce and a fieldReduce.
internal static fieldElement fieldAddMul(fieldElement a, fieldElement b, fieldElement c, fieldElement d) {
    var x = ((uint32)a) * ((uint32)b);
    x += ((uint32)c) * ((uint32)d);
    return fieldReduce(x);
}

// compress maps a field element uniformly to the range 0 to 2ᵈ-1, according to
// FIPS 203 (DRAFT), Definition 4.5.
internal static uint16 compress(fieldElement x, uint8 d) {
    // We want to compute (x * 2ᵈ) / q, rounded to nearest integer, with 1/2
    // rounding up (see FIPS 203 (DRAFT), Section 2.3).
    // Barrett reduction produces a quotient and a remainder in the range [0, 2q),
    // such that dividend = quotient * q + remainder.
    var dividend = ((uint32)x) << (int)(d);
    // x * 2ᵈ
    var quotient = ((uint32)(((uint64)dividend) * barrettMultiplier >> (int)(barrettShift)));
    var remainder = dividend - quotient * q;
    // Since the remainder is in the range [0, 2q), not [0, q), we need to
    // portion it into three spans for rounding.
    //
    //     [ 0,       q/2     ) -> round to 0
    //     [ q/2,     q + q/2 ) -> round to 1
    //     [ q + q/2, 2q      ) -> round to 2
    //
    // We can convert that to the following logic: add 1 if remainder > q/2,
    // then add 1 again if remainder > q + q/2.
    //
    // Note that if remainder > x, then ⌊x⌋ - remainder underflows, and the top
    // bit of the difference will be set.
    quotient += (uint32)((q / 2 - remainder) >> (int)(31) & 1);
    quotient += (uint32)((q + q / 2 - remainder) >> (int)(31) & 1);
    // quotient might have overflowed at this point, so reduce it by masking.
    uint32 mask = (1 << (int)(d)) - 1;
    return ((uint16)((uint32)(quotient & mask)));
}

// decompress maps a number x between 0 and 2ᵈ-1 uniformly to the full range of
// field elements, according to FIPS 203 (DRAFT), Definition 4.6.
internal static fieldElement decompress(uint16 y, uint8 d) {
    // We want to compute (y * q) / 2ᵈ, rounded to nearest integer, with 1/2
    // rounding up (see FIPS 203 (DRAFT), Section 2.3).
    var dividend = ((uint32)y) * q;
    var quotient = dividend >> (int)(d);
    // (y * q) / 2ᵈ
    // The d'th least-significant bit of the dividend (the most significant bit
    // of the remainder) is 1 for the top half of the values that divide to the
    // same quotient, which are the ones that round up.
    quotient += (uint32)(dividend >> (int)((d - 1)) & 1);
    // quotient is at most (2¹¹-1) * q / 2¹¹ + 1 = 3328, so it didn't overflow.
    return ((fieldElement)quotient);
}

[GoType("[256]fieldElement")] /* [n]fieldElement */
partial struct ringElement;

// polyAdd adds two ringElements or nttElements.
internal static T /*s*/ polyAdd<T>(T a, T b)
    where T : /* ~[256]crypto/internal/mlkem768.fieldElement */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T s = default!;

    foreach (var (i, _) in s) {
        s[i] = fieldAdd(a[i], b[i]);
    }
    return s;
}

// polySub subtracts two ringElements or nttElements.
internal static T /*s*/ polySub<T>(T a, T b)
    where T : /* ~[256]crypto/internal/mlkem768.fieldElement */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    T s = default!;

    foreach (var (i, _) in s) {
        s[i] = fieldSub(a[i], b[i]);
    }
    return s;
}

// polyByteEncode appends the 384-byte encoding of f to b.
//
// It implements ByteEncode₁₂, according to FIPS 203 (DRAFT), Algorithm 4.
internal static slice<byte> polyByteEncode<T>(slice<byte> b, T f)
    where T : /* ~[256]crypto/internal/mlkem768.fieldElement */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    (@out, B) = sliceForAppend(b, encodingSize12);
    for (nint i = 0; i < n; i += 2) {
        var x = (uint32)(((uint32)f[i]) | ((uint32)f[i + 1]) << (int)(12));
        B[0] = ((uint8)x);
        B[1] = ((uint8)(x >> (int)(8)));
        B[2] = ((uint8)(x >> (int)(16)));
        B = B[3..];
    }
    return @out;
}

// polyByteDecode decodes the 384-byte encoding of a polynomial, checking that
// all the coefficients are properly reduced. This achieves the "Modulus check"
// step of ML-KEM Encapsulation Input Validation.
//
// polyByteDecode is also used in ML-KEM Decapsulation, where the input
// validation is not required, but implicitly allowed by the specification.
//
// It implements ByteDecode₁₂, according to FIPS 203 (DRAFT), Algorithm 5.
internal static (T, error) polyByteDecode<T>(slice<byte> b)
    where T : /* ~[256]crypto/internal/mlkem768.fieldElement */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    if (len(b) != encodingSize12) {
        return (new T{}, errors.New("mlkem768: invalid encoding length"u8));
    }
    T f = default!;
    for (nint i = 0; i < n; i += 2) {
        var d = (uint32)((uint32)(((uint32)b[0]) | ((uint32)b[1]) << (int)(8)) | ((uint32)b[2]) << (int)(16));
        static readonly UntypedInt mask12 = /* 0b1111_1111_1111 */ 4095;
        error err = default!;
        {
            (f[i], err) = fieldCheckReduced(((uint16)((uint32)(d & mask12)))); if (err != default!) {
                return (new T{}, errors.New("mlkem768: invalid polynomial encoding"u8));
            }
        }
        {
            (f[i + 1], err) = fieldCheckReduced(((uint16)(d >> (int)(12)))); if (err != default!) {
                return (new T{}, errors.New("mlkem768: invalid polynomial encoding"u8));
            }
        }
        b = b[3..];
    }
    return (f, default!);
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in[..(int)(total)];
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head[(int)(len(@in))..];
    return (head, tail);
}

// ringCompressAndEncode1 appends a 32-byte encoding of a ring element to s,
// compressing one coefficients per bit.
//
// It implements Compress₁, according to FIPS 203 (DRAFT), Definition 4.5,
// followed by ByteEncode₁, according to FIPS 203 (DRAFT), Algorithm 4.
internal static slice<byte> ringCompressAndEncode1(slice<byte> s, ringElement f) {
    (s, b) = sliceForAppend(s, encodingSize1);
    foreach (var (i, _) in b) {
        b[i] = 0;
    }
    foreach (var (i, _) in f) {
        b[i / 8] |= (uint8)(((uint8)(compress(f[i], 1) << (int)((i % 8)))));
    }
    return s;
}

// ringDecodeAndDecompress1 decodes a 32-byte slice to a ring element where each
// bit is mapped to 0 or ⌈q/2⌋.
//
// It implements ByteDecode₁, according to FIPS 203 (DRAFT), Algorithm 5,
// followed by Decompress₁, according to FIPS 203 (DRAFT), Definition 4.6.
internal static ringElement ringDecodeAndDecompress1(ж<array<byte>> Ꮡb) {
    ref var b = ref Ꮡb.val;

    ringElement f = default!;
    foreach (var (i, _) in f) {
        var b_i = (byte)(b[i / 8] >> (int)((i % 8)) & 1);
        static readonly UntypedInt halfQ = /* (q + 1) / 2 */ 1665; // ⌈q/2⌋, rounded up per FIPS 203 (DRAFT), Section 2.3
        f[i] = ((fieldElement)b_i) * halfQ;
    }
    // 0 decompresses to 0, and 1 to ⌈q/2⌋
    return f;
}

// ringCompressAndEncode4 appends a 128-byte encoding of a ring element to s,
// compressing two coefficients per byte.
//
// It implements Compress₄, according to FIPS 203 (DRAFT), Definition 4.5,
// followed by ByteEncode₄, according to FIPS 203 (DRAFT), Algorithm 4.
internal static slice<byte> ringCompressAndEncode4(slice<byte> s, ringElement f) {
    (s, b) = sliceForAppend(s, encodingSize4);
    for (nint i = 0; i < n; i += 2) {
        b[i / 2] = ((uint8)((uint16)(compress(f[i], 4) | compress(f[i + 1], 4) << (int)(4))));
    }
    return s;
}

// ringDecodeAndDecompress4 decodes a 128-byte encoding of a ring element where
// each four bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₄, according to FIPS 203 (DRAFT), Algorithm 5,
// followed by Decompress₄, according to FIPS 203 (DRAFT), Definition 4.6.
internal static ringElement ringDecodeAndDecompress4(ж<array<byte>> Ꮡb) {
    ref var b = ref Ꮡb.val;

    ringElement f = default!;
    for (nint i = 0; i < n; i += 2) {
        f[i] = ((fieldElement)decompress(((uint16)((byte)(b[i / 2] & 15))), 4));
        f[i + 1] = ((fieldElement)decompress(((uint16)(b[i / 2] >> (int)(4))), 4));
    }
    return f;
}

// ringCompressAndEncode10 appends a 320-byte encoding of a ring element to s,
// compressing four coefficients per five bytes.
//
// It implements Compress₁₀, according to FIPS 203 (DRAFT), Definition 4.5,
// followed by ByteEncode₁₀, according to FIPS 203 (DRAFT), Algorithm 4.
internal static slice<byte> ringCompressAndEncode10(slice<byte> s, ringElement f) {
    (s, b) = sliceForAppend(s, encodingSize10);
    for (nint i = 0; i < n; i += 4) {
        uint64 x = default!;
        x |= (uint64)(((uint64)compress(f[i + 0], 10)));
        x |= (uint64)(((uint64)compress(f[i + 1], 10)) << (int)(10));
        x |= (uint64)(((uint64)compress(f[i + 2], 10)) << (int)(20));
        x |= (uint64)(((uint64)compress(f[i + 3], 10)) << (int)(30));
        b[0] = ((uint8)x);
        b[1] = ((uint8)(x >> (int)(8)));
        b[2] = ((uint8)(x >> (int)(16)));
        b[3] = ((uint8)(x >> (int)(24)));
        b[4] = ((uint8)(x >> (int)(32)));
        b = b[5..];
    }
    return s;
}

// ringDecodeAndDecompress10 decodes a 320-byte encoding of a ring element where
// each ten bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₁₀, according to FIPS 203 (DRAFT), Algorithm 5,
// followed by Decompress₁₀, according to FIPS 203 (DRAFT), Definition 4.6.
internal static ringElement ringDecodeAndDecompress10(ж<array<byte>> Ꮡbb) {
    ref var bb = ref Ꮡbb.val;

    var b = bb[..];
    ringElement f = default!;
    for (nint i = 0; i < n; i += 4) {
        var x = (uint64)((uint64)((uint64)((uint64)(((uint64)b[0]) | ((uint64)b[1]) << (int)(8)) | ((uint64)b[2]) << (int)(16)) | ((uint64)b[3]) << (int)(24)) | ((uint64)b[4]) << (int)(32));
        b = b[5..];
        f[i] = ((fieldElement)decompress(((uint16)((uint64)(x >> (int)(0) & 1023))), 10));
        f[i + 1] = ((fieldElement)decompress(((uint16)((uint64)(x >> (int)(10) & 1023))), 10));
        f[i + 2] = ((fieldElement)decompress(((uint16)((uint64)(x >> (int)(20) & 1023))), 10));
        f[i + 3] = ((fieldElement)decompress(((uint16)((uint64)(x >> (int)(30) & 1023))), 10));
    }
    return f;
}

// samplePolyCBD draws a ringElement from the special Dη distribution given a
// stream of random bytes generated by the PRF function, according to FIPS 203
// (DRAFT), Algorithm 7 and Definition 4.1.
internal static ringElement samplePolyCBD(slice<byte> s, byte b) {
    var prf = sha3.NewShake256();
    prf.Write(s);
    prf.Write(new byte[]{b}.slice());
    var B = new slice<byte>(128);
    prf.Read(B);
    // SamplePolyCBD simply draws four (2η) bits for each coefficient, and adds
    // the first two and subtracts the last two.
    ringElement f = default!;
    for (nint i = 0; i < n; i += 2) {
        var bΔ1 = B[i / 2];
        var (b_7, b_6, b_5, b_4) = (bΔ1 >> (int)(7), (byte)(bΔ1 >> (int)(6) & 1), (byte)(bΔ1 >> (int)(5) & 1), (byte)(bΔ1 >> (int)(4) & 1));
        var (b_3, b_2, b_1, b_0) = ((byte)(bΔ1 >> (int)(3) & 1), (byte)(bΔ1 >> (int)(2) & 1), (byte)(bΔ1 >> (int)(1) & 1), (byte)(bΔ1 & 1));
        f[i] = fieldSub(((fieldElement)(b_0 + b_1)), ((fieldElement)(b_2 + b_3)));
        f[i + 1] = fieldSub(((fieldElement)(b_4 + b_5)), ((fieldElement)(b_6 + b_7)));
    }
    return f;
}

[GoType("[256]fieldElement")] /* [n]fieldElement */
partial struct nttElement;

// gammas are the values ζ^2BitRev7(i)+1 mod q for each index i.
internal static array<fieldElement> gammas = new fieldElement[]{17, 3312, 2761, 568, 583, 2746, 2649, 680, 1637, 1692, 723, 2606, 2288, 1041, 1100, 2229, 1409, 1920, 2662, 667, 3281, 48, 233, 3096, 756, 2573, 2156, 1173, 3015, 314, 3050, 279, 1703, 1626, 1651, 1678, 2789, 540, 1789, 1540, 1847, 1482, 952, 2377, 1461, 1868, 2687, 642, 939, 2390, 2308, 1021, 2437, 892, 2388, 941, 733, 2596, 2337, 992, 268, 3061, 641, 2688, 1584, 1745, 2298, 1031, 2037, 1292, 3220, 109, 375, 2954, 2549, 780, 2090, 1239, 1645, 1684, 1063, 2266, 319, 3010, 2773, 556, 757, 2572, 2099, 1230, 561, 2768, 2466, 863, 2594, 735, 2804, 525, 1092, 2237, 403, 2926, 1026, 2303, 1143, 2186, 2150, 1179, 2775, 554, 886, 2443, 1722, 1607, 1212, 2117, 1874, 1455, 1029, 2300, 2110, 1219, 2935, 394, 885, 2444, 2154, 1175}.array();

// nttMul multiplies two nttElements.
//
// It implements MultiplyNTTs, according to FIPS 203 (DRAFT), Algorithm 10.
internal static nttElement nttMul(nttElement f, nttElement g) {
    nttElement h = default!;
    // We use i += 2 for bounds check elimination. See https://go.dev/issue/66826.
    for (nint i = 0; i < 256; i += 2) {
        var (a0, a1) = (f[i], f[i + 1]);
        var (b0, b1) = (g[i], g[i + 1]);
        h[i] = fieldAddMul(a0, b0, fieldMul(a1, b1), gammas[i / 2]);
        h[i + 1] = fieldAddMul(a0, b1, a1, b0);
    }
    return h;
}

// zetas are the values ζ^BitRev7(k) mod q for each index k.
internal static array<fieldElement> zetas = new fieldElement[]{1, 1729, 2580, 3289, 2642, 630, 1897, 848, 1062, 1919, 193, 797, 2786, 3260, 569, 1746, 296, 2447, 1339, 1476, 3046, 56, 2240, 1333, 1426, 2094, 535, 2882, 2393, 2879, 1974, 821, 289, 331, 3253, 1756, 1197, 2304, 2277, 2055, 650, 1977, 2513, 632, 2865, 33, 1320, 1915, 2319, 1435, 807, 452, 1438, 2868, 1534, 2402, 2647, 2617, 1481, 648, 2474, 3110, 1227, 910, 17, 2761, 583, 2649, 1637, 723, 2288, 1100, 1409, 2662, 3281, 233, 756, 2156, 3015, 3050, 1703, 1651, 2789, 1789, 1847, 952, 1461, 2687, 939, 2308, 2437, 2388, 733, 2337, 268, 641, 1584, 2298, 2037, 3220, 375, 2549, 2090, 1645, 1063, 319, 2773, 757, 2099, 561, 2466, 2594, 2804, 1092, 403, 1026, 1143, 2150, 2775, 886, 1722, 1212, 1874, 1029, 2110, 2935, 885, 2154}.array();

// ntt maps a ringElement to its nttElement representation.
//
// It implements NTT, according to FIPS 203 (DRAFT), Algorithm 8.
internal static nttElement ntt(ringElement f) {
    nint k = 1;
    for (nint len = 128; len >= 2; len /= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            var zeta = zetas[k];
            k++;
            // Bounds check elimination hint.
            var fΔ1 = f[(int)(start)..(int)(start + len)];
            var flen = f[(int)(start + len)..(int)(start + len + len)];
            for (nint j = 0; j < len; j++) {
                var t = fieldMul(zeta, flen[j]);
                flen[j] = fieldSub(fΔ1[j], t);
                fΔ1[j] = fieldAdd(fΔ1[j], t);
            }
        }
    }
    return ((nttElement)f);
}

// inverseNTT maps a nttElement back to the ringElement it represents.
//
// It implements NTT⁻¹, according to FIPS 203 (DRAFT), Algorithm 9.
internal static ringElement inverseNTT(nttElement f) {
    nint k = 127;
    for (nint len = 2; len <= 128; len *= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            var zeta = zetas[k];
            k--;
            // Bounds check elimination hint.
            var fΔ1 = f[(int)(start)..(int)(start + len)];
            var flen = f[(int)(start + len)..(int)(start + len + len)];
            for (nint j = 0; j < len; j++) {
                var t = fΔ1[j];
                fΔ1[j] = fieldAdd(t, flen[j]);
                flen[j] = fieldMulSub(zeta, flen[j], t);
            }
        }
    }
    foreach (var (i, _) in f) {
        f[i] = fieldMul(f[i], 3303);
    }
    // 3303 = 128⁻¹ mod q
    return ((ringElement)f);
}

// sampleNTT draws a uniformly random nttElement from a stream of uniformly
// random bytes generated by the XOF function, according to FIPS 203 (DRAFT),
// Algorithm 6 and Definition 4.2.
internal static nttElement sampleNTT(slice<byte> rho, byte ii, byte jj) {
    var B = sha3.NewShake128();
    B.Write(rho);
    B.Write(new byte[]{ii, jj}.slice());
    // SampleNTT essentially draws 12 bits at a time from r, interprets them in
    // little-endian, and rejects values higher than q, until it drew 256
    // values. (The rejection rate is approximately 19%.)
    //
    // To do this from a bytes stream, it draws three bytes at a time, and
    // splits them into two uint16 appropriately masked.
    //
    //               r₀              r₁              r₂
    //       |- - - - - - - -|- - - - - - - -|- - - - - - - -|
    //
    //               Uint16(r₀ || r₁)
    //       |- - - - - - - - - - - - - - - -|
    //       |- - - - - - - - - - - -|
    //                   d₁
    //
    //                                Uint16(r₁ || r₂)
    //                       |- - - - - - - - - - - - - - - -|
    //                               |- - - - - - - - - - - -|
    //                                           d₂
    //
    // Note that in little-endian, the rightmost bits are the most significant
    // bits (dropped with a mask) and the leftmost bits are the least
    // significant bits (dropped with a right shift).
    nttElement a = default!;
    nint j = default!;              // index into a
    array<byte> buf = new(24);               // buffered reads from B
    nint off = len(buf);
    // index into buf, starts in a "buffer fully consumed" state
    while (ᐧ) {
        if (off >= len(buf)) {
            B.Read(buf[..]);
            off = 0;
        }
        var d1 = (uint16)(byteorder.LeUint16(buf[(int)(off)..]) & 4095);
        var d2 = byteorder.LeUint16(buf[(int)(off + 1)..]) >> (int)(4);
        off += 3;
        if (d1 < q) {
            a[j] = ((fieldElement)d1);
            j++;
        }
        if (j >= len(a)) {
            break;
        }
        if (d2 < q) {
            a[j] = ((fieldElement)d2);
            j++;
        }
        if (j >= len(a)) {
            break;
        }
    }
    return a;
}

} // end mlkem768_package
