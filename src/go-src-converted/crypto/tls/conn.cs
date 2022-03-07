// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TLS low level connection and record layer

// package tls -- go2cs converted at 2022 March 06 22:20:08 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\conn.go
using bytes = go.bytes_package;
using context = go.context_package;
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using io = go.io_package;
using net = go.net_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using System;
using System.Threading;


namespace go.crypto;

public static partial class tls_package {

    // A Conn represents a secured connection.
    // It implements the net.Conn interface.
public partial struct Conn {
    public net.Conn conn;
    public bool isClient;
    public Func<context.Context, error> handshakeFn; // (*Conn).clientHandshake or serverHandshake

// handshakeStatus is 1 if the connection is currently transferring
// application data (i.e. is not currently processing a handshake).
// This field is only to be accessed with sync/atomic.
    public uint handshakeStatus; // constant after handshake; protected by handshakeMutex
    public sync.Mutex handshakeMutex;
    public error handshakeErr; // error resulting from handshake
    public ushort vers; // TLS version
    public bool haveVers; // version has been negotiated
    public ptr<Config> config; // configuration passed to constructor
// handshakes counts the number of handshakes performed on the
// connection so far. If renegotiation is disabled then this is either
// zero or one.
    public nint handshakes;
    public bool didResume; // whether this connection was a session resumption
    public ushort cipherSuite;
    public slice<byte> ocspResponse; // stapled OCSP response
    public slice<slice<byte>> scts; // signed certificate timestamps from server
    public slice<ptr<x509.Certificate>> peerCertificates; // verifiedChains contains the certificate chains that we built, as
// opposed to the ones presented by the server.
    public slice<slice<ptr<x509.Certificate>>> verifiedChains; // serverName contains the server name indicated by the client, if any.
    public @string serverName; // secureRenegotiation is true if the server echoed the secure
// renegotiation extension. (This is meaningless as a server because
// renegotiation is not supported in that case.)
    public bool secureRenegotiation; // ekm is a closure for exporting keying material.
    public Func<@string, slice<byte>, nint, (slice<byte>, error)> ekm; // resumptionSecret is the resumption_master_secret for handling
// NewSessionTicket messages. nil if config.SessionTicketsDisabled.
    public slice<byte> resumptionSecret; // ticketKeys is the set of active session ticket keys for this
// connection. The first one is used to encrypt new tickets and
// all are tried to decrypt tickets.
    public slice<ticketKey> ticketKeys; // clientFinishedIsFirst is true if the client sent the first Finished
// message during the most recent handshake. This is recorded because
// the first transmitted Finished message is the tls-unique
// channel-binding value.
    public bool clientFinishedIsFirst; // closeNotifyErr is any error from sending the alertCloseNotify record.
    public error closeNotifyErr; // closeNotifySent is true if the Conn attempted to send an
// alertCloseNotify record.
    public bool closeNotifySent; // clientFinished and serverFinished contain the Finished message sent
// by the client or server in the most recent handshake. This is
// retained to support the renegotiation extension and tls-unique
// channel-binding.
    public array<byte> clientFinished;
    public array<byte> serverFinished; // clientProtocol is the negotiated ALPN protocol.
    public @string clientProtocol; // input/output
    public halfConn @in;
    public halfConn @out;
    public bytes.Buffer rawInput; // raw input, starting with a record header
    public bytes.Reader input; // application data waiting to be read, from rawInput.Next
    public bytes.Buffer hand; // handshake data waiting to be read
    public bool buffering; // whether records are buffered in sendBuf
    public slice<byte> sendBuf; // a buffer of records waiting to be sent

// bytesSent counts the bytes of application data sent.
// packetsSent counts packets.
    public long bytesSent;
    public long packetsSent; // retryCount counts the number of consecutive non-advancing records
// received by Conn.readRecord. That is, records that neither advance the
// handshake, nor deliver application data. Protected by in.Mutex.
    public nint retryCount; // activeCall is an atomic int32; the low bit is whether Close has
// been called. the rest of the bits are the number of goroutines
// in Conn.Write.
    public int activeCall;
    public array<byte> tmp;
}

// Access to net.Conn methods.
// Cannot just embed net.Conn because that would
// export the struct field too.

// LocalAddr returns the local network address.
private static net.Addr LocalAddr(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return c.conn.LocalAddr();
}

// RemoteAddr returns the remote network address.
private static net.Addr RemoteAddr(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return c.conn.RemoteAddr();
}

// SetDeadline sets the read and write deadlines associated with the connection.
// A zero value for t means Read and Write will not time out.
// After a Write has timed out, the TLS state is corrupt and all future writes will return the same error.
private static error SetDeadline(this ptr<Conn> _addr_c, time.Time t) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.conn.SetDeadline(t))!;
}

// SetReadDeadline sets the read deadline on the underlying connection.
// A zero value for t means Read will not time out.
private static error SetReadDeadline(this ptr<Conn> _addr_c, time.Time t) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.conn.SetReadDeadline(t))!;
}

// SetWriteDeadline sets the write deadline on the underlying connection.
// A zero value for t means Write will not time out.
// After a Write has timed out, the TLS state is corrupt and all future writes will return the same error.
private static error SetWriteDeadline(this ptr<Conn> _addr_c, time.Time t) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.conn.SetWriteDeadline(t))!;
}

// A halfConn represents one direction of the record layer
// connection, either sending or receiving.
private partial struct halfConn {
    public ref sync.Mutex Mutex => ref Mutex_val;
    public error err; // first permanent error
    public ushort version; // protocol version
    public hash.Hash mac;
    public array<byte> seq; // 64-bit sequence number

    public array<byte> scratchBuf; // to avoid allocs; interface method args escape

    public hash.Hash nextMac; // next MAC algorithm

    public slice<byte> trafficSecret; // current TLS 1.3 traffic secret
}

private partial struct permanentError {
    public net.Error err;
}

private static @string Error(this ptr<permanentError> _addr_e) {
    ref permanentError e = ref _addr_e.val;

    return e.err.Error();
}
private static error Unwrap(this ptr<permanentError> _addr_e) {
    ref permanentError e = ref _addr_e.val;

    return error.As(e.err)!;
}
private static bool Timeout(this ptr<permanentError> _addr_e) {
    ref permanentError e = ref _addr_e.val;

    return e.err.Timeout();
}
private static bool Temporary(this ptr<permanentError> _addr_e) {
    ref permanentError e = ref _addr_e.val;

    return false;
}

private static error setErrorLocked(this ptr<halfConn> _addr_hc, error err) {
    ref halfConn hc = ref _addr_hc.val;

    {
        net.Error (e, ok) = err._<net.Error>();

        if (ok) {
            hc.err = addr(new permanentError(err:e));
        }
        else
 {
            hc.err = err;
        }
    }

    return error.As(hc.err)!;

}

// prepareCipherSpec sets the encryption and MAC states
// that a subsequent changeCipherSpec will use.
private static void prepareCipherSpec(this ptr<halfConn> _addr_hc, ushort version, object cipher, hash.Hash mac) {
    ref halfConn hc = ref _addr_hc.val;

    hc.version = version;
    hc.nextCipher = cipher;
    hc.nextMac = mac;
}

// changeCipherSpec changes the encryption and MAC states
// to the ones previously passed to prepareCipherSpec.
private static error changeCipherSpec(this ptr<halfConn> _addr_hc) {
    ref halfConn hc = ref _addr_hc.val;

    if (hc.nextCipher == null || hc.version == VersionTLS13) {
        return error.As(alertInternalError)!;
    }
    hc.cipher = hc.nextCipher;
    hc.mac = hc.nextMac;
    hc.nextCipher = null;
    hc.nextMac = null;
    foreach (var (i) in hc.seq) {
        hc.seq[i] = 0;
    }    return error.As(null!)!;

}

private static void setTrafficSecret(this ptr<halfConn> _addr_hc, ptr<cipherSuiteTLS13> _addr_suite, slice<byte> secret) {
    ref halfConn hc = ref _addr_hc.val;
    ref cipherSuiteTLS13 suite = ref _addr_suite.val;

    hc.trafficSecret = secret;
    var (key, iv) = suite.trafficKey(secret);
    hc.cipher = suite.aead(key, iv);
    foreach (var (i) in hc.seq) {
        hc.seq[i] = 0;
    }
}

// incSeq increments the sequence number.
private static void incSeq(this ptr<halfConn> _addr_hc) => func((_, panic, _) => {
    ref halfConn hc = ref _addr_hc.val;

    for (nint i = 7; i >= 0; i--) {
        hc.seq[i]++;
        if (hc.seq[i] != 0) {
            return ;
        }
    } 

    // Not allowed to let sequence number wrap.
    // Instead, must renegotiate before it does.
    // Not likely enough to bother.
    panic("TLS: sequence number wraparound");

});

// explicitNonceLen returns the number of bytes of explicit nonce or IV included
// in each record. Explicit nonces are present only in CBC modes after TLS 1.0
// and in certain AEAD modes in TLS 1.2.
private static nint explicitNonceLen(this ptr<halfConn> _addr_hc) => func((_, panic, _) => {
    ref halfConn hc = ref _addr_hc.val;

    if (hc.cipher == null) {
        return 0;
    }
    switch (hc.cipher.type()) {
        case cipher.Stream c:
            return 0;
            break;
        case aead c:
            return c.explicitNonceLen();
            break;
        case cbcMode c:
            if (hc.version >= VersionTLS11) {
                return c.BlockSize();
            }
            return 0;
            break;
        default:
        {
            var c = hc.cipher.type();
            panic("unknown cipher type");
            break;
        }
    }

});

// extractPadding returns, in constant time, the length of the padding to remove
// from the end of payload. It also returns a byte which is equal to 255 if the
// padding was valid and 0 otherwise. See RFC 2246, Section 6.2.3.2.
private static (nint, byte) extractPadding(slice<byte> payload) {
    nint toRemove = default;
    byte good = default;

    if (len(payload) < 1) {
        return (0, 0);
    }
    var paddingLen = payload[len(payload) - 1];
    var t = uint(len(payload) - 1) - uint(paddingLen); 
    // if len(payload) >= (paddingLen - 1) then the MSB of t is zero
    good = byte(int32(~t) >> 31); 

    // The maximum possible padding length plus the actual length field
    nint toCheck = 256; 
    // The length of the padded data is public, so we can use an if here
    if (toCheck > len(payload)) {
        toCheck = len(payload);
    }
    for (nint i = 0; i < toCheck; i++) {
        t = uint(paddingLen) - uint(i); 
        // if i <= paddingLen then the MSB of t is zero
        var mask = byte(int32(~t) >> 31);
        var b = payload[len(payload) - 1 - i];
        good &= mask & paddingLen ^ mask & b;

    } 

    // We AND together the bits of good and replicate the result across
    // all the bits.
    good &= good << 4;
    good &= good << 2;
    good &= good << 1;
    good = uint8(int8(good) >> 7); 

    // Zero the padding length on error. This ensures any unchecked bytes
    // are included in the MAC. Otherwise, an attacker that could
    // distinguish MAC failures from padding failures could mount an attack
    // similar to POODLE in SSL 3.0: given a good ciphertext that uses a
    // full block's worth of padding, replace the final block with another
    // block. If the MAC check passed but the padding check failed, the
    // last byte of that block decrypted to the block size.
    //
    // See also macAndPaddingGood logic below.
    paddingLen &= good;

    toRemove = int(paddingLen) + 1;
    return ;

}

private static nint roundUp(nint a, nint b) {
    return a + (b - a % b) % b;
}

// cbcMode is an interface for block ciphers using cipher block chaining.
private partial interface cbcMode {
    void SetIV(slice<byte> _p0);
}

// decrypt authenticates and decrypts the record if protection is active at
// this stage. The returned plaintext might overlap with the input.
private static (slice<byte>, recordType, error) decrypt(this ptr<halfConn> _addr_hc, slice<byte> record) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    recordType _p0 = default;
    error _p0 = default!;
    ref halfConn hc = ref _addr_hc.val;

    slice<byte> plaintext = default;
    var typ = recordType(record[0]);
    var payload = record[(int)recordHeaderLen..]; 

    // In TLS 1.3, change_cipher_spec messages are to be ignored without being
    // decrypted. See RFC 8446, Appendix D.4.
    if (hc.version == VersionTLS13 && typ == recordTypeChangeCipherSpec) {
        return (payload, typ, error.As(null!)!);
    }
    var paddingGood = byte(255);
    nint paddingLen = 0;

    var explicitNonceLen = hc.explicitNonceLen();

    if (hc.cipher != null) {
        switch (hc.cipher.type()) {
            case cipher.Stream c:
                c.XORKeyStream(payload, payload);
                break;
            case aead c:
                if (len(payload) < explicitNonceLen) {
                    return (null, 0, error.As(alertBadRecordMAC)!);
                }
                var nonce = payload[..(int)explicitNonceLen];
                if (len(nonce) == 0) {
                    nonce = hc.seq[..];
                }
                payload = payload[(int)explicitNonceLen..];

                slice<byte> additionalData = default;
                if (hc.version == VersionTLS13) {
                    additionalData = record[..(int)recordHeaderLen];
                }
                else
 {
                    additionalData = append(hc.scratchBuf[..(int)0], hc.seq[..]);
                    additionalData = append(additionalData, record[..(int)3]);
                    var n = len(payload) - c.Overhead();
                    additionalData = append(additionalData, byte(n >> 8), byte(n));
                }

                error err = default!;
                plaintext, err = c.Open(payload[..(int)0], nonce, payload, additionalData);
                if (err != null) {
                    return (null, 0, error.As(alertBadRecordMAC)!);
                }

                break;
            case cbcMode c:
                var blockSize = c.BlockSize();
                var minPayload = explicitNonceLen + roundUp(hc.mac.Size() + 1, blockSize);
                if (len(payload) % blockSize != 0 || len(payload) < minPayload) {
                    return (null, 0, error.As(alertBadRecordMAC)!);
                }
                if (explicitNonceLen > 0) {
                    c.SetIV(payload[..(int)explicitNonceLen]);
                    payload = payload[(int)explicitNonceLen..];
                }
                c.CryptBlocks(payload, payload); 

                // In a limited attempt to protect against CBC padding oracles like
                // Lucky13, the data past paddingLen (which is secret) is passed to
                // the MAC function as extra data, to be fed into the HMAC after
                // computing the digest. This makes the MAC roughly constant time as
                // long as the digest computation is constant time and does not
                // affect the subsequent write, modulo cache effects.
                paddingLen, paddingGood = extractPadding(payload);
                break;
            default:
            {
                var c = hc.cipher.type();
                panic("unknown cipher type");
                break;
            }

        }

        if (hc.version == VersionTLS13) {
            if (typ != recordTypeApplicationData) {
                return (null, 0, error.As(alertUnexpectedMessage)!);
            }
            if (len(plaintext) > maxPlaintext + 1) {
                return (null, 0, error.As(alertRecordOverflow)!);
            } 
            // Remove padding and find the ContentType scanning from the end.
            for (var i = len(plaintext) - 1; i >= 0; i--) {
                if (plaintext[i] != 0) {
                    typ = recordType(plaintext[i]);
                    plaintext = plaintext[..(int)i];
                    break;
                }
                if (i == 0) {
                    return (null, 0, error.As(alertUnexpectedMessage)!);
                }
            }


        }
    else
    } {
        plaintext = payload;
    }
    if (hc.mac != null) {
        var macSize = hc.mac.Size();
        if (len(payload) < macSize) {
            return (null, 0, error.As(alertBadRecordMAC)!);
        }
        n = len(payload) - macSize - paddingLen;
        n = subtle.ConstantTimeSelect(int(uint32(n) >> 31), 0, n); // if n < 0 { n = 0 }
        record[3] = byte(n >> 8);
        record[4] = byte(n);
        var remoteMAC = payload[(int)n..(int)n + macSize];
        var localMAC = tls10MAC(hc.mac, hc.scratchBuf[..(int)0], hc.seq[..], record[..(int)recordHeaderLen], payload[..(int)n], payload[(int)n + macSize..]); 

        // This is equivalent to checking the MACs and paddingGood
        // separately, but in constant-time to prevent distinguishing
        // padding failures from MAC failures. Depending on what value
        // of paddingLen was returned on bad padding, distinguishing
        // bad MAC from bad padding can lead to an attack.
        //
        // See also the logic at the end of extractPadding.
        var macAndPaddingGood = subtle.ConstantTimeCompare(localMAC, remoteMAC) & int(paddingGood);
        if (macAndPaddingGood != 1) {
            return (null, 0, error.As(alertBadRecordMAC)!);
        }
        plaintext = payload[..(int)n];

    }
    hc.incSeq();
    return (plaintext, typ, error.As(null!)!);

});

// sliceForAppend extends the input slice by n bytes. head is the full extended
// slice, while tail is the appended part. If the original slice has sufficient
// capacity no allocation is performed.
private static (slice<byte>, slice<byte>) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default;
    slice<byte> tail = default;

    {
        var total = len(in) + n;

        if (cap(in) >= total) {
            head = in[..(int)total];
        }
        else
 {
            head = make_slice<byte>(total);
            copy(head, in);
        }
    }

    tail = head[(int)len(in)..];
    return ;

}

// encrypt encrypts payload, adding the appropriate nonce and/or MAC, and
// appends it to record, which must already contain the record header.
private static (slice<byte>, error) encrypt(this ptr<halfConn> _addr_hc, slice<byte> record, slice<byte> payload, io.Reader rand) => func((_, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref halfConn hc = ref _addr_hc.val;

    if (hc.cipher == null) {
        return (append(record, payload), error.As(null!)!);
    }
    slice<byte> explicitNonce = default;
    {
        var explicitNonceLen = hc.explicitNonceLen();

        if (explicitNonceLen > 0) {
            record, explicitNonce = sliceForAppend(record, explicitNonceLen);
            {
                cbcMode (_, isCBC) = cbcMode.As(hc.cipher._<cbcMode>())!;

                if (!isCBC && explicitNonceLen < 16) { 
                    // The AES-GCM construction in TLS has an explicit nonce so that the
                    // nonce can be random. However, the nonce is only 8 bytes which is
                    // too small for a secure, random nonce. Therefore we use the
                    // sequence number as the nonce. The 3DES-CBC construction also has
                    // an 8 bytes nonce but its nonces must be unpredictable (see RFC
                    // 5246, Appendix F.3), forcing us to use randomness. That's not
                    // 3DES' biggest problem anyway because the birthday bound on block
                    // collision is reached first due to its similarly small block size
                    // (see the Sweet32 attack).
                    copy(explicitNonce, hc.seq[..]);

                }
                else
 {
                    {
                        var (_, err) = io.ReadFull(rand, explicitNonce);

                        if (err != null) {
                            return (null, error.As(err)!);
                        }

                    }

                }

            }

        }
    }


    slice<byte> dst = default;
    switch (hc.cipher.type()) {
        case cipher.Stream c:
            var mac = tls10MAC(hc.mac, hc.scratchBuf[..(int)0], hc.seq[..], record[..(int)recordHeaderLen], payload, null);
            record, dst = sliceForAppend(record, len(payload) + len(mac));
            c.XORKeyStream(dst[..(int)len(payload)], payload);
            c.XORKeyStream(dst[(int)len(payload)..], mac);
            break;
        case aead c:
            var nonce = explicitNonce;
            if (len(nonce) == 0) {
                nonce = hc.seq[..];
            }
            if (hc.version == VersionTLS13) {
                record = append(record, payload); 

                // Encrypt the actual ContentType and replace the plaintext one.
                record = append(record, record[0]);
                record[0] = byte(recordTypeApplicationData);

                var n = len(payload) + 1 + c.Overhead();
                record[3] = byte(n >> 8);
                record[4] = byte(n);

                record = c.Seal(record[..(int)recordHeaderLen], nonce, record[(int)recordHeaderLen..], record[..(int)recordHeaderLen]);

            }
            else
 {
                var additionalData = append(hc.scratchBuf[..(int)0], hc.seq[..]);
                additionalData = append(additionalData, record[..(int)recordHeaderLen]);
                record = c.Seal(record, nonce, payload, additionalData);
            }

            break;
        case cbcMode c:
            mac = tls10MAC(hc.mac, hc.scratchBuf[..(int)0], hc.seq[..], record[..(int)recordHeaderLen], payload, null);
            var blockSize = c.BlockSize();
            var plaintextLen = len(payload) + len(mac);
            var paddingLen = blockSize - plaintextLen % blockSize;
            record, dst = sliceForAppend(record, plaintextLen + paddingLen);
            copy(dst, payload);
            copy(dst[(int)len(payload)..], mac);
            for (var i = plaintextLen; i < len(dst); i++) {
                dst[i] = byte(paddingLen - 1);
            }

            if (len(explicitNonce) > 0) {
                c.SetIV(explicitNonce);
            }
            c.CryptBlocks(dst, dst);
            break;
        default:
        {
            var c = hc.cipher.type();
            panic("unknown cipher type");
            break;
        } 

        // Update length to include nonce, MAC and any block padding needed.
    } 

    // Update length to include nonce, MAC and any block padding needed.
    n = len(record) - recordHeaderLen;
    record[3] = byte(n >> 8);
    record[4] = byte(n);
    hc.incSeq();

    return (record, error.As(null!)!);

});

// RecordHeaderError is returned when a TLS record header is invalid.
public partial struct RecordHeaderError {
    public @string Msg; // RecordHeader contains the five bytes of TLS record header that
// triggered the error.
    public array<byte> RecordHeader; // Conn provides the underlying net.Conn in the case that a client
// sent an initial handshake that didn't look like TLS.
// It is nil if there's already been a handshake or a TLS alert has
// been written to the connection.
    public net.Conn Conn;
}

public static @string Error(this RecordHeaderError e) {
    return "tls: " + e.Msg;
}

private static RecordHeaderError newRecordHeaderError(this ptr<Conn> _addr_c, net.Conn conn, @string msg) {
    RecordHeaderError err = default;
    ref Conn c = ref _addr_c.val;

    err.Msg = msg;
    err.Conn = conn;
    copy(err.RecordHeader[..], c.rawInput.Bytes());
    return err;
}

private static error readRecord(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.readRecordOrCCS(false))!;
}

private static error readChangeCipherSpec(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.readRecordOrCCS(true))!;
}

// readRecordOrCCS reads one or more TLS records from the connection and
// updates the record layer state. Some invariants:
//   * c.in must be locked
//   * c.input must be empty
// During the handshake one and only one of the following will happen:
//   - c.hand grows
//   - c.in.changeCipherSpec is called
//   - an error is returned
// After the handshake one and only one of the following will happen:
//   - c.hand grows
//   - c.input is set
//   - an error is returned
private static error readRecordOrCCS(this ptr<Conn> _addr_c, bool expectChangeCipherSpec) {
    ref Conn c = ref _addr_c.val;

    if (c.@in.err != null) {
        return error.As(c.@in.err)!;
    }
    var handshakeComplete = c.handshakeComplete(); 

    // This function modifies c.rawInput, which owns the c.input memory.
    if (c.input.Len() != 0) {
        return error.As(c.@in.setErrorLocked(errors.New("tls: internal error: attempted to read record with pending application data")))!;
    }
    c.input.Reset(null); 

    // Read header, payload.
    {
        var err__prev1 = err;

        var err = c.readFromUntil(c.conn, recordHeaderLen);

        if (err != null) { 
            // RFC 8446, Section 6.1 suggests that EOF without an alertCloseNotify
            // is an error, but popular web sites seem to do this, so we accept it
            // if and only if at the record boundary.
            if (err == io.ErrUnexpectedEOF && c.rawInput.Len() == 0) {
                err = io.EOF;
            }

            {
                net.Error e__prev2 = e;

                net.Error (e, ok) = err._<net.Error>();

                if (!ok || !e.Temporary()) {
                    c.@in.setErrorLocked(err);
                }

                e = e__prev2;

            }

            return error.As(err)!;

        }
        err = err__prev1;

    }

    var hdr = c.rawInput.Bytes()[..(int)recordHeaderLen];
    var typ = recordType(hdr[0]); 

    // No valid TLS record has a type of 0x80, however SSLv2 handshakes
    // start with a uint16 length where the MSB is set and the first record
    // is always < 256 bytes long. Therefore typ == 0x80 strongly suggests
    // an SSLv2 client.
    if (!handshakeComplete && typ == 0x80) {
        c.sendAlert(alertProtocolVersion);
        return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(null, "unsupported SSLv2 handshake received")))!;
    }
    var vers = uint16(hdr[1]) << 8 | uint16(hdr[2]);
    var n = int(hdr[3]) << 8 | int(hdr[4]);
    if (c.haveVers && c.vers != VersionTLS13 && vers != c.vers) {
        c.sendAlert(alertProtocolVersion);
        var msg = fmt.Sprintf("received record with version %x when expecting version %x", vers, c.vers);
        return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(null, msg)))!;
    }
    if (!c.haveVers) { 
        // First message, be extra suspicious: this might not be a TLS
        // client. Bail out before reading a full 'body', if possible.
        // The current max version is 3.3 so if the version is >= 16.0,
        // it's probably not real.
        if ((typ != recordTypeAlert && typ != recordTypeHandshake) || vers >= 0x1000) {
            return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(c.conn, "first record does not look like a TLS handshake")))!;
        }
    }
    if (c.vers == VersionTLS13 && n > maxCiphertextTLS13 || n > maxCiphertext) {
        c.sendAlert(alertRecordOverflow);
        msg = fmt.Sprintf("oversized record received with length %d", n);
        return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(null, msg)))!;
    }
    {
        var err__prev1 = err;

        err = c.readFromUntil(c.conn, recordHeaderLen + n);

        if (err != null) {
            {
                net.Error e__prev2 = e;

                (e, ok) = err._<net.Error>();

                if (!ok || !e.Temporary()) {
                    c.@in.setErrorLocked(err);
                }

                e = e__prev2;

            }

            return error.As(err)!;

        }
        err = err__prev1;

    } 

    // Process message.
    var record = c.rawInput.Next(recordHeaderLen + n);
    var (data, typ, err) = c.@in.decrypt(record);
    if (err != null) {
        return error.As(c.@in.setErrorLocked(c.sendAlert(err._<alert>())))!;
    }
    if (len(data) > maxPlaintext) {
        return error.As(c.@in.setErrorLocked(c.sendAlert(alertRecordOverflow)))!;
    }
    if (c.@in.cipher == null && typ == recordTypeApplicationData) {
        return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
    }
    if (typ != recordTypeAlert && typ != recordTypeChangeCipherSpec && len(data) > 0) { 
        // This is a state-advancing message: reset the retry count.
        c.retryCount = 0;

    }
    if (c.vers == VersionTLS13 && typ != recordTypeHandshake && c.hand.Len() > 0) {
        return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
    }

    if (typ == recordTypeAlert) 
        if (len(data) != 2) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        }
        if (alert(data[1]) == alertCloseNotify) {
            return error.As(c.@in.setErrorLocked(io.EOF))!;
        }
        if (c.vers == VersionTLS13) {
            return error.As(c.@in.setErrorLocked(addr(new net.OpError(Op:"remote error",Err:alert(data[1])))))!;
        }

        if (data[0] == alertLevelWarning) 
            // Drop the record on the floor and retry.
            return error.As(c.retryReadRecord(expectChangeCipherSpec))!;
        else if (data[0] == alertLevelError) 
            return error.As(c.@in.setErrorLocked(addr(new net.OpError(Op:"remote error",Err:alert(data[1])))))!;
        else 
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
            else if (typ == recordTypeChangeCipherSpec) 
        if (len(data) != 1 || data[0] != 1) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertDecodeError)))!;
        }
        if (c.hand.Len() > 0) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        }
        if (c.vers == VersionTLS13) {
            return error.As(c.retryReadRecord(expectChangeCipherSpec))!;
        }
        if (!expectChangeCipherSpec) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        }
        {
            var err__prev1 = err;

            err = c.@in.changeCipherSpec();

            if (err != null) {
                return error.As(c.@in.setErrorLocked(c.sendAlert(err._<alert>())))!;
            }

            err = err__prev1;

        }


    else if (typ == recordTypeApplicationData) 
        if (!handshakeComplete || expectChangeCipherSpec) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        }
        if (len(data) == 0) {
            return error.As(c.retryReadRecord(expectChangeCipherSpec))!;
        }
        c.input.Reset(data);
    else if (typ == recordTypeHandshake) 
        if (len(data) == 0 || expectChangeCipherSpec) {
            return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        }
        c.hand.Write(data);
    else 
        return error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!;
        return error.As(null!)!;

}

// retryReadRecord recurses into readRecordOrCCS to drop a non-advancing record, like
// a warning alert, empty application_data, or a change_cipher_spec in TLS 1.3.
private static error retryReadRecord(this ptr<Conn> _addr_c, bool expectChangeCipherSpec) {
    ref Conn c = ref _addr_c.val;

    c.retryCount++;
    if (c.retryCount > maxUselessRecords) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(c.@in.setErrorLocked(errors.New("tls: too many ignored records")))!;
    }
    return error.As(c.readRecordOrCCS(expectChangeCipherSpec))!;

}

// atLeastReader reads from R, stopping with EOF once at least N bytes have been
// read. It is different from an io.LimitedReader in that it doesn't cut short
// the last Read call, and in that it considers an early EOF an error.
private partial struct atLeastReader {
    public io.Reader R;
    public long N;
}

private static (nint, error) Read(this ptr<atLeastReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref atLeastReader r = ref _addr_r.val;

    if (r.N <= 0) {
        return (0, error.As(io.EOF)!);
    }
    var (n, err) = r.R.Read(p);
    r.N -= int64(n); // won't underflow unless len(p) >= n > 9223372036854775809
    if (r.N > 0 && err == io.EOF) {
        return (n, error.As(io.ErrUnexpectedEOF)!);
    }
    if (r.N <= 0 && err == null) {
        return (n, error.As(io.EOF)!);
    }
    return (n, error.As(err)!);

}

// readFromUntil reads from r into c.rawInput until c.rawInput contains
// at least n bytes or else returns an error.
private static error readFromUntil(this ptr<Conn> _addr_c, io.Reader r, nint n) {
    ref Conn c = ref _addr_c.val;

    if (c.rawInput.Len() >= n) {
        return error.As(null!)!;
    }
    var needs = n - c.rawInput.Len(); 
    // There might be extra input waiting on the wire. Make a best effort
    // attempt to fetch it so that it can be used in (*Conn).Read to
    // "predict" closeNotify alerts.
    c.rawInput.Grow(needs + bytes.MinRead);
    var (_, err) = c.rawInput.ReadFrom(addr(new atLeastReader(r,int64(needs))));
    return error.As(err)!;

}

// sendAlert sends a TLS alert message.
private static error sendAlertLocked(this ptr<Conn> _addr_c, alert err) {
    ref Conn c = ref _addr_c.val;


    if (err == alertNoRenegotiation || err == alertCloseNotify) 
        c.tmp[0] = alertLevelWarning;
    else 
        c.tmp[0] = alertLevelError;
        c.tmp[1] = byte(err);

    var (_, writeErr) = c.writeRecordLocked(recordTypeAlert, c.tmp[(int)0..(int)2]);
    if (err == alertCloseNotify) { 
        // closeNotify is a special case in that it isn't an error.
        return error.As(writeErr)!;

    }
    return error.As(c.@out.setErrorLocked(addr(new net.OpError(Op:"local error",Err:err))))!;

}

// sendAlert sends a TLS alert message.
private static error sendAlert(this ptr<Conn> _addr_c, alert err) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    c.@out.Lock();
    defer(c.@out.Unlock());
    return error.As(c.sendAlertLocked(err))!;
});

 
// tcpMSSEstimate is a conservative estimate of the TCP maximum segment
// size (MSS). A constant is used, rather than querying the kernel for
// the actual MSS, to avoid complexity. The value here is the IPv6
// minimum MTU (1280 bytes) minus the overhead of an IPv6 header (40
// bytes) and a TCP header with timestamps (32 bytes).
private static readonly nint tcpMSSEstimate = 1208; 

// recordSizeBoostThreshold is the number of bytes of application data
// sent after which the TLS record size will be increased to the
// maximum.
private static readonly nint recordSizeBoostThreshold = 128 * 1024;


// maxPayloadSizeForWrite returns the maximum TLS payload size to use for the
// next application data record. There is the following trade-off:
//
//   - For latency-sensitive applications, such as web browsing, each TLS
//     record should fit in one TCP segment.
//   - For throughput-sensitive applications, such as large file transfers,
//     larger TLS records better amortize framing and encryption overheads.
//
// A simple heuristic that works well in practice is to use small records for
// the first 1MB of data, then use larger records for subsequent data, and
// reset back to smaller records after the connection becomes idle. See "High
// Performance Web Networking", Chapter 4, or:
// https://www.igvita.com/2013/10/24/optimizing-tls-record-size-and-buffering-latency/
//
// In the interests of simplicity and determinism, this code does not attempt
// to reset the record size once the connection is idle, however.
private static nint maxPayloadSizeForWrite(this ptr<Conn> _addr_c, recordType typ) => func((_, panic, _) => {
    ref Conn c = ref _addr_c.val;

    if (c.config.DynamicRecordSizingDisabled || typ != recordTypeApplicationData) {
        return maxPlaintext;
    }
    if (c.bytesSent >= recordSizeBoostThreshold) {
        return maxPlaintext;
    }
    var payloadBytes = tcpMSSEstimate - recordHeaderLen - c.@out.explicitNonceLen();
    if (c.@out.cipher != null) {
        switch (c.@out.cipher.type()) {
            case cipher.Stream ciph:
                payloadBytes -= c.@out.mac.Size();
                break;
            case cipher.AEAD ciph:
                payloadBytes -= ciph.Overhead();
                break;
            case cbcMode ciph:
                var blockSize = ciph.BlockSize(); 
                // The payload must fit in a multiple of blockSize, with
                // room for at least one padding byte.
                payloadBytes = (payloadBytes & ~(blockSize - 1)) - 1; 
                // The MAC is appended before padding so affects the
                // payload size directly.
                payloadBytes -= c.@out.mac.Size();
                break;
            default:
            {
                var ciph = c.@out.cipher.type();
                panic("unknown cipher type");
                break;
            }
        }

    }
    if (c.vers == VersionTLS13) {
        payloadBytes--; // encrypted ContentType
    }
    var pkt = c.packetsSent;
    c.packetsSent++;
    if (pkt > 1000) {
        return maxPlaintext; // avoid overflow in multiply below
    }
    var n = payloadBytes * int(pkt + 1);
    if (n > maxPlaintext) {
        n = maxPlaintext;
    }
    return n;

});

private static (nint, error) write(this ptr<Conn> _addr_c, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    if (c.buffering) {
        c.sendBuf = append(c.sendBuf, data);
        return (len(data), error.As(null!)!);
    }
    var (n, err) = c.conn.Write(data);
    c.bytesSent += int64(n);
    return (n, error.As(err)!);

}

private static (nint, error) flush(this ptr<Conn> _addr_c) {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    if (len(c.sendBuf) == 0) {
        return (0, error.As(null!)!);
    }
    var (n, err) = c.conn.Write(c.sendBuf);
    c.bytesSent += int64(n);
    c.sendBuf = null;
    c.buffering = false;
    return (n, error.As(err)!);

}

// outBufPool pools the record-sized scratch buffers used by writeRecordLocked.
private static sync.Pool outBufPool = new sync.Pool(New:func()interface{}{returnnew([]byte)},);

// writeRecordLocked writes a TLS record with the given type and payload to the
// connection and updates the record layer state.
private static (nint, error) writeRecordLocked(this ptr<Conn> _addr_c, recordType typ, slice<byte> data) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    ptr<slice<byte>> outBufPtr = outBufPool.Get()._<ptr<slice<byte>>>();
    var outBuf = outBufPtr.val;
    defer(() => { 
        // You might be tempted to simplify this by just passing &outBuf to Put,
        // but that would make the local copy of the outBuf slice header escape
        // to the heap, causing an allocation. Instead, we keep around the
        // pointer to the slice header returned by Get, which is already on the
        // heap, and overwrite and return that.
        outBufPtr.val = outBuf;
        outBufPool.Put(outBufPtr);

    }());

    nint n = default;
    while (len(data) > 0) {
        var m = len(data);
        {
            var maxPayload = c.maxPayloadSizeForWrite(typ);

            if (m > maxPayload) {
                m = maxPayload;
            }

        }


        _, outBuf = sliceForAppend(outBuf[..(int)0], recordHeaderLen);
        outBuf[0] = byte(typ);
        var vers = c.vers;
        if (vers == 0) { 
            // Some TLS servers fail if the record version is
            // greater than TLS 1.0 for the initial ClientHello.
            vers = VersionTLS10;

        }
        else if (vers == VersionTLS13) { 
            // TLS 1.3 froze the record layer version to 1.2.
            // See RFC 8446, Section 5.1.
            vers = VersionTLS12;

        }
        outBuf[1] = byte(vers >> 8);
        outBuf[2] = byte(vers);
        outBuf[3] = byte(m >> 8);
        outBuf[4] = byte(m);

        error err = default!;
        outBuf, err = c.@out.encrypt(outBuf, data[..(int)m], c.config.rand());
        if (err != null) {
            return (n, error.As(err)!);
        }
        {
            error err__prev1 = err;

            var (_, err) = c.write(outBuf);

            if (err != null) {
                return (n, error.As(err)!);
            }

            err = err__prev1;

        }

        n += m;
        data = data[(int)m..];

    }

    if (typ == recordTypeChangeCipherSpec && c.vers != VersionTLS13) {
        {
            error err__prev2 = err;

            err = c.@out.changeCipherSpec();

            if (err != null) {
                return (n, error.As(c.sendAlertLocked(err._<alert>()))!);
            }

            err = err__prev2;

        }

    }
    return (n, error.As(null!)!);

});

// writeRecord writes a TLS record with the given type and payload to the
// connection and updates the record layer state.
private static (nint, error) writeRecord(this ptr<Conn> _addr_c, recordType typ, slice<byte> data) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    c.@out.Lock();
    defer(c.@out.Unlock());

    return c.writeRecordLocked(typ, data);
});

// readHandshake reads the next handshake message from
// the record layer.
private static (object, error) readHandshake(this ptr<Conn> _addr_c) {
    object _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    while (c.hand.Len() < 4) {
        {
            var err__prev1 = err;

            var err = c.readRecord();

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

    }

    var data = c.hand.Bytes();
    var n = int(data[1]) << 16 | int(data[2]) << 8 | int(data[3]);
    if (n > maxHandshake) {
        c.sendAlertLocked(alertInternalError);
        return (null, error.As(c.@in.setErrorLocked(fmt.Errorf("tls: handshake message of length %d bytes exceeds maximum of %d bytes", n, maxHandshake)))!);
    }
    while (c.hand.Len() < 4 + n) {
        {
            var err__prev1 = err;

            err = c.readRecord();

            if (err != null) {
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

    }
    data = c.hand.Next(4 + n);
    handshakeMessage m = default;

    if (data[0] == typeHelloRequest) 
        m = @new<helloRequestMsg>();
    else if (data[0] == typeClientHello) 
        m = @new<clientHelloMsg>();
    else if (data[0] == typeServerHello) 
        m = @new<serverHelloMsg>();
    else if (data[0] == typeNewSessionTicket) 
        if (c.vers == VersionTLS13) {
            m = @new<newSessionTicketMsgTLS13>();
        }
        else
 {
            m = @new<newSessionTicketMsg>();
        }
    else if (data[0] == typeCertificate) 
        if (c.vers == VersionTLS13) {
            m = @new<certificateMsgTLS13>();
        }
        else
 {
            m = @new<certificateMsg>();
        }
    else if (data[0] == typeCertificateRequest) 
        if (c.vers == VersionTLS13) {
            m = @new<certificateRequestMsgTLS13>();
        }
        else
 {
            m = addr(new certificateRequestMsg(hasSignatureAlgorithm:c.vers>=VersionTLS12,));
        }
    else if (data[0] == typeCertificateStatus) 
        m = @new<certificateStatusMsg>();
    else if (data[0] == typeServerKeyExchange) 
        m = @new<serverKeyExchangeMsg>();
    else if (data[0] == typeServerHelloDone) 
        m = @new<serverHelloDoneMsg>();
    else if (data[0] == typeClientKeyExchange) 
        m = @new<clientKeyExchangeMsg>();
    else if (data[0] == typeCertificateVerify) 
        m = addr(new certificateVerifyMsg(hasSignatureAlgorithm:c.vers>=VersionTLS12,));
    else if (data[0] == typeFinished) 
        m = @new<finishedMsg>();
    else if (data[0] == typeEncryptedExtensions) 
        m = @new<encryptedExtensionsMsg>();
    else if (data[0] == typeEndOfEarlyData) 
        m = @new<endOfEarlyDataMsg>();
    else if (data[0] == typeKeyUpdate) 
        m = @new<keyUpdateMsg>();
    else 
        return (null, error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!);
    // The handshake message unmarshalers
    // expect to be able to keep references to data,
    // so pass in a fresh copy that won't be overwritten.
    data = append((slice<byte>)null, data);

    if (!m.unmarshal(data)) {
        return (null, error.As(c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)))!);
    }
    return (m, error.As(null!)!);

}

private static var errShutdown = errors.New("tls: protocol is shutdown");

// Write writes data to the connection.
//
// As Write calls Handshake, in order to prevent indefinite blocking a deadline
// must be set for both Read and Write before Write is called when the handshake
// has not yet completed. See SetDeadline, SetReadDeadline, and
// SetWriteDeadline.
private static (nint, error) Write(this ptr<Conn> _addr_c, slice<byte> b) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;
 
    // interlock with Close below
    while (true) {
        var x = atomic.LoadInt32(_addr_c.activeCall);
        if (x & 1 != 0) {
            return (0, error.As(net.ErrClosed)!);
        }
        if (atomic.CompareAndSwapInt32(_addr_c.activeCall, x, x + 2)) {
            break;
        }
    }
    defer(atomic.AddInt32(_addr_c.activeCall, -2));

    {
        var err__prev1 = err;

        var err = c.Handshake();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }


    c.@out.Lock();
    defer(c.@out.Unlock());

    {
        var err__prev1 = err;

        err = c.@out.err;

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }


    if (!c.handshakeComplete()) {
        return (0, error.As(alertInternalError)!);
    }
    if (c.closeNotifySent) {
        return (0, error.As(errShutdown)!);
    }
    nint m = default;
    if (len(b) > 1 && c.vers == VersionTLS10) {
        {
            cipher.BlockMode (_, ok) = c.@out.cipher._<cipher.BlockMode>();

            if (ok) {
                var (n, err) = c.writeRecordLocked(recordTypeApplicationData, b[..(int)1]);
                if (err != null) {
                    return (n, error.As(c.@out.setErrorLocked(err))!);
                }
                (m, b) = (1, b[(int)1..]);
            }

        }

    }
    (n, err) = c.writeRecordLocked(recordTypeApplicationData, b);
    return (n + m, error.As(c.@out.setErrorLocked(err))!);

});

// handleRenegotiation processes a HelloRequest handshake message.
private static error handleRenegotiation(this ptr<Conn> _addr_c) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    if (c.vers == VersionTLS13) {
        return error.As(errors.New("tls: internal error: unexpected renegotiation"))!;
    }
    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    ptr<helloRequestMsg> (helloReq, ok) = msg._<ptr<helloRequestMsg>>();
    if (!ok) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(unexpectedMessageError(helloReq, msg))!;
    }
    if (!c.isClient) {
        return error.As(c.sendAlert(alertNoRenegotiation))!;
    }

    if (c.config.Renegotiation == RenegotiateNever) 
        return error.As(c.sendAlert(alertNoRenegotiation))!;
    else if (c.config.Renegotiation == RenegotiateOnceAsClient) 
        if (c.handshakes > 1) {
            return error.As(c.sendAlert(alertNoRenegotiation))!;
        }
    else if (c.config.Renegotiation == RenegotiateFreelyAsClient)     else 
        c.sendAlert(alertInternalError);
        return error.As(errors.New("tls: unknown Renegotiation value"))!;
        c.handshakeMutex.Lock();
    defer(c.handshakeMutex.Unlock());

    atomic.StoreUint32(_addr_c.handshakeStatus, 0);
    c.handshakeErr = c.clientHandshake(context.Background());

    if (c.handshakeErr == null) {
        c.handshakes++;
    }
    return error.As(c.handshakeErr)!;

});

// handlePostHandshakeMessage processes a handshake message arrived after the
// handshake is complete. Up to TLS 1.2, it indicates the start of a renegotiation.
private static error handlePostHandshakeMessage(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    if (c.vers != VersionTLS13) {
        return error.As(c.handleRenegotiation())!;
    }
    var (msg, err) = c.readHandshake();
    if (err != null) {
        return error.As(err)!;
    }
    c.retryCount++;
    if (c.retryCount > maxUselessRecords) {
        c.sendAlert(alertUnexpectedMessage);
        return error.As(c.@in.setErrorLocked(errors.New("tls: too many non-advancing records")))!;
    }
    switch (msg.type()) {
        case ptr<newSessionTicketMsgTLS13> msg:
            return error.As(c.handleNewSessionTicket(msg))!;
            break;
        case ptr<keyUpdateMsg> msg:
            return error.As(c.handleKeyUpdate(msg))!;
            break;
        default:
        {
            var msg = msg.type();
            c.sendAlert(alertUnexpectedMessage);
            return error.As(fmt.Errorf("tls: received unexpected handshake message of type %T", msg))!;
            break;
        }
    }

}

private static error handleKeyUpdate(this ptr<Conn> _addr_c, ptr<keyUpdateMsg> _addr_keyUpdate) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;
    ref keyUpdateMsg keyUpdate = ref _addr_keyUpdate.val;

    var cipherSuite = cipherSuiteTLS13ByID(c.cipherSuite);
    if (cipherSuite == null) {
        return error.As(c.@in.setErrorLocked(c.sendAlert(alertInternalError)))!;
    }
    var newSecret = cipherSuite.nextTrafficSecret(c.@in.trafficSecret);
    c.@in.setTrafficSecret(cipherSuite, newSecret);

    if (keyUpdate.updateRequested) {
        c.@out.Lock();
        defer(c.@out.Unlock());

        ptr<keyUpdateMsg> msg = addr(new keyUpdateMsg());
        var (_, err) = c.writeRecordLocked(recordTypeHandshake, msg.marshal());
        if (err != null) { 
            // Surface the error at the next write.
            c.@out.setErrorLocked(err);
            return error.As(null!)!;

        }
        newSecret = cipherSuite.nextTrafficSecret(c.@out.trafficSecret);
        c.@out.setTrafficSecret(cipherSuite, newSecret);

    }
    return error.As(null!)!;

});

// Read reads data from the connection.
//
// As Read calls Handshake, in order to prevent indefinite blocking a deadline
// must be set for both Read and Write before Read is called when the handshake
// has not yet completed. See SetDeadline, SetReadDeadline, and
// SetWriteDeadline.
private static (nint, error) Read(this ptr<Conn> _addr_c, slice<byte> b) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    {
        var err__prev1 = err;

        var err = c.Handshake();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    if (len(b) == 0) { 
        // Put this after Handshake, in case people were calling
        // Read(nil) for the side effect of the Handshake.
        return (0, error.As(null!)!);

    }
    c.@in.Lock();
    defer(c.@in.Unlock());

    while (c.input.Len() == 0) {
        {
            var err__prev1 = err;

            err = c.readRecord();

            if (err != null) {
                return (0, error.As(err)!);
            }

            err = err__prev1;

        }

        while (c.hand.Len() > 0) {
            {
                var err__prev1 = err;

                err = c.handlePostHandshakeMessage();

                if (err != null) {
                    return (0, error.As(err)!);
                }

                err = err__prev1;

            }

        }

    }

    var (n, _) = c.input.Read(b); 

    // If a close-notify alert is waiting, read it so that we can return (n,
    // EOF) instead of (n, nil), to signal to the HTTP response reading
    // goroutine that the connection is now closed. This eliminates a race
    // where the HTTP response reading goroutine would otherwise not observe
    // the EOF until its next read, by which time a client goroutine might
    // have already tried to reuse the HTTP connection for a new request.
    // See https://golang.org/cl/76400046 and https://golang.org/issue/3514
    if (n != 0 && c.input.Len() == 0 && c.rawInput.Len() > 0 && recordType(c.rawInput.Bytes()[0]) == recordTypeAlert) {
        {
            var err__prev2 = err;

            err = c.readRecord();

            if (err != null) {
                return (n, error.As(err)!); // will be io.EOF on closeNotify
            }

            err = err__prev2;

        }

    }
    return (n, error.As(null!)!);

});

// Close closes the connection.
private static error Close(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;
 
    // Interlock with Conn.Write above.
    int x = default;
    while (true) {
        x = atomic.LoadInt32(_addr_c.activeCall);
        if (x & 1 != 0) {
            return error.As(net.ErrClosed)!;
        }
        if (atomic.CompareAndSwapInt32(_addr_c.activeCall, x, x | 1)) {
            break;
        }
    }
    if (x != 0) { 
        // io.Writer and io.Closer should not be used concurrently.
        // If Close is called while a Write is currently in-flight,
        // interpret that as a sign that this Close is really just
        // being used to break the Write and/or clean up resources and
        // avoid sending the alertCloseNotify, which may block
        // waiting on handshakeMutex or the c.out mutex.
        return error.As(c.conn.Close())!;

    }
    error alertErr = default!;
    if (c.handshakeComplete()) {
        {
            var err__prev2 = err;

            var err = c.closeNotify();

            if (err != null) {
                alertErr = error.As(fmt.Errorf("tls: failed to send closeNotify alert (but connection was closed anyway): %w", err))!;
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        err = c.conn.Close();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    return error.As(alertErr)!;

}

private static var errEarlyCloseWrite = errors.New("tls: CloseWrite called before handshake complete");

// CloseWrite shuts down the writing side of the connection. It should only be
// called once the handshake has completed and does not call CloseWrite on the
// underlying connection. Most callers should just use Close.
private static error CloseWrite(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    if (!c.handshakeComplete()) {
        return error.As(errEarlyCloseWrite)!;
    }
    return error.As(c.closeNotify())!;

}

private static error closeNotify(this ptr<Conn> _addr_c) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    c.@out.Lock();
    defer(c.@out.Unlock());

    if (!c.closeNotifySent) { 
        // Set a Write Deadline to prevent possibly blocking forever.
        c.SetWriteDeadline(time.Now().Add(time.Second * 5));
        c.closeNotifyErr = c.sendAlertLocked(alertCloseNotify);
        c.closeNotifySent = true; 
        // Any subsequent writes will fail.
        c.SetWriteDeadline(time.Now());

    }
    return error.As(c.closeNotifyErr)!;

});

// Handshake runs the client or server handshake
// protocol if it has not yet been run.
//
// Most uses of this package need not call Handshake explicitly: the
// first Read or Write will call it automatically.
//
// For control over canceling or setting a timeout on a handshake, use
// HandshakeContext or the Dialer's DialContext method instead.
private static error Handshake(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return error.As(c.HandshakeContext(context.Background()))!;
}

// HandshakeContext runs the client or server handshake
// protocol if it has not yet been run.
//
// The provided Context must be non-nil. If the context is canceled before
// the handshake is complete, the handshake is interrupted and an error is returned.
// Once the handshake has completed, cancellation of the context will not affect the
// connection.
//
// Most uses of this package need not call HandshakeContext explicitly: the
// first Read or Write will call it automatically.
private static error HandshakeContext(this ptr<Conn> _addr_c, context.Context ctx) {
    ref Conn c = ref _addr_c.val;
 
    // Delegate to unexported method for named return
    // without confusing documented signature.
    return error.As(c.handshakeContext(ctx))!;

}

private static error handshakeContext(this ptr<Conn> _addr_c, context.Context ctx) => func((defer, _, _) => {
    error ret = default!;
    ref Conn c = ref _addr_c.val;

    var (handshakeCtx, cancel) = context.WithCancel(ctx); 
    // Note: defer this before starting the "interrupter" goroutine
    // so that we can tell the difference between the input being canceled and
    // this cancellation. In the former case, we need to close the connection.
    defer(cancel()); 

    // Start the "interrupter" goroutine, if this context might be canceled.
    // (The background context cannot).
    //
    // The interrupter goroutine waits for the input context to be done and
    // closes the connection if this happens before the function returns.
    if (ctx.Done() != null) {
        var done = make_channel<object>();
        var interruptRes = make_channel<error>(1);
        defer(() => {
            close(done);
            {
                var ctxErr = interruptRes.Receive();

                if (ctxErr != null) { 
                    // Return context error to user.
                    ret = ctxErr;

                }

            }

        }());
        go_(() => () => {
            _ = c.conn.Close();
            interruptRes.Send(handshakeCtx.Err());
            interruptRes.Send(null);
        }());

    }
    c.handshakeMutex.Lock();
    defer(c.handshakeMutex.Unlock());

    {
        var err = c.handshakeErr;

        if (err != null) {
            return error.As(err)!;
        }
    }

    if (c.handshakeComplete()) {
        return error.As(null!)!;
    }
    c.@in.Lock();
    defer(c.@in.Unlock());

    c.handshakeErr = c.handshakeFn(handshakeCtx);
    if (c.handshakeErr == null) {
        c.handshakes++;
    }
    else
 { 
        // If an error occurred during the handshake try to flush the
        // alert that might be left in the buffer.
        c.flush();

    }
    if (c.handshakeErr == null && !c.handshakeComplete()) {
        c.handshakeErr = errors.New("tls: internal error: handshake should have had a result");
    }
    return error.As(c.handshakeErr)!;

});

// ConnectionState returns basic TLS details about the connection.
private static ConnectionState ConnectionState(this ptr<Conn> _addr_c) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    c.handshakeMutex.Lock();
    defer(c.handshakeMutex.Unlock());
    return c.connectionStateLocked();
});

private static ConnectionState connectionStateLocked(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    ConnectionState state = default;
    state.HandshakeComplete = c.handshakeComplete();
    state.Version = c.vers;
    state.NegotiatedProtocol = c.clientProtocol;
    state.DidResume = c.didResume;
    state.NegotiatedProtocolIsMutual = true;
    state.ServerName = c.serverName;
    state.CipherSuite = c.cipherSuite;
    state.PeerCertificates = c.peerCertificates;
    state.VerifiedChains = c.verifiedChains;
    state.SignedCertificateTimestamps = c.scts;
    state.OCSPResponse = c.ocspResponse;
    if (!c.didResume && c.vers != VersionTLS13) {
        if (c.clientFinishedIsFirst) {
            state.TLSUnique = c.clientFinished[..];
        }
        else
 {
            state.TLSUnique = c.serverFinished[..];
        }
    }
    if (c.config.Renegotiation != RenegotiateNever) {
        state.ekm = noExportedKeyingMaterial;
    }
    else
 {
        state.ekm = c.ekm;
    }
    return state;

}

// OCSPResponse returns the stapled OCSP response from the TLS server, if
// any. (Only valid for client connections.)
private static slice<byte> OCSPResponse(this ptr<Conn> _addr_c) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    c.handshakeMutex.Lock();
    defer(c.handshakeMutex.Unlock());

    return c.ocspResponse;
});

// VerifyHostname checks that the peer certificate chain is valid for
// connecting to host. If so, it returns nil; if not, it returns an error
// describing the problem.
private static error VerifyHostname(this ptr<Conn> _addr_c, @string host) => func((defer, _, _) => {
    ref Conn c = ref _addr_c.val;

    c.handshakeMutex.Lock();
    defer(c.handshakeMutex.Unlock());
    if (!c.isClient) {
        return error.As(errors.New("tls: VerifyHostname called on TLS server connection"))!;
    }
    if (!c.handshakeComplete()) {
        return error.As(errors.New("tls: handshake has not yet been performed"))!;
    }
    if (len(c.verifiedChains) == 0) {
        return error.As(errors.New("tls: handshake did not verify certificate chain"))!;
    }
    return error.As(c.peerCertificates[0].VerifyHostname(host))!;

});

private static bool handshakeComplete(this ptr<Conn> _addr_c) {
    ref Conn c = ref _addr_c.val;

    return atomic.LoadUint32(_addr_c.handshakeStatus) == 1;
}

} // end tls_package
