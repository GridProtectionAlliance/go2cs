// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using hmac = crypto.hmac_package;
using md5 = crypto.md5_package;
using sha1 = crypto.sha1_package;
using sha256 = crypto.sha256_package;
using sha512 = crypto.sha512_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;

partial class tls_package {

// Split a premaster secret in two as specified in RFC 4346, Section 5.
internal static (slice<byte> s1, slice<byte> s2) splitPreMasterSecret(slice<byte> secret) {
    slice<byte> s1 = default!;
    slice<byte> s2 = default!;

    s1 = secret[0..(int)((len(secret) + 1) / 2)];
    s2 = secret[(int)(len(secret) / 2)..];
    return (s1, s2);
}

// pHash implements the P_hash function, as defined in RFC 4346, Section 5.
internal static void pHash(slice<byte> result, slice<byte> secret, slice<byte> seed, Func<hash.Hash> hash) {
    var h = hmac.New(hash, secret);
    h.Write(seed);
    var a = h.Sum(default!);
    nint j = 0;
    while (j < len(result)) {
        h.Reset();
        h.Write(a);
        h.Write(seed);
        var b = h.Sum(default!);
        copy(result[(int)(j)..], b);
        j += len(b);
        h.Reset();
        h.Write(a);
        a = h.Sum(default!);
    }
}

// prf10 implements the TLS 1.0 pseudo-random function, as defined in RFC 2246, Section 5.
internal static void prf10(slice<byte> result, slice<byte> secret, slice<byte> label, slice<byte> seed) {
    var hashSHA1 = sha1.New;
    var hashMD5 = md5.New;
    var labelAndSeed = new slice<byte>(len(label) + len(seed));
    copy(labelAndSeed, label);
    copy(labelAndSeed[(int)(len(label))..], seed);
    (s1, s2) = splitPreMasterSecret(secret);
    pHash(result, s1, labelAndSeed, hashMD5);
    var result2 = new slice<byte>(len(result));
    pHash(result2, s2, labelAndSeed, hashSHA1);
    foreach (var (i, b) in result2) {
        result[i] ^= (byte)(b);
    }
}

// prf12 implements the TLS 1.2 pseudo-random function, as defined in RFC 5246, Section 5.
internal static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prf12(Func<hash.Hash> hashFunc) {
    return (slice<byte> result, slice<byte> secret, slice<byte> label, slice<byte> seed) => {
        var labelAndSeed = new slice<byte>(len(label) + len(seed));
        copy(labelAndSeed, label);
        copy(labelAndSeed[(int)(len(label))..], seed);
        pHash(result, secret, labelAndSeed, hashFunc);
    };
}

internal static readonly UntypedInt masterSecretLength = 48; // Length of a master secret in TLS 1.1.
internal static readonly UntypedInt finishedVerifyLength = 12; // Length of verify_data in a Finished message.

internal static slice<byte> masterSecretLabel = slice<byte>("master secret");

internal static slice<byte> extendedMasterSecretLabel = slice<byte>("extended master secret");

internal static slice<byte> keyExpansionLabel = slice<byte>("key expansion");

internal static slice<byte> clientFinishedLabel = slice<byte>("client finished");

internal static slice<byte> serverFinishedLabel = slice<byte>("server finished");

internal static (Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>>, crypto.Hash) prfAndHashForVersion(uint16 version, ж<cipherSuite> Ꮡsuite) {
    ref var suite = ref Ꮡsuite.val;

    var exprᴛ1 = version;
    if (exprᴛ1 == VersionTLS10 || exprᴛ1 == VersionTLS11) {
        return (prf10, ((crypto.Hash)0));
    }
    if (exprᴛ1 == VersionTLS12) {
        if ((nint)(suite.flags & suiteSHA384) != 0) {
            return (prf12(sha512.New384), crypto.SHA384);
        }
        return (prf12(sha256.New), crypto.SHA256);
    }
    { /* default: */
        throw panic("unknown version");
    }

}

internal static Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prfForVersion(uint16 version, ж<cipherSuite> Ꮡsuite) {
    ref var suite = ref Ꮡsuite.val;

    var (prf, _) = prfAndHashForVersion(version, Ꮡsuite);
    return prf;
}

// masterFromPreMasterSecret generates the master secret from the pre-master
// secret. See RFC 5246, Section 8.1.
internal static slice<byte> masterFromPreMasterSecret(uint16 version, ж<cipherSuite> Ꮡsuite, slice<byte> preMasterSecret, slice<byte> clientRandom, slice<byte> serverRandom) {
    ref var suite = ref Ꮡsuite.val;

    var seed = new slice<byte>(0, len(clientRandom) + len(serverRandom));
    seed = append(seed, clientRandom.ꓸꓸꓸ);
    seed = append(seed, serverRandom.ꓸꓸꓸ);
    var masterSecret = new slice<byte>(masterSecretLength);
    prfForVersion(version, Ꮡsuite)(masterSecret, preMasterSecret, masterSecretLabel, seed);
    return masterSecret;
}

// extMasterFromPreMasterSecret generates the extended master secret from the
// pre-master secret. See RFC 7627.
internal static slice<byte> extMasterFromPreMasterSecret(uint16 version, ж<cipherSuite> Ꮡsuite, slice<byte> preMasterSecret, slice<byte> transcript) {
    ref var suite = ref Ꮡsuite.val;

    var masterSecret = new slice<byte>(masterSecretLength);
    prfForVersion(version, Ꮡsuite)(masterSecret, preMasterSecret, extendedMasterSecretLabel, transcript);
    return masterSecret;
}

// keysFromMasterSecret generates the connection keys from the master
// secret, given the lengths of the MAC key, cipher key and IV, as defined in
// RFC 2246, Section 6.3.
internal static (slice<byte> clientMAC, slice<byte> serverMAC, slice<byte> clientKey, slice<byte> serverKey, slice<byte> clientIV, slice<byte> serverIV) keysFromMasterSecret(uint16 version, ж<cipherSuite> Ꮡsuite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom, nint macLen, nint keyLen, nint ivLen) {
    slice<byte> clientMAC = default!;
    slice<byte> serverMAC = default!;
    slice<byte> clientKey = default!;
    slice<byte> serverKey = default!;
    slice<byte> clientIV = default!;
    slice<byte> serverIV = default!;

    ref var suite = ref Ꮡsuite.val;
    var seed = new slice<byte>(0, len(serverRandom) + len(clientRandom));
    seed = append(seed, serverRandom.ꓸꓸꓸ);
    seed = append(seed, clientRandom.ꓸꓸꓸ);
    nint n = 2 * macLen + 2 * keyLen + 2 * ivLen;
    var keyMaterial = new slice<byte>(n);
    prfForVersion(version, Ꮡsuite)(keyMaterial, masterSecret, keyExpansionLabel, seed);
    clientMAC = keyMaterial[..(int)(macLen)];
    keyMaterial = keyMaterial[(int)(macLen)..];
    serverMAC = keyMaterial[..(int)(macLen)];
    keyMaterial = keyMaterial[(int)(macLen)..];
    clientKey = keyMaterial[..(int)(keyLen)];
    keyMaterial = keyMaterial[(int)(keyLen)..];
    serverKey = keyMaterial[..(int)(keyLen)];
    keyMaterial = keyMaterial[(int)(keyLen)..];
    clientIV = keyMaterial[..(int)(ivLen)];
    keyMaterial = keyMaterial[(int)(ivLen)..];
    serverIV = keyMaterial[..(int)(ivLen)];
    return (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV);
}

internal static ΔfinishedHash newFinishedHash(uint16 version, ж<cipherSuite> ᏑcipherSuite) {
    ref var cipherSuite = ref ᏑcipherSuite.val;

    slice<byte> buffer = default!;
    if (version >= VersionTLS12) {
        buffer = new byte[]{}.slice();
    }
    var (prf, hash) = prfAndHashForVersion(version, ᏑcipherSuite);
    if (hash != 0) {
        return new ΔfinishedHash(hash.New(), hash.New(), default!, default!, buffer, version, prf);
    }
    return new ΔfinishedHash(sha1.New(), sha1.New(), md5.New(), md5.New(), buffer, version, prf);
}

// A finishedHash calculates the hash of a set of handshake messages suitable
// for including in a Finished message.
[GoType] partial struct ΔfinishedHash {
    internal hash_package.Hash client;
    internal hash_package.Hash server;
    // Prior to TLS 1.2, an additional MD5 hash is required.
    internal hash_package.Hash clientMD5;
    internal hash_package.Hash serverMD5;
    // In TLS 1.2, a full buffer is sadly required.
    internal slice<byte> buffer;
    internal uint16 version;
    internal Action<slice<byte>, slice<byte>, slice<byte>, slice<byte>> prf;
}

[GoRecv] public static (nint n, error err) Write(this ref ΔfinishedHash h, slice<byte> msg) {
    nint n = default!;
    error err = default!;

    h.client.Write(msg);
    h.server.Write(msg);
    if (h.version < VersionTLS12) {
        h.clientMD5.Write(msg);
        h.serverMD5.Write(msg);
    }
    if (h.buffer != default!) {
        h.buffer = append(h.buffer, msg.ꓸꓸꓸ);
    }
    return (len(msg), default!);
}

public static slice<byte> Sum(this ΔfinishedHash h) {
    if (h.version >= VersionTLS12) {
        return h.client.Sum(default!);
    }
    var @out = new slice<byte>(0, md5.ΔSize + sha1.ΔSize);
    @out = h.clientMD5.Sum(@out);
    return h.client.Sum(@out);
}

// clientSum returns the contents of the verify_data member of a client's
// Finished message.
internal static slice<byte> clientSum(this ΔfinishedHash h, slice<byte> masterSecret) {
    var @out = new slice<byte>(finishedVerifyLength);
    h.prf(@out, masterSecret, clientFinishedLabel, h.Sum());
    return @out;
}

// serverSum returns the contents of the verify_data member of a server's
// Finished message.
internal static slice<byte> serverSum(this ΔfinishedHash h, slice<byte> masterSecret) {
    var @out = new slice<byte>(finishedVerifyLength);
    h.prf(@out, masterSecret, serverFinishedLabel, h.Sum());
    return @out;
}

// hashForClientCertificate returns the handshake messages so far, pre-hashed if
// necessary, suitable for signing by a TLS client certificate.
internal static slice<byte> hashForClientCertificate(this ΔfinishedHash h, uint8 sigType, crypto.Hash hashAlg) {
    if ((h.version >= VersionTLS12 || sigType == signatureEd25519) && h.buffer == default!) {
        throw panic("tls: handshake hash for a client certificate requested after discarding the handshake buffer");
    }
    if (sigType == signatureEd25519) {
        return h.buffer;
    }
    if (h.version >= VersionTLS12) {
        var hash = hashAlg.New();
        hash.Write(h.buffer);
        return hash.Sum(default!);
    }
    if (sigType == signatureECDSA) {
        return h.server.Sum(default!);
    }
    return h.Sum();
}

// discardHandshakeBuffer is called when there is no more need to
// buffer the entirety of the handshake messages.
[GoRecv] internal static void discardHandshakeBuffer(this ref ΔfinishedHash h) {
    h.buffer = default!;
}

// noEKMBecauseRenegotiation is used as a value of
// ConnectionState.ekm when renegotiation is enabled and thus
// we wish to fail all key-material export requests.
internal static (slice<byte>, error) noEKMBecauseRenegotiation(@string label, slice<byte> context, nint length) {
    return (default!, errors.New("crypto/tls: ExportKeyingMaterial is unavailable when renegotiation is enabled"u8));
}

// noEKMBecauseNoEMS is used as a value of ConnectionState.ekm when Extended
// Master Secret is not negotiated and thus we wish to fail all key-material
// export requests.
internal static (slice<byte>, error) noEKMBecauseNoEMS(@string label, slice<byte> context, nint length) {
    return (default!, errors.New("crypto/tls: ExportKeyingMaterial is unavailable when neither TLS 1.3 nor Extended Master Secret are negotiated; override with GODEBUG=tlsunsafeekm=1"u8));
}

// ekmFromMasterSecret generates exported keying material as defined in RFC 5705.
internal static Func<@string, slice<byte>, nint, (<>byte, error)> ekmFromMasterSecret(uint16 version, ж<cipherSuite> Ꮡsuite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom) {
    ref var suite = ref Ꮡsuite.val;

    var clientRandomʗ1 = clientRandom;
    var masterSecretʗ1 = masterSecret;
    var serverRandomʗ1 = serverRandom;
    return (@string label, slice<byte> context, nint length) => {
        var exprᴛ1 = label;
        if (exprᴛ1 == "client finished"u8 || exprᴛ1 == "server finished"u8 || exprᴛ1 == "master secret"u8 || exprᴛ1 == "key expansion"u8) {
            return (default!, fmt.Errorf("crypto/tls: reserved ExportKeyingMaterial label: %s"u8, // These values are reserved and may not be used.
 label));
        }

        nint seedLen = len(serverRandomʗ1) + len(clientRandomʗ1);
        if (context != default!) {
            seedLen += 2 + len(context);
        }
        var seed = new slice<byte>(0, seedLen);
        seed = append(seed, clientRandomʗ1.ꓸꓸꓸ);
        seed = append(seed, serverRandomʗ1.ꓸꓸꓸ);
        if (context != default!) {
            if (len(context) >= 1 << (int)(16)) {
                return (default!, fmt.Errorf("crypto/tls: ExportKeyingMaterial context too long"u8));
            }
            seed = append(seed, ((byte)(len(context) >> (int)(8))), ((byte)len(context)));
            seed = append(seed, context.ꓸꓸꓸ);
        }
        var keyMaterial = new slice<byte>(length);
        prfForVersion(version, Ꮡsuite)(keyMaterial, masterSecretʗ1, slice<byte>(label), seed);
        return (keyMaterial, default!);
    };
}

} // end tls_package
