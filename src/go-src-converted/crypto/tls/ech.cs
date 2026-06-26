// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hpke = crypto.@internal.hpke_package;
using errors = errors_package;
using strings = strings_package;
using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using crypto.@internal;
using golang.org.x.crypto;

partial class tls_package {

[GoType] partial struct echCipher {
    public uint16 KDFID;
    public uint16 AEADID;
}

[GoType] partial struct echExtension {
    public uint16 Type;
    public slice<byte> Data;
}

[GoType] partial struct echConfig {
    internal slice<byte> raw;
    public uint16 Version;
    public uint16 Length;
    public uint8 ConfigID;
    public uint16 KemID;
    public slice<byte> PublicKey;
    public slice<echCipher> SymmetricCipherSuite;
    public uint8 MaxNameLength;
    public slice<byte> PublicName;
    public slice<echExtension> Extensions;
}

internal static error errMalformedECHConfig = errors.New("tls: malformed ECHConfigList"u8);

// parseECHConfigList parses a draft-ietf-tls-esni-18 ECHConfigList, returning a
// slice of parsed ECHConfigs, in the same order they were parsed, or an error
// if the list is malformed.
internal static (slice<echConfig>, error) parseECHConfigList(slice<byte> data) {
    var s = ((cryptobyte.String)data);
    // Skip the length prefix
    ref var length = ref heap(new uint16(), out var Ꮡlength);
    if (!s.ReadUint16(Ꮡlength)) {
        return (default!, errMalformedECHConfig);
    }
    if (length != ((uint16)(len(data) - 2))) {
        return (default!, errMalformedECHConfig);
    }
    slice<echConfig> configs = default!;
    while (len(s) > 0) {
        ref var ec = ref heap(new echConfig(), out var Ꮡec);
        ec.raw = slice<byte>(s);
        if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑVersion))) {
            return (default!, errMalformedECHConfig);
        }
        if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑLength))) {
            return (default!, errMalformedECHConfig);
        }
        if (len(ec.raw) < ((nint)ec.Length) + 4) {
            return (default!, errMalformedECHConfig);
        }
        ec.raw = ec.raw[..(int)(ec.Length + 4)];
        if (ec.Version != extensionEncryptedClientHello) {
            s.Skip(((nint)ec.Length));
            continue;
        }
        if (!s.ReadUint8(Ꮡec.of(echConfig.ᏑConfigID))) {
            return (default!, errMalformedECHConfig);
        }
        if (!s.ReadUint16(Ꮡec.of(echConfig.ᏑKemID))) {
            return (default!, errMalformedECHConfig);
        }
        if (!s.ReadUint16LengthPrefixed((ж<cryptobyte.String>)(Ꮡec.of(echConfig.ᏑPublicKey)))) {
            return (default!, errMalformedECHConfig);
        }
        cryptobyte.String ΔcipherSuites = default!;
        if (!s.ReadUint16LengthPrefixed(Ꮡ(ΔcipherSuites))) {
            return (default!, errMalformedECHConfig);
        }
        while (!ΔcipherSuites.Empty()) {
            ref var c = ref heap(new echCipher(), out var Ꮡc);
            if (!ΔcipherSuites.ReadUint16(Ꮡc.of(echCipher.ᏑKDFID))) {
                return (default!, errMalformedECHConfig);
            }
            if (!ΔcipherSuites.ReadUint16(Ꮡc.of(echCipher.ᏑAEADID))) {
                return (default!, errMalformedECHConfig);
            }
            ec.SymmetricCipherSuite = append(ec.SymmetricCipherSuite, c);
        }
        if (!s.ReadUint8(Ꮡec.of(echConfig.ᏑMaxNameLength))) {
            return (default!, errMalformedECHConfig);
        }
        cryptobyte.String publicName = default!;
        if (!s.ReadUint8LengthPrefixed(Ꮡ(publicName))) {
            return (default!, errMalformedECHConfig);
        }
        ec.PublicName = publicName;
        cryptobyte.String extensions = default!;
        if (!s.ReadUint16LengthPrefixed(Ꮡ(extensions))) {
            return (default!, errMalformedECHConfig);
        }
        while (!extensions.Empty()) {
            ref var e = ref heap(new echExtension(), out var Ꮡe);
            if (!extensions.ReadUint16(Ꮡe.of(echExtension.ᏑType))) {
                return (default!, errMalformedECHConfig);
            }
            if (!extensions.ReadUint16LengthPrefixed((ж<cryptobyte.String>)(Ꮡe.of(echExtension.ᏑData)))) {
                return (default!, errMalformedECHConfig);
            }
            ec.Extensions = append(ec.Extensions, e);
        }
        configs = append(configs, ec);
    }
    return (configs, default!);
}

internal static ж<echConfig> pickECHConfig(slice<echConfig> list) {
    ref var ec = ref heap(new echConfig(), out var Ꮡec);

    foreach (var (_, ec) in list) {
        {
            var (_, ok) = hpke.SupportedKEMs[ec.KemID]; if (!ok) {
                continue;
            }
        }
        bool validSCS = default!;
        foreach (var (_, cs) in ec.SymmetricCipherSuite) {
            {
                var (_, ok) = hpke.SupportedAEADs[cs.AEADID]; if (!ok) {
                    continue;
                }
            }
            {
                var _ = hpke.SupportedKDFs[cs.KDFID];
                var ok = hpke.SupportedKDFs[cs.KDFID]; if (!ok) {
                    continue;
                }
            }
            validSCS = true;
            break;
        }
        if (!validSCS) {
            continue;
        }
        if (!validDNSName(((@string)ec.PublicName))) {
            continue;
        }
        bool unsupportedExt = default!;
        foreach (var (_, ext) in ec.Extensions) {
            // If high order bit is set to 1 the extension is mandatory.
            // Since we don't support any extensions, if we see a mandatory
            // bit, we skip the config.
            if ((uint16)(ext.Type & ((uint16)(1 << (int)(15)))) != 0) {
                unsupportedExt = true;
            }
        }
        if (unsupportedExt) {
            continue;
        }
        return Ꮡec;
    }
    return default!;
}

internal static (echCipher, error) pickECHCipherSuite(slice<echCipher> suites) {
    foreach (var (_, s) in suites) {
        // NOTE: all of the supported AEADs and KDFs are fine, rather than
        // imposing some sort of preference here, we just pick the first valid
        // suite.
        {
            var (_, ok) = hpke.SupportedAEADs[s.AEADID]; if (!ok) {
                continue;
            }
        }
        {
            var _ = hpke.SupportedKDFs[s.KDFID];
            var ok = hpke.SupportedKDFs[s.KDFID]; if (!ok) {
                continue;
            }
        }
        return (s, default!);
    }
    return (new echCipher(nil), errors.New("tls: no supported symmetric ciphersuites for ECH"u8));
}

internal static (slice<byte>, error) encodeInnerClientHello(ж<clientHelloMsg> Ꮡinner, nint maxNameLength) {
    ref var inner = ref Ꮡinner.val;

    (h, err) = inner.marshalMsg(true);
    if (err != default!) {
        return (default!, err);
    }
    h = h[4..];
    // strip four byte prefix
    nint paddingLen = default!;
    if (inner.serverName != ""u8){
        paddingLen = max(0, maxNameLength - len(inner.serverName));
    } else {
        paddingLen = maxNameLength + 9;
    }
    paddingLen = 31 - ((len(h) + paddingLen - 1) % 32);
    return (append(h, new slice<byte>(paddingLen).ꓸꓸꓸ), default!);
}

internal static (slice<byte>, error) generateOuterECHExt(uint8 id, uint16 kdfID, uint16 aeadID, slice<byte> encodedKey, slice<byte> payload) {
    cryptobyte.Builder b = default!;
    b.AddUint8(0);
    // outer
    b.AddUint16(kdfID);
    b.AddUint16(aeadID);
    b.AddUint8(id);
    b.AddUint16LengthPrefixed(
    var encodedKeyʗ2 = encodedKey;
    (ж<cryptobyte.Builder> b) => {
        bΔ1.AddBytes(encodedKeyʗ2);
    });
    b.AddUint16LengthPrefixed(
    var payloadʗ2 = payload;
    (ж<cryptobyte.Builder> b) => {
        bΔ2.AddBytes(payloadʗ2);
    });
    return b.Bytes();
}

internal static error computeAndUpdateOuterECHExtension(ж<clientHelloMsg> Ꮡouter, ж<clientHelloMsg> Ꮡinner, ж<echContext> Ꮡech, bool useKey) {
    ref var outer = ref Ꮡouter.val;
    ref var inner = ref Ꮡinner.val;
    ref var ech = ref Ꮡech.val;

    slice<byte> encapKey = default!;
    if (useKey) {
        encapKey = ech.encapsulatedKey;
    }
    (encodedInner, err) = encodeInnerClientHello(Ꮡinner, ((nint)ech.config.MaxNameLength));
    if (err != default!) {
        return err;
    }
    // NOTE: the tag lengths for all of the supported AEADs are the same (16
    // bytes), so we have hardcoded it here. If we add support for another AEAD
    // with a different tag length, we will need to change this.
    nint encryptedLen = len(encodedInner) + 16;
    // AEAD tag length
    (outer.encryptedClientHello, err) = generateOuterECHExt(ech.config.ConfigID, ech.kdfID, ech.aeadID, encapKey, new slice<byte>(encryptedLen));
    if (err != default!) {
        return err;
    }
    (serializedOuter, err) = outer.marshal();
    if (err != default!) {
        return err;
    }
    serializedOuter = serializedOuter[4..];
    // strip the four byte prefix
    (encryptedInner, err) = ech.hpkeContext.Seal(serializedOuter, encodedInner);
    if (err != default!) {
        return err;
    }
    (outer.encryptedClientHello, err) = generateOuterECHExt(ech.config.ConfigID, ech.kdfID, ech.aeadID, encapKey, encryptedInner);
    if (err != default!) {
        return err;
    }
    return default!;
}

// validDNSName is a rather rudimentary check for the validity of a DNS name.
// This is used to check if the public_name in a ECHConfig is valid when we are
// picking a config. This can be somewhat lax because even if we pick a
// valid-looking name, the DNS layer will later reject it anyway.
internal static bool validDNSName(@string name) {
    if (len(name) > 253) {
        return false;
    }
    var labels = strings.Split(name, "."u8);
    if (len(labels) <= 1) {
        return false;
    }
    foreach (var (_, l) in labels) {
        nint labelLen = len(l);
        if (labelLen == 0) {
            return false;
        }
        foreach (var (i, r) in l) {
            if (r == (rune)'-' && (i == 0 || i == labelLen - 1)) {
                return false;
            }
            if ((r < (rune)'0' || r > (rune)'9') && (r < (rune)'a' || r > (rune)'z') && (r < (rune)'A' || r > (rune)'Z') && r != (rune)'-') {
                return false;
            }
        }
    }
    return true;
}

// ECHRejectionError is the error type returned when ECH is rejected by a remote
// server. If the server offered a ECHConfigList to use for retries, the
// RetryConfigList field will contain this list.
//
// The client may treat an ECHRejectionError with an empty set of RetryConfigs
// as a secure signal from the server.
[GoType] partial struct ECHRejectionError {
    public slice<byte> RetryConfigList;
}

[GoRecv] public static @string Error(this ref ECHRejectionError e) {
    return "tls: server rejected ECH"u8;
}

} // end tls_package
