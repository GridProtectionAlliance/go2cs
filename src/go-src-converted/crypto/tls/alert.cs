// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 06 22:16:59 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\alert.go
using strconv = go.strconv_package;

namespace go.crypto;

public static partial class tls_package {

private partial struct alert { // : byte
}

 
// alert level
private static readonly nint alertLevelWarning = 1;
private static readonly nint alertLevelError = 2;


private static readonly alert alertCloseNotify = 0;
private static readonly alert alertUnexpectedMessage = 10;
private static readonly alert alertBadRecordMAC = 20;
private static readonly alert alertDecryptionFailed = 21;
private static readonly alert alertRecordOverflow = 22;
private static readonly alert alertDecompressionFailure = 30;
private static readonly alert alertHandshakeFailure = 40;
private static readonly alert alertBadCertificate = 42;
private static readonly alert alertUnsupportedCertificate = 43;
private static readonly alert alertCertificateRevoked = 44;
private static readonly alert alertCertificateExpired = 45;
private static readonly alert alertCertificateUnknown = 46;
private static readonly alert alertIllegalParameter = 47;
private static readonly alert alertUnknownCA = 48;
private static readonly alert alertAccessDenied = 49;
private static readonly alert alertDecodeError = 50;
private static readonly alert alertDecryptError = 51;
private static readonly alert alertExportRestriction = 60;
private static readonly alert alertProtocolVersion = 70;
private static readonly alert alertInsufficientSecurity = 71;
private static readonly alert alertInternalError = 80;
private static readonly alert alertInappropriateFallback = 86;
private static readonly alert alertUserCanceled = 90;
private static readonly alert alertNoRenegotiation = 100;
private static readonly alert alertMissingExtension = 109;
private static readonly alert alertUnsupportedExtension = 110;
private static readonly alert alertCertificateUnobtainable = 111;
private static readonly alert alertUnrecognizedName = 112;
private static readonly alert alertBadCertificateStatusResponse = 113;
private static readonly alert alertBadCertificateHashValue = 114;
private static readonly alert alertUnknownPSKIdentity = 115;
private static readonly alert alertCertificateRequired = 116;
private static readonly alert alertNoApplicationProtocol = 120;


private static map alertText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<alert, @string>{alertCloseNotify:"close notify",alertUnexpectedMessage:"unexpected message",alertBadRecordMAC:"bad record MAC",alertDecryptionFailed:"decryption failed",alertRecordOverflow:"record overflow",alertDecompressionFailure:"decompression failure",alertHandshakeFailure:"handshake failure",alertBadCertificate:"bad certificate",alertUnsupportedCertificate:"unsupported certificate",alertCertificateRevoked:"revoked certificate",alertCertificateExpired:"expired certificate",alertCertificateUnknown:"unknown certificate",alertIllegalParameter:"illegal parameter",alertUnknownCA:"unknown certificate authority",alertAccessDenied:"access denied",alertDecodeError:"error decoding message",alertDecryptError:"error decrypting message",alertExportRestriction:"export restriction",alertProtocolVersion:"protocol version not supported",alertInsufficientSecurity:"insufficient security level",alertInternalError:"internal error",alertInappropriateFallback:"inappropriate fallback",alertUserCanceled:"user canceled",alertNoRenegotiation:"no renegotiation",alertMissingExtension:"missing extension",alertUnsupportedExtension:"unsupported extension",alertCertificateUnobtainable:"certificate unobtainable",alertUnrecognizedName:"unrecognized name",alertBadCertificateStatusResponse:"bad certificate status response",alertBadCertificateHashValue:"bad certificate hash value",alertUnknownPSKIdentity:"unknown PSK identity",alertCertificateRequired:"certificate required",alertNoApplicationProtocol:"no application protocol",};

private static @string String(this alert e) {
    var (s, ok) = alertText[e];
    if (ok) {
        return "tls: " + s;
    }
    return "tls: alert(" + strconv.Itoa(int(e)) + ")";

}

private static @string Error(this alert e) {
    return e.String();
}

} // end tls_package
