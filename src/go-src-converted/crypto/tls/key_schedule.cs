// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 13 05:36:11 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\key_schedule.go
namespace go.crypto;

using elliptic = crypto.elliptic_package;
using hmac = crypto.hmac_package;
using errors = errors_package;
using hash = hash_package;
using io = io_package;
using big = math.big_package;

using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using curve25519 = golang.org.x.crypto.curve25519_package;
using hkdf = golang.org.x.crypto.hkdf_package;


// This file contains the functions necessary to compute the TLS 1.3 key
// schedule. See RFC 8446, Section 7.


using System;public static partial class tls_package {

private static readonly @string resumptionBinderLabel = "res binder";
private static readonly @string clientHandshakeTrafficLabel = "c hs traffic";
private static readonly @string serverHandshakeTrafficLabel = "s hs traffic";
private static readonly @string clientApplicationTrafficLabel = "c ap traffic";
private static readonly @string serverApplicationTrafficLabel = "s ap traffic";
private static readonly @string exporterLabel = "exp master";
private static readonly @string resumptionLabel = "res master";
private static readonly @string trafficUpdateLabel = "traffic upd";

// expandLabel implements HKDF-Expand-Label from RFC 8446, Section 7.1.
private static slice<byte> expandLabel(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> secret, @string label, slice<byte> context, nint length) => func((_, panic, _) => {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    cryptobyte.Builder hkdfLabel = default;
    hkdfLabel.AddUint16(uint16(length));
    hkdfLabel.AddUint8LengthPrefixed(b => {
        b.AddBytes((slice<byte>)"tls13 ");
        b.AddBytes((slice<byte>)label);
    });
    hkdfLabel.AddUint8LengthPrefixed(b => {
        b.AddBytes(context);
    });
    var @out = make_slice<byte>(length);
    var (n, err) = hkdf.Expand(c.hash.New, secret, hkdfLabel.BytesOrPanic()).Read(out);
    if (err != null || n != length) {
        panic("tls: HKDF-Expand-Label invocation failed unexpectedly");
    }
    return out;
});

// deriveSecret implements Derive-Secret from RFC 8446, Section 7.1.
private static slice<byte> deriveSecret(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> secret, @string label, hash.Hash transcript) {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    if (transcript == null) {
        transcript = c.hash.New();
    }
    return c.expandLabel(secret, label, transcript.Sum(null), c.hash.Size());
}

// extract implements HKDF-Extract with the cipher suite hash.
private static slice<byte> extract(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> newSecret, slice<byte> currentSecret) {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    if (newSecret == null) {
        newSecret = make_slice<byte>(c.hash.Size());
    }
    return hkdf.Extract(c.hash.New, newSecret, currentSecret);
}

// nextTrafficSecret generates the next traffic secret, given the current one,
// according to RFC 8446, Section 7.2.
private static slice<byte> nextTrafficSecret(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> trafficSecret) {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    return c.expandLabel(trafficSecret, trafficUpdateLabel, null, c.hash.Size());
}

// trafficKey generates traffic keys according to RFC 8446, Section 7.3.
private static (slice<byte>, slice<byte>) trafficKey(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> trafficSecret) {
    slice<byte> key = default;
    slice<byte> iv = default;
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    key = c.expandLabel(trafficSecret, "key", null, c.keyLen);
    iv = c.expandLabel(trafficSecret, "iv", null, aeadNonceLength);
    return ;
}

// finishedHash generates the Finished verify_data or PskBinderEntry according
// to RFC 8446, Section 4.4.4. See sections 4.4 and 4.2.11.2 for the baseKey
// selection.
private static slice<byte> finishedHash(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> baseKey, hash.Hash transcript) {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    var finishedKey = c.expandLabel(baseKey, "finished", null, c.hash.Size());
    var verifyData = hmac.New(c.hash.New, finishedKey);
    verifyData.Write(transcript.Sum(null));
    return verifyData.Sum(null);
}

// exportKeyingMaterial implements RFC5705 exporters for TLS 1.3 according to
// RFC 8446, Section 7.5.
private static Func<@string, slice<byte>, nint, (slice<byte>, error)> exportKeyingMaterial(this ptr<cipherSuiteTLS13> _addr_c, slice<byte> masterSecret, hash.Hash transcript) {
    ref cipherSuiteTLS13 c = ref _addr_c.val;

    var expMasterSecret = c.deriveSecret(masterSecret, exporterLabel, transcript);
    return (label, context, length) => {
        var secret = c.deriveSecret(expMasterSecret, label, null);
        var h = c.hash.New();
        h.Write(context);
        return (c.expandLabel(secret, "exporter", h.Sum(null), length), null);
    };
}

// ecdheParameters implements Diffie-Hellman with either NIST curves or X25519,
// according to RFC 8446, Section 4.2.8.2.
private partial interface ecdheParameters {
    slice<byte> CurveID();
    slice<byte> PublicKey();
    slice<byte> SharedKey(slice<byte> peerPublicKey);
}

private static (ecdheParameters, error) generateECDHEParameters(io.Reader rand, CurveID curveID) {
    ecdheParameters _p0 = default;
    error _p0 = default!;

    if (curveID == X25519) {
        var privateKey = make_slice<byte>(curve25519.ScalarSize);
        {
            var (_, err) = io.ReadFull(rand, privateKey);

            if (err != null) {
                return (null, error.As(err)!);
            }

        }
        var (publicKey, err) = curve25519.X25519(privateKey, curve25519.Basepoint);
        if (err != null) {
            return (null, error.As(err)!);
        }
        return (addr(new x25519Parameters(privateKey:privateKey,publicKey:publicKey)), error.As(null!)!);
    }
    var (curve, ok) = curveForCurveID(curveID);
    if (!ok) {
        return (null, error.As(errors.New("tls: internal error: unsupported curve"))!);
    }
    ptr<nistParameters> p = addr(new nistParameters(curveID:curveID));
    error err = default!;
    p.privateKey, p.x, p.y, err = elliptic.GenerateKey(curve, rand);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (p, error.As(null!)!);
}

private static (elliptic.Curve, bool) curveForCurveID(CurveID id) {
    elliptic.Curve _p0 = default;
    bool _p0 = default;


    if (id == CurveP256) 
        return (elliptic.P256(), true);
    else if (id == CurveP384) 
        return (elliptic.P384(), true);
    else if (id == CurveP521) 
        return (elliptic.P521(), true);
    else 
        return (null, false);
    }

private partial struct nistParameters {
    public slice<byte> privateKey;
    public ptr<big.Int> x; // public key
    public ptr<big.Int> y; // public key
    public CurveID curveID;
}

private static CurveID CurveID(this ptr<nistParameters> _addr_p) {
    ref nistParameters p = ref _addr_p.val;

    return p.curveID;
}

private static slice<byte> PublicKey(this ptr<nistParameters> _addr_p) {
    ref nistParameters p = ref _addr_p.val;

    var (curve, _) = curveForCurveID(p.curveID);
    return elliptic.Marshal(curve, p.x, p.y);
}

private static slice<byte> SharedKey(this ptr<nistParameters> _addr_p, slice<byte> peerPublicKey) {
    ref nistParameters p = ref _addr_p.val;

    var (curve, _) = curveForCurveID(p.curveID); 
    // Unmarshal also checks whether the given point is on the curve.
    var (x, y) = elliptic.Unmarshal(curve, peerPublicKey);
    if (x == null) {
        return null;
    }
    var (xShared, _) = curve.ScalarMult(x, y, p.privateKey);
    var sharedKey = make_slice<byte>((curve.Params().BitSize + 7) / 8);
    return xShared.FillBytes(sharedKey);
}

private partial struct x25519Parameters {
    public slice<byte> privateKey;
    public slice<byte> publicKey;
}

private static CurveID CurveID(this ptr<x25519Parameters> _addr_p) {
    ref x25519Parameters p = ref _addr_p.val;

    return X25519;
}

private static slice<byte> PublicKey(this ptr<x25519Parameters> _addr_p) {
    ref x25519Parameters p = ref _addr_p.val;

    return p.publicKey[..];
}

private static slice<byte> SharedKey(this ptr<x25519Parameters> _addr_p, slice<byte> peerPublicKey) {
    ref x25519Parameters p = ref _addr_p.val;

    var (sharedKey, err) = curve25519.X25519(p.privateKey, peerPublicKey);
    if (err != null) {
        return null;
    }
    return sharedKey;
}

} // end tls_package
