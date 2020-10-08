// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 October 08 03:35:09 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\alert.go
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        private partial struct alert // : byte
        {
        }

 
        // alert level
        private static readonly long alertLevelWarning = (long)1L;
        private static readonly long alertLevelError = (long)2L;


        private static readonly alert alertCloseNotify = (alert)0L;
        private static readonly alert alertUnexpectedMessage = (alert)10L;
        private static readonly alert alertBadRecordMAC = (alert)20L;
        private static readonly alert alertDecryptionFailed = (alert)21L;
        private static readonly alert alertRecordOverflow = (alert)22L;
        private static readonly alert alertDecompressionFailure = (alert)30L;
        private static readonly alert alertHandshakeFailure = (alert)40L;
        private static readonly alert alertBadCertificate = (alert)42L;
        private static readonly alert alertUnsupportedCertificate = (alert)43L;
        private static readonly alert alertCertificateRevoked = (alert)44L;
        private static readonly alert alertCertificateExpired = (alert)45L;
        private static readonly alert alertCertificateUnknown = (alert)46L;
        private static readonly alert alertIllegalParameter = (alert)47L;
        private static readonly alert alertUnknownCA = (alert)48L;
        private static readonly alert alertAccessDenied = (alert)49L;
        private static readonly alert alertDecodeError = (alert)50L;
        private static readonly alert alertDecryptError = (alert)51L;
        private static readonly alert alertExportRestriction = (alert)60L;
        private static readonly alert alertProtocolVersion = (alert)70L;
        private static readonly alert alertInsufficientSecurity = (alert)71L;
        private static readonly alert alertInternalError = (alert)80L;
        private static readonly alert alertInappropriateFallback = (alert)86L;
        private static readonly alert alertUserCanceled = (alert)90L;
        private static readonly alert alertNoRenegotiation = (alert)100L;
        private static readonly alert alertMissingExtension = (alert)109L;
        private static readonly alert alertUnsupportedExtension = (alert)110L;
        private static readonly alert alertCertificateUnobtainable = (alert)111L;
        private static readonly alert alertUnrecognizedName = (alert)112L;
        private static readonly alert alertBadCertificateStatusResponse = (alert)113L;
        private static readonly alert alertBadCertificateHashValue = (alert)114L;
        private static readonly alert alertUnknownPSKIdentity = (alert)115L;
        private static readonly alert alertCertificateRequired = (alert)116L;
        private static readonly alert alertNoApplicationProtocol = (alert)120L;


        private static map alertText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<alert, @string>{alertCloseNotify:"close notify",alertUnexpectedMessage:"unexpected message",alertBadRecordMAC:"bad record MAC",alertDecryptionFailed:"decryption failed",alertRecordOverflow:"record overflow",alertDecompressionFailure:"decompression failure",alertHandshakeFailure:"handshake failure",alertBadCertificate:"bad certificate",alertUnsupportedCertificate:"unsupported certificate",alertCertificateRevoked:"revoked certificate",alertCertificateExpired:"expired certificate",alertCertificateUnknown:"unknown certificate",alertIllegalParameter:"illegal parameter",alertUnknownCA:"unknown certificate authority",alertAccessDenied:"access denied",alertDecodeError:"error decoding message",alertDecryptError:"error decrypting message",alertExportRestriction:"export restriction",alertProtocolVersion:"protocol version not supported",alertInsufficientSecurity:"insufficient security level",alertInternalError:"internal error",alertInappropriateFallback:"inappropriate fallback",alertUserCanceled:"user canceled",alertNoRenegotiation:"no renegotiation",alertMissingExtension:"missing extension",alertUnsupportedExtension:"unsupported extension",alertCertificateUnobtainable:"certificate unobtainable",alertUnrecognizedName:"unrecognized name",alertBadCertificateStatusResponse:"bad certificate status response",alertBadCertificateHashValue:"bad certificate hash value",alertUnknownPSKIdentity:"unknown PSK identity",alertCertificateRequired:"certificate required",alertNoApplicationProtocol:"no application protocol",};

        private static @string String(this alert e)
        {
            var (s, ok) = alertText[e];
            if (ok)
            {
                return "tls: " + s;
            }

            return "tls: alert(" + strconv.Itoa(int(e)) + ")";

        }

        private static @string Error(this alert e)
        {
            return e.String();
        }
    }
}}
