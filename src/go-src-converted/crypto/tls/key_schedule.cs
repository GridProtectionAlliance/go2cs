// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = go.crypto.ecdh_package;
using hmac = go.crypto.hmac_package;
using mlkem768 = go.crypto.@internal.mlkem768_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using hkdf = vendor.golang.org.x.crypto.hkdf_package;
using sha3 = vendor.golang.org.x.crypto.sha3_package;
using go.crypto;
using go.crypto.@internal;
using vendor.golang.org.x.crypto;

partial class tls_package {

// This file contains the functions necessary to compute the TLS 1.3 key
// schedule. See RFC 8446, Section 7.
internal static readonly @string resumptionBinderLabel = "res binder"u8;
internal static readonly @string clientEarlyTrafficLabel = "c e traffic"u8;
internal static readonly @string clientHandshakeTrafficLabel = "c hs traffic"u8;
internal static readonly @string serverHandshakeTrafficLabel = "s hs traffic"u8;
internal static readonly @string clientApplicationTrafficLabel = "c ap traffic"u8;
internal static readonly @string serverApplicationTrafficLabel = "s ap traffic"u8;
internal static readonly @string exporterLabel = "exp master"u8;
internal static readonly @string resumptionLabel = "res master"u8;
internal static readonly @string trafficUpdateLabel = "traffic upd"u8;

// expandLabel implements HKDF-Expand-Label from RFC 8446, Section 7.1.
internal static slice<byte> expandLabel(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> secret, @string label, slice<byte> context, nint length) {
    ref var c = ref Ꮡc.Value;

    ref var hkdfLabel = ref heap(new cryptobyte.Builder(), out var ᏑhkdfLabel);
    hkdfLabel.AddUint16((uint16)length);
    ᏑhkdfLabel.AddUint8LengthPrefixed((ж<cryptobyte.Builder> b) => {
        b.AddBytes(slice<byte>((@string)"tls13 "));
        b.AddBytes(slice<byte>(label));
    });
    var contextʗ1 = context;
    ᏑhkdfLabel.AddUint8LengthPrefixed((ж<cryptobyte.Builder> b) => {
        b.AddBytes(contextʗ1);
    });
    var (hkdfLabelBytes, err) = hkdfLabel.Bytes();
    if (err != default!) {
        // Rather than calling BytesOrPanic, we explicitly handle this error, in
        // order to provide a reasonable error message. It should be basically
        // impossible for this to panic, and routing errors back through the
        // tree rooted in this function is quite painful. The labels are fixed
        // size, and the context is either a fixed-length computed hash, or
        // parsed from a field which has the same length limitation. As such, an
        // error here is likely to only be caused during development.
        //
        // NOTE: another reasonable approach here might be to return a
        // randomized slice if we encounter an error, which would break the
        // connection, but avoid panicking. This would perhaps be safer but
        // significantly more confusing to users.
        throw panic(fmt.Errorf("failed to construct HKDF label: %s"u8, err));
    }
    var @out = new slice<byte>(length);
    (var n, err) = hkdf.Expand(() => Ꮡc.Value.hash.New(), secret, hkdfLabelBytes).Read(@out);
    if (err != default! || n != length) {
        throw panic("tls: HKDF-Expand-Label invocation failed unexpectedly");
    }
    return @out;
}

// deriveSecret implements Derive-Secret from RFC 8446, Section 7.1.
internal static slice<byte> deriveSecret(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> secret, @string label, hash.Hash transcript) {
    ref var c = ref Ꮡc.Value;

    if (transcript == default!) {
        transcript = c.hash.New();
    }
    return Ꮡc.expandLabel(secret, label, transcript.Sum(default!), c.hash.Size());
}

// extract implements HKDF-Extract with the cipher suite hash.
internal static slice<byte> extract(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> newSecret, slice<byte> currentSecret) {
    ref var c = ref Ꮡc.Value;

    if (newSecret == default!) {
        newSecret = new slice<byte>(c.hash.Size());
    }
    return hkdf.Extract(() => Ꮡc.Value.hash.New(), newSecret, currentSecret);
}

// nextTrafficSecret generates the next traffic secret, given the current one,
// according to RFC 8446, Section 7.2.
internal static slice<byte> nextTrafficSecret(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) {
    ref var c = ref Ꮡc.Value;

    return Ꮡc.expandLabel(trafficSecret, trafficUpdateLabel, default!, c.hash.Size());
}

// trafficKey generates traffic keys according to RFC 8446, Section 7.3.
internal static (slice<byte> key, slice<byte> iv) trafficKey(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) {
    slice<byte> key = default!;
    slice<byte> iv = default!;

    ref var c = ref Ꮡc.Value;
    key = Ꮡc.expandLabel(trafficSecret, "key"u8, default!, c.keyLen);
    iv = Ꮡc.expandLabel(trafficSecret, "iv"u8, default!, aeadNonceLength);
    return (key, iv);
}

// finishedHash generates the Finished verify_data or PskBinderEntry according
// to RFC 8446, Section 4.4.4. See sections 4.4 and 4.2.11.2 for the baseKey
// selection.
internal static slice<byte> finishedHash(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> baseKey, hash.Hash transcript) {
    ref var c = ref Ꮡc.Value;

    var finishedKey = Ꮡc.expandLabel(baseKey, "finished"u8, default!, c.hash.Size());
    var verifyData = hmac.New(() => Ꮡc.Value.hash.New(), finishedKey);
    verifyData.Write(transcript.Sum(default!));
    return verifyData.Sum(default!);
}

// exportKeyingMaterial implements RFC5705 exporters for TLS 1.3 according to
// RFC 8446, Section 7.5.
internal static Func<@string, slice<byte>, nint, (slice<byte>, error)> exportKeyingMaterial(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> masterSecret, hash.Hash transcript) {
    ref var c = ref Ꮡc.Value;

    var expMasterSecret = Ꮡc.deriveSecret(masterSecret, exporterLabel, transcript);
    var expMasterSecretʗ1 = expMasterSecret;
    return (@string label, slice<byte> context, nint length) => {
        var secret = Ꮡc.deriveSecret(expMasterSecretʗ1, label, default!);
        var h = Ꮡc.Value.hash.New();
        h.Write(context);
        return (Ꮡc.expandLabel(secret, "exporter"u8, h.Sum(default!), length), default!);
    };
}

[GoType] partial struct keySharePrivateKeys {
    internal CurveID curveID;
    internal ж<ecdh.PrivateKey> ecdhe;
    internal ж<mlkem768.DecapsulationKey> kyber;
}

// kyberDecapsulate implements decapsulation according to Kyber Round 3.
internal static (slice<byte>, error) kyberDecapsulate(ж<mlkem768.DecapsulationKey> Ꮡdk, slice<byte> c) {
    ref var dk = ref Ꮡdk.Value;

    var (K, err) = mlkem768.Decapsulate(Ꮡdk, c);
    if (err != default!) {
        return (default!, err);
    }
    return (kyberSharedSecret(K, c), default!);
}

// kyberEncapsulate implements encapsulation according to Kyber Round 3.
internal static (slice<byte> c, slice<byte> ss, error err) kyberEncapsulate(slice<byte> ek) {
    slice<byte> c = default!;
    slice<byte> ss = default!;
    error err = default!;

    (c, ss, err) = mlkem768.Encapsulate(ek);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (c, kyberSharedSecret(ss, c), default!);
}

internal static slice<byte> kyberSharedSecret(slice<byte> K, slice<byte> c) {
    // Package mlkem768 implements ML-KEM, which compared to Kyber removed a
    // final hashing step. Compute SHAKE-256(K || SHA3-256(c), 32) to match Kyber.
    // See https://words.filippo.io/mlkem768/#bonus-track-using-a-ml-kem-implementation-as-kyber-v3.
    var h = sha3.NewShake256();
    h.Write(K);
    var ch = sha3.Sum256(c);
    h.Write(ch[..]);
    var @out = new slice<byte>(32);
    h.Read(@out);
    return @out;
}

internal static readonly UntypedInt x25519PublicKeySize = 32;

// generateECDHEKey returns a PrivateKey that implements Diffie-Hellman
// according to RFC 8446, Section 4.2.8.2.
internal static (ж<ecdh.PrivateKey>, error) generateECDHEKey(io.Reader rand, CurveID curveID) {
    var (curve, ok) = curveForCurveID(curveID);
    if (!ok) {
        return (default!, errors.New("tls: internal error: unsupported curve"u8));
    }
    return curve.GenerateKey(rand);
}

internal static (ecdhꓸCurve, bool) curveForCurveID(CurveID id) {
    var exprᴛ1 = id;
    if (exprᴛ1 == X25519) {
        return (ecdh.X25519(), true);
    }
    if (exprᴛ1 == CurveP256) {
        return (ecdh.P256(), true);
    }
    if (exprᴛ1 == CurveP384) {
        return (ecdh.P384(), true);
    }
    if (exprᴛ1 == CurveP521) {
        return (ecdh.P521(), true);
    }
    { /* default: */
        return (default!, false);
    }

}

internal static (CurveID, bool) curveIDForCurve(ecdhꓸCurve curve) {
    var exprᴛ1 = curve;
    if (AreEqual(exprᴛ1, ecdh.X25519())) {
        return (X25519, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P256())) {
        return (CurveP256, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P384())) {
        return (CurveP384, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P521())) {
        return (CurveP521, true);
    }
    { /* default: */
        return (0, false);
    }

}

} // end tls_package
