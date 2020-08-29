// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:37 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\ticket.go
using bytes = go.bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using hmac = go.crypto.hmac_package;
using sha256 = go.crypto.sha256_package;
using subtle = go.crypto.subtle_package;
using errors = go.errors_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // sessionState contains the information that is serialized into a session
        // ticket in order to later resume a connection.
        private partial struct sessionState
        {
            public ushort vers;
            public ushort cipherSuite;
            public slice<byte> masterSecret;
            public slice<slice<byte>> certificates; // usedOldKey is true if the ticket from which this session came from
// was encrypted with an older key and thus should be refreshed.
            public bool usedOldKey;
        }

        private static bool equal(this ref sessionState s, object i)
        {
            ref sessionState (s1, ok) = i._<ref sessionState>();
            if (!ok)
            {
                return false;
            }
            if (s.vers != s1.vers || s.cipherSuite != s1.cipherSuite || !bytes.Equal(s.masterSecret, s1.masterSecret))
            {
                return false;
            }
            if (len(s.certificates) != len(s1.certificates))
            {
                return false;
            }
            foreach (var (i) in s.certificates)
            {
                if (!bytes.Equal(s.certificates[i], s1.certificates[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static slice<byte> marshal(this ref sessionState s)
        {
            long length = 2L + 2L + 2L + len(s.masterSecret) + 2L;
            {
                var cert__prev1 = cert;

                foreach (var (_, __cert) in s.certificates)
                {
                    cert = __cert;
                    length += 4L + len(cert);
                }

                cert = cert__prev1;
            }

            var ret = make_slice<byte>(length);
            var x = ret;
            x[0L] = byte(s.vers >> (int)(8L));
            x[1L] = byte(s.vers);
            x[2L] = byte(s.cipherSuite >> (int)(8L));
            x[3L] = byte(s.cipherSuite);
            x[4L] = byte(len(s.masterSecret) >> (int)(8L));
            x[5L] = byte(len(s.masterSecret));
            x = x[6L..];
            copy(x, s.masterSecret);
            x = x[len(s.masterSecret)..];

            x[0L] = byte(len(s.certificates) >> (int)(8L));
            x[1L] = byte(len(s.certificates));
            x = x[2L..];

            {
                var cert__prev1 = cert;

                foreach (var (_, __cert) in s.certificates)
                {
                    cert = __cert;
                    x[0L] = byte(len(cert) >> (int)(24L));
                    x[1L] = byte(len(cert) >> (int)(16L));
                    x[2L] = byte(len(cert) >> (int)(8L));
                    x[3L] = byte(len(cert));
                    copy(x[4L..], cert);
                    x = x[4L + len(cert)..];
                }

                cert = cert__prev1;
            }

            return ret;
        }

        private static bool unmarshal(this ref sessionState s, slice<byte> data)
        {
            if (len(data) < 8L)
            {
                return false;
            }
            s.vers = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
            s.cipherSuite = uint16(data[2L]) << (int)(8L) | uint16(data[3L]);
            var masterSecretLen = int(data[4L]) << (int)(8L) | int(data[5L]);
            data = data[6L..];
            if (len(data) < masterSecretLen)
            {
                return false;
            }
            s.masterSecret = data[..masterSecretLen];
            data = data[masterSecretLen..];

            if (len(data) < 2L)
            {
                return false;
            }
            var numCerts = int(data[0L]) << (int)(8L) | int(data[1L]);
            data = data[2L..];

            s.certificates = make_slice<slice<byte>>(numCerts);
            foreach (var (i) in s.certificates)
            {
                if (len(data) < 4L)
                {
                    return false;
                }
                var certLen = int(data[0L]) << (int)(24L) | int(data[1L]) << (int)(16L) | int(data[2L]) << (int)(8L) | int(data[3L]);
                data = data[4L..];
                if (certLen < 0L)
                {
                    return false;
                }
                if (len(data) < certLen)
                {
                    return false;
                }
                s.certificates[i] = data[..certLen];
                data = data[certLen..];
            }
            return len(data) == 0L;
        }

        private static (slice<byte>, error) encryptTicket(this ref Conn c, ref sessionState state)
        {
            var serialized = state.marshal();
            var encrypted = make_slice<byte>(ticketKeyNameLen + aes.BlockSize + len(serialized) + sha256.Size);
            var keyName = encrypted[..ticketKeyNameLen];
            var iv = encrypted[ticketKeyNameLen..ticketKeyNameLen + aes.BlockSize];
            var macBytes = encrypted[len(encrypted) - sha256.Size..];

            {
                var (_, err) = io.ReadFull(c.config.rand(), iv);

                if (err != null)
                {
                    return (null, err);
                }

            }
            var key = c.config.ticketKeys()[0L];
            copy(keyName, key.keyName[..]);
            var (block, err) = aes.NewCipher(key.aesKey[..]);
            if (err != null)
            {
                return (null, errors.New("tls: failed to create cipher while encrypting ticket: " + err.Error()));
            }
            cipher.NewCTR(block, iv).XORKeyStream(encrypted[ticketKeyNameLen + aes.BlockSize..], serialized);

            var mac = hmac.New(sha256.New, key.hmacKey[..]);
            mac.Write(encrypted[..len(encrypted) - sha256.Size]);
            mac.Sum(macBytes[..0L]);

            return (encrypted, null);
        }

        private static (ref sessionState, bool) decryptTicket(this ref Conn c, slice<byte> encrypted)
        {
            if (c.config.SessionTicketsDisabled || len(encrypted) < ticketKeyNameLen + aes.BlockSize + sha256.Size)
            {
                return (null, false);
            }
            var keyName = encrypted[..ticketKeyNameLen];
            var iv = encrypted[ticketKeyNameLen..ticketKeyNameLen + aes.BlockSize];
            var macBytes = encrypted[len(encrypted) - sha256.Size..];

            var keys = c.config.ticketKeys();
            long keyIndex = -1L;
            foreach (var (i, candidateKey) in keys)
            {
                if (bytes.Equal(keyName, candidateKey.keyName[..]))
                {
                    keyIndex = i;
                    break;
                }
            }
            if (keyIndex == -1L)
            {
                return (null, false);
            }
            var key = ref keys[keyIndex];

            var mac = hmac.New(sha256.New, key.hmacKey[..]);
            mac.Write(encrypted[..len(encrypted) - sha256.Size]);
            var expected = mac.Sum(null);

            if (subtle.ConstantTimeCompare(macBytes, expected) != 1L)
            {
                return (null, false);
            }
            var (block, err) = aes.NewCipher(key.aesKey[..]);
            if (err != null)
            {
                return (null, false);
            }
            var ciphertext = encrypted[ticketKeyNameLen + aes.BlockSize..len(encrypted) - sha256.Size];
            var plaintext = ciphertext;
            cipher.NewCTR(block, iv).XORKeyStream(plaintext, ciphertext);

            sessionState state = ref new sessionState(usedOldKey:keyIndex>0);
            var ok = state.unmarshal(plaintext);
            return (state, ok);
        }
    }
}}
