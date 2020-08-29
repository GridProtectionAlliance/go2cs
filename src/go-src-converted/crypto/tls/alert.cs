// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:28:18 UTC
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
        private static readonly long alertLevelWarning = 1L;
        private static readonly long alertLevelError = 2L;

        private static readonly alert alertCloseNotify = 0L;
        private static readonly alert alertUnexpectedMessage = 10L;
        private static readonly alert alertBadRecordMAC = 20L;
        private static readonly alert alertDecryptionFailed = 21L;
        private static readonly alert alertRecordOverflow = 22L;
        private static readonly alert alertDecompressionFailure = 30L;
        private static readonly alert alertHandshakeFailure = 40L;
        private static readonly alert alertBadCertificate = 42L;
        private static readonly alert alertUnsupportedCertificate = 43L;
        private static readonly alert alertCertificateRevoked = 44L;
        private static readonly alert alertCertificateExpired = 45L;
        private static readonly alert alertCertificateUnknown = 46L;
        private static readonly alert alertIllegalParameter = 47L;
        private static readonly alert alertUnknownCA = 48L;
        private static readonly alert alertAccessDenied = 49L;
        private static readonly alert alertDecodeError = 50L;
        private static readonly alert alertDecryptError = 51L;
        private static readonly alert alertProtocolVersion = 70L;
        private static readonly alert alertInsufficientSecurity = 71L;
        private static readonly alert alertInternalError = 80L;
        private static readonly alert alertInappropriateFallback = 86L;
        private static readonly alert alertUserCanceled = 90L;
        private static readonly alert alertNoRenegotiation = 100L;
        private static readonly alert alertNoApplicationProtocol = 120L;

        private static map alertText = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<alert, @string>{alertCloseNotify:"close notify",alertUnexpectedMessage:"unexpected message",alertBadRecordMAC:"bad record MAC",alertDecryptionFailed:"decryption failed",alertRecordOverflow:"record overflow",alertDecompressionFailure:"decompression failure",alertHandshakeFailure:"handshake failure",alertBadCertificate:"bad certificate",alertUnsupportedCertificate:"unsupported certificate",alertCertificateRevoked:"revoked certificate",alertCertificateExpired:"expired certificate",alertCertificateUnknown:"unknown certificate",alertIllegalParameter:"illegal parameter",alertUnknownCA:"unknown certificate authority",alertAccessDenied:"access denied",alertDecodeError:"error decoding message",alertDecryptError:"error decrypting message",alertProtocolVersion:"protocol version not supported",alertInsufficientSecurity:"insufficient security level",alertInternalError:"internal error",alertInappropriateFallback:"inappropriate fallback",alertUserCanceled:"user canceled",alertNoRenegotiation:"no renegotiation",alertNoApplicationProtocol:"no application protocol",};

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
