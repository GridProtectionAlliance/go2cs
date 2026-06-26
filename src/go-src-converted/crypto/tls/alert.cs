// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using strconv = strconv_package;

partial class tls_package {

[GoType("num:uint8")] partial struct AlertError;

public static @string Error(this AlertError e) {
    return ((alert)e).String();
}

[GoType("num:uint8")] partial struct alert;

internal static readonly UntypedInt alertLevelWarning = 1;
internal static readonly UntypedInt alertLevelError = 2;

internal static readonly alert alertCloseNotify = 0;
internal static readonly alert alertUnexpectedMessage = 10;
internal static readonly alert alertBadRecordMAC = 20;
internal static readonly alert alertDecryptionFailed = 21;
internal static readonly alert alertRecordOverflow = 22;
internal static readonly alert alertDecompressionFailure = 30;
internal static readonly alert alertHandshakeFailure = 40;
internal static readonly alert alertBadCertificate = 42;
internal static readonly alert alertUnsupportedCertificate = 43;
internal static readonly alert alertCertificateRevoked = 44;
internal static readonly alert alertCertificateExpired = 45;
internal static readonly alert alertCertificateUnknown = 46;
internal static readonly alert alertIllegalParameter = 47;
internal static readonly alert alertUnknownCA = 48;
internal static readonly alert alertAccessDenied = 49;
internal static readonly alert alertDecodeError = 50;
internal static readonly alert alertDecryptError = 51;
internal static readonly alert alertExportRestriction = 60;
internal static readonly alert alertProtocolVersion = 70;
internal static readonly alert alertInsufficientSecurity = 71;
internal static readonly alert alertInternalError = 80;
internal static readonly alert alertInappropriateFallback = 86;
internal static readonly alert alertUserCanceled = 90;
internal static readonly alert alertNoRenegotiation = 100;
internal static readonly alert alertMissingExtension = 109;
internal static readonly alert alertUnsupportedExtension = 110;
internal static readonly alert alertCertificateUnobtainable = 111;
internal static readonly alert alertUnrecognizedName = 112;
internal static readonly alert alertBadCertificateStatusResponse = 113;
internal static readonly alert alertBadCertificateHashValue = 114;
internal static readonly alert alertUnknownPSKIdentity = 115;
internal static readonly alert alertCertificateRequired = 116;
internal static readonly alert alertNoApplicationProtocol = 120;
internal static readonly alert alertECHRequired = 121;

internal static map<alert, @string> alertText = new map<alert, @string>{
    [alertCloseNotify] = "close notify"u8,
    [alertUnexpectedMessage] = "unexpected message"u8,
    [alertBadRecordMAC] = "bad record MAC"u8,
    [alertDecryptionFailed] = "decryption failed"u8,
    [alertRecordOverflow] = "record overflow"u8,
    [alertDecompressionFailure] = "decompression failure"u8,
    [alertHandshakeFailure] = "handshake failure"u8,
    [alertBadCertificate] = "bad certificate"u8,
    [alertUnsupportedCertificate] = "unsupported certificate"u8,
    [alertCertificateRevoked] = "revoked certificate"u8,
    [alertCertificateExpired] = "expired certificate"u8,
    [alertCertificateUnknown] = "unknown certificate"u8,
    [alertIllegalParameter] = "illegal parameter"u8,
    [alertUnknownCA] = "unknown certificate authority"u8,
    [alertAccessDenied] = "access denied"u8,
    [alertDecodeError] = "error decoding message"u8,
    [alertDecryptError] = "error decrypting message"u8,
    [alertExportRestriction] = "export restriction"u8,
    [alertProtocolVersion] = "protocol version not supported"u8,
    [alertInsufficientSecurity] = "insufficient security level"u8,
    [alertInternalError] = "internal error"u8,
    [alertInappropriateFallback] = "inappropriate fallback"u8,
    [alertUserCanceled] = "user canceled"u8,
    [alertNoRenegotiation] = "no renegotiation"u8,
    [alertMissingExtension] = "missing extension"u8,
    [alertUnsupportedExtension] = "unsupported extension"u8,
    [alertCertificateUnobtainable] = "certificate unobtainable"u8,
    [alertUnrecognizedName] = "unrecognized name"u8,
    [alertBadCertificateStatusResponse] = "bad certificate status response"u8,
    [alertBadCertificateHashValue] = "bad certificate hash value"u8,
    [alertUnknownPSKIdentity] = "unknown PSK identity"u8,
    [alertCertificateRequired] = "certificate required"u8,
    [alertNoApplicationProtocol] = "no application protocol"u8,
    [alertECHRequired] = "encrypted client hello required"u8
};

internal static @string String(this alert e) {
    @string s = alertText[e];
    var ok = alertText[e];
    if (ok) {
        return "tls: "u8 + s;
    }
    return "tls: alert("u8 + strconv.Itoa(((nint)e)) + ")"u8;
}

internal static @string Error(this alert e) {
    return e.String();
}

} // end tls_package
