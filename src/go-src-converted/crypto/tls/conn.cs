// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TLS low level connection and record layer

// package tls -- go2cs converted at 2020 August 29 08:31:11 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\conn.go
using bytes = go.bytes_package;
using cipher = go.crypto.cipher_package;
using subtle = go.crypto.subtle_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // A Conn represents a secured connection.
        // It implements the net.Conn interface.
        public partial struct Conn
        {
            public net.Conn conn;
            public bool isClient; // constant after handshake; protected by handshakeMutex
            public sync.Mutex handshakeMutex; // handshakeMutex < in.Mutex, out.Mutex, errMutex
// handshakeCond, if not nil, indicates that a goroutine is committed
// to running the handshake for this Conn. Other goroutines that need
// to wait for the handshake can wait on this, under handshakeMutex.
            public ptr<sync.Cond> handshakeCond;
            public error handshakeErr; // error resulting from handshake
            public ushort vers; // TLS version
            public bool haveVers; // version has been negotiated
            public ptr<Config> config; // configuration passed to constructor
// handshakeComplete is true if the connection is currently transferring
// application data (i.e. is not currently processing a handshake).
            public bool handshakeComplete; // handshakes counts the number of handshakes performed on the
// connection so far. If renegotiation is disabled then this is either
// zero or one.
            public long handshakes;
            public bool didResume; // whether this connection was a session resumption
            public ushort cipherSuite;
            public slice<byte> ocspResponse; // stapled OCSP response
            public slice<slice<byte>> scts; // signed certificate timestamps from server
            public slice<ref x509.Certificate> peerCertificates; // verifiedChains contains the certificate chains that we built, as
// opposed to the ones presented by the server.
            public slice<slice<ref x509.Certificate>> verifiedChains; // serverName contains the server name indicated by the client, if any.
            public @string serverName; // secureRenegotiation is true if the server echoed the secure
// renegotiation extension. (This is meaningless as a server because
// renegotiation is not supported in that case.)
            public bool secureRenegotiation; // clientFinishedIsFirst is true if the client sent the first Finished
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
            public array<byte> serverFinished;
            public @string clientProtocol;
            public bool clientProtocolFallback; // input/output
            public halfConn @in; // in.Mutex < out.Mutex
            public halfConn @out; // in.Mutex < out.Mutex
            public ptr<block> rawInput; // raw input, right off the wire
            public ptr<block> input; // application data waiting to be read
            public bytes.Buffer hand; // handshake data waiting to be read
            public bool buffering; // whether records are buffered in sendBuf
            public slice<byte> sendBuf; // a buffer of records waiting to be sent

// bytesSent counts the bytes of application data sent.
// packetsSent counts packets.
            public long bytesSent;
            public long packetsSent; // warnCount counts the number of consecutive warning alerts received
// by Conn.readRecord. Protected by in.Mutex.
            public long warnCount; // activeCall is an atomic int32; the low bit is whether Close has
// been called. the rest of the bits are the number of goroutines
// in Conn.Write.
            public int activeCall;
            public array<byte> tmp;
        }

        // Access to net.Conn methods.
        // Cannot just embed net.Conn because that would
        // export the struct field too.

        // LocalAddr returns the local network address.
        private static net.Addr LocalAddr(this ref Conn c)
        {
            return c.conn.LocalAddr();
        }

        // RemoteAddr returns the remote network address.
        private static net.Addr RemoteAddr(this ref Conn c)
        {
            return c.conn.RemoteAddr();
        }

        // SetDeadline sets the read and write deadlines associated with the connection.
        // A zero value for t means Read and Write will not time out.
        // After a Write has timed out, the TLS state is corrupt and all future writes will return the same error.
        private static error SetDeadline(this ref Conn c, time.Time t)
        {
            return error.As(c.conn.SetDeadline(t));
        }

        // SetReadDeadline sets the read deadline on the underlying connection.
        // A zero value for t means Read will not time out.
        private static error SetReadDeadline(this ref Conn c, time.Time t)
        {
            return error.As(c.conn.SetReadDeadline(t));
        }

        // SetWriteDeadline sets the write deadline on the underlying connection.
        // A zero value for t means Write will not time out.
        // After a Write has timed out, the TLS state is corrupt and all future writes will return the same error.
        private static error SetWriteDeadline(this ref Conn c, time.Time t)
        {
            return error.As(c.conn.SetWriteDeadline(t));
        }

        // A halfConn represents one direction of the record layer
        // connection, either sending or receiving.
        private partial struct halfConn
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public error err; // first permanent error
            public ushort version; // protocol version
            public macFunction mac;
            public array<byte> seq; // 64-bit sequence number
            public ptr<block> bfree; // list of free blocks
            public array<byte> additionalData; // to avoid allocs; interface method args escape

            public macFunction nextMac; // next MAC algorithm

// used to save allocating a new buffer for each MAC.
            public slice<byte> inDigestBuf;
            public slice<byte> outDigestBuf;
        }

        private static error setErrorLocked(this ref halfConn hc, error err)
        {
            hc.err = err;
            return error.As(err);
        }

        // prepareCipherSpec sets the encryption and MAC states
        // that a subsequent changeCipherSpec will use.
        private static void prepareCipherSpec(this ref halfConn hc, ushort version, object cipher, macFunction mac)
        {
            hc.version = version;
            hc.nextCipher = cipher;
            hc.nextMac = mac;
        }

        // changeCipherSpec changes the encryption and MAC states
        // to the ones previously passed to prepareCipherSpec.
        private static error changeCipherSpec(this ref halfConn hc)
        {
            if (hc.nextCipher == null)
            {
                return error.As(alertInternalError);
            }
            hc.cipher = hc.nextCipher;
            hc.mac = hc.nextMac;
            hc.nextCipher = null;
            hc.nextMac = null;
            foreach (var (i) in hc.seq)
            {
                hc.seq[i] = 0L;
            }
            return error.As(null);
        }

        // incSeq increments the sequence number.
        private static void incSeq(this ref halfConn _hc) => func(_hc, (ref halfConn hc, Defer _, Panic panic, Recover __) =>
        {
            for (long i = 7L; i >= 0L; i--)
            {
                hc.seq[i]++;
                if (hc.seq[i] != 0L)
                {
                    return;
                }
            } 

            // Not allowed to let sequence number wrap.
            // Instead, must renegotiate before it does.
            // Not likely enough to bother.
 

            // Not allowed to let sequence number wrap.
            // Instead, must renegotiate before it does.
            // Not likely enough to bother.
            panic("TLS: sequence number wraparound");
        });

        // extractPadding returns, in constant time, the length of the padding to remove
        // from the end of payload. It also returns a byte which is equal to 255 if the
        // padding was valid and 0 otherwise. See RFC 2246, section 6.2.3.2
        private static (long, byte) extractPadding(slice<byte> payload)
        {
            if (len(payload) < 1L)
            {
                return (0L, 0L);
            }
            var paddingLen = payload[len(payload) - 1L];
            var t = uint(len(payload) - 1L) - uint(paddingLen); 
            // if len(payload) >= (paddingLen - 1) then the MSB of t is zero
            good = byte(int32(~t) >> (int)(31L)); 

            // The maximum possible padding length plus the actual length field
            long toCheck = 256L; 
            // The length of the padded data is public, so we can use an if here
            if (toCheck > len(payload))
            {
                toCheck = len(payload);
            }
            for (long i = 0L; i < toCheck; i++)
            {
                t = uint(paddingLen) - uint(i); 
                // if i <= paddingLen then the MSB of t is zero
                var mask = byte(int32(~t) >> (int)(31L));
                var b = payload[len(payload) - 1L - i];
                good &= mask & paddingLen ^ mask & b;
            } 

            // We AND together the bits of good and replicate the result across
            // all the bits.
 

            // We AND together the bits of good and replicate the result across
            // all the bits.
            good &= good << (int)(4L);
            good &= good << (int)(2L);
            good &= good << (int)(1L);
            good = uint8(int8(good) >> (int)(7L));

            toRemove = int(paddingLen) + 1L;
            return;
        }

        // extractPaddingSSL30 is a replacement for extractPadding in the case that the
        // protocol version is SSLv3. In this version, the contents of the padding
        // are random and cannot be checked.
        private static (long, byte) extractPaddingSSL30(slice<byte> payload)
        {
            if (len(payload) < 1L)
            {
                return (0L, 0L);
            }
            var paddingLen = int(payload[len(payload) - 1L]) + 1L;
            if (paddingLen > len(payload))
            {
                return (0L, 0L);
            }
            return (paddingLen, 255L);
        }

        private static long roundUp(long a, long b)
        {
            return a + (b - a % b) % b;
        }

        // cbcMode is an interface for block ciphers using cipher block chaining.
        private partial interface cbcMode : cipher.BlockMode
        {
            void SetIV(slice<byte> _p0);
        }

        // decrypt checks and strips the mac and decrypts the data in b. Returns a
        // success boolean, the number of bytes to skip from the start of the record in
        // order to get the application payload, and an optional alert value.
        private static (bool, long, alert) decrypt(this ref halfConn _hc, ref block _b) => func(_hc, _b, (ref halfConn hc, ref block b, Defer _, Panic panic, Recover __) =>
        { 
            // pull out payload
            var payload = b.data[recordHeaderLen..];

            long macSize = 0L;
            if (hc.mac != null)
            {
                macSize = hc.mac.Size();
            }
            var paddingGood = byte(255L);
            long paddingLen = 0L;
            long explicitIVLen = 0L; 

            // decrypt
            if (hc.cipher != null)
            {
                switch (hc.cipher.type())
                {
                    case cipher.Stream c:
                        c.XORKeyStream(payload, payload);
                        break;
                    case aead c:
                        explicitIVLen = c.explicitNonceLen();
                        if (len(payload) < explicitIVLen)
                        {
                            return (false, 0L, alertBadRecordMAC);
                        }
                        var nonce = payload[..explicitIVLen];
                        payload = payload[explicitIVLen..];

                        if (len(nonce) == 0L)
                        {
                            nonce = hc.seq[..];
                        }
                        copy(hc.additionalData[..], hc.seq[..]);
                        copy(hc.additionalData[8L..], b.data[..3L]);
                        var n = len(payload) - c.Overhead();
                        hc.additionalData[11L] = byte(n >> (int)(8L));
                        hc.additionalData[12L] = byte(n);
                        error err = default;
                        payload, err = c.Open(payload[..0L], nonce, payload, hc.additionalData[..]);
                        if (err != null)
                        {
                            return (false, 0L, alertBadRecordMAC);
                        }
                        b.resize(recordHeaderLen + explicitIVLen + len(payload));
                        break;
                    case cbcMode c:
                        var blockSize = c.BlockSize();
                        if (hc.version >= VersionTLS11)
                        {
                            explicitIVLen = blockSize;
                        }
                        if (len(payload) % blockSize != 0L || len(payload) < roundUp(explicitIVLen + macSize + 1L, blockSize))
                        {
                            return (false, 0L, alertBadRecordMAC);
                        }
                        if (explicitIVLen > 0L)
                        {
                            c.SetIV(payload[..explicitIVLen]);
                            payload = payload[explicitIVLen..];
                        }
                        c.CryptBlocks(payload, payload);
                        if (hc.version == VersionSSL30)
                        {
                            paddingLen, paddingGood = extractPaddingSSL30(payload);
                        }
                        else
                        {
                            paddingLen, paddingGood = extractPadding(payload); 

                            // To protect against CBC padding oracles like Lucky13, the data
                            // past paddingLen (which is secret) is passed to the MAC
                            // function as extra data, to be fed into the HMAC after
                            // computing the digest. This makes the MAC constant time as
                            // long as the digest computation is constant time and does not
                            // affect the subsequent write.
                        }
                        break;
                    default:
                    {
                        var c = hc.cipher.type();
                        panic("unknown cipher type");
                        break;
                    }
                }
            } 

            // check, strip mac
            if (hc.mac != null)
            {
                if (len(payload) < macSize)
                {
                    return (false, 0L, alertBadRecordMAC);
                } 

                // strip mac off payload, b.data
                n = len(payload) - macSize - paddingLen;
                n = subtle.ConstantTimeSelect(int(uint32(n) >> (int)(31L)), 0L, n); // if n < 0 { n = 0 }
                b.data[3L] = byte(n >> (int)(8L));
                b.data[4L] = byte(n);
                var remoteMAC = payload[n..n + macSize];
                var localMAC = hc.mac.MAC(hc.inDigestBuf, hc.seq[0L..], b.data[..recordHeaderLen], payload[..n], payload[n + macSize..]);

                if (subtle.ConstantTimeCompare(localMAC, remoteMAC) != 1L || paddingGood != 255L)
                {
                    return (false, 0L, alertBadRecordMAC);
                }
                hc.inDigestBuf = localMAC;

                b.resize(recordHeaderLen + explicitIVLen + n);
            }
            hc.incSeq();

            return (true, recordHeaderLen + explicitIVLen, 0L);
        });

        // padToBlockSize calculates the needed padding block, if any, for a payload.
        // On exit, prefix aliases payload and extends to the end of the last full
        // block of payload. finalBlock is a fresh slice which contains the contents of
        // any suffix of payload as well as the needed padding to make finalBlock a
        // full block.
        private static (slice<byte>, slice<byte>) padToBlockSize(slice<byte> payload, long blockSize)
        {
            var overrun = len(payload) % blockSize;
            var paddingLen = blockSize - overrun;
            prefix = payload[..len(payload) - overrun];
            finalBlock = make_slice<byte>(blockSize);
            copy(finalBlock, payload[len(payload) - overrun..]);
            for (var i = overrun; i < blockSize; i++)
            {
                finalBlock[i] = byte(paddingLen - 1L);
            }

            return;
        }

        // encrypt encrypts and macs the data in b.
        private static (bool, alert) encrypt(this ref halfConn _hc, ref block _b, long explicitIVLen) => func(_hc, _b, (ref halfConn hc, ref block b, Defer _, Panic panic, Recover __) =>
        { 
            // mac
            if (hc.mac != null)
            {
                var mac = hc.mac.MAC(hc.outDigestBuf, hc.seq[0L..], b.data[..recordHeaderLen], b.data[recordHeaderLen + explicitIVLen..], null);

                var n = len(b.data);
                b.resize(n + len(mac));
                copy(b.data[n..], mac);
                hc.outDigestBuf = mac;
            }
            var payload = b.data[recordHeaderLen..]; 

            // encrypt
            if (hc.cipher != null)
            {
                switch (hc.cipher.type())
                {
                    case cipher.Stream c:
                        c.XORKeyStream(payload, payload);
                        break;
                    case aead c:
                        var payloadLen = len(b.data) - recordHeaderLen - explicitIVLen;
                        b.resize(len(b.data) + c.Overhead());
                        var nonce = b.data[recordHeaderLen..recordHeaderLen + explicitIVLen];
                        if (len(nonce) == 0L)
                        {
                            nonce = hc.seq[..];
                        }
                        payload = b.data[recordHeaderLen + explicitIVLen..];
                        payload = payload[..payloadLen];

                        copy(hc.additionalData[..], hc.seq[..]);
                        copy(hc.additionalData[8L..], b.data[..3L]);
                        hc.additionalData[11L] = byte(payloadLen >> (int)(8L));
                        hc.additionalData[12L] = byte(payloadLen);

                        c.Seal(payload[..0L], nonce, payload, hc.additionalData[..]);
                        break;
                    case cbcMode c:
                        var blockSize = c.BlockSize();
                        if (explicitIVLen > 0L)
                        {
                            c.SetIV(payload[..explicitIVLen]);
                            payload = payload[explicitIVLen..];
                        }
                        var (prefix, finalBlock) = padToBlockSize(payload, blockSize);
                        b.resize(recordHeaderLen + explicitIVLen + len(prefix) + len(finalBlock));
                        c.CryptBlocks(b.data[recordHeaderLen + explicitIVLen..], prefix);
                        c.CryptBlocks(b.data[recordHeaderLen + explicitIVLen + len(prefix)..], finalBlock);
                        break;
                    default:
                    {
                        var c = hc.cipher.type();
                        panic("unknown cipher type");
                        break;
                    }
                }
            } 

            // update length to include MAC and any block padding needed.
            n = len(b.data) - recordHeaderLen;
            b.data[3L] = byte(n >> (int)(8L));
            b.data[4L] = byte(n);
            hc.incSeq();

            return (true, 0L);
        });

        // A block is a simple data buffer.
        private partial struct block
        {
            public slice<byte> data;
            public long off; // index for Read
            public ptr<block> link;
        }

        // resize resizes block to be n bytes, growing if necessary.
        private static void resize(this ref block b, long n)
        {
            if (n > cap(b.data))
            {
                b.reserve(n);
            }
            b.data = b.data[0L..n];
        }

        // reserve makes sure that block contains a capacity of at least n bytes.
        private static void reserve(this ref block b, long n)
        {
            if (cap(b.data) >= n)
            {
                return;
            }
            var m = cap(b.data);
            if (m == 0L)
            {
                m = 1024L;
            }
            while (m < n)
            {
                m *= 2L;
            }

            var data = make_slice<byte>(len(b.data), m);
            copy(data, b.data);
            b.data = data;
        }

        // readFromUntil reads from r into b until b contains at least n bytes
        // or else returns an error.
        private static error readFromUntil(this ref block b, io.Reader r, long n)
        { 
            // quick case
            if (len(b.data) >= n)
            {
                return error.As(null);
            } 

            // read until have enough.
            b.reserve(n);
            while (true)
            {
                var (m, err) = r.Read(b.data[len(b.data)..cap(b.data)]);
                b.data = b.data[0L..len(b.data) + m];
                if (len(b.data) >= n)
                { 
                    // TODO(bradfitz,agl): slightly suspicious
                    // that we're throwing away r.Read's err here.
                    break;
                }
                if (err != null)
                {
                    return error.As(err);
                }
            }

            return error.As(null);
        }

        private static (long, error) Read(this ref block b, slice<byte> p)
        {
            n = copy(p, b.data[b.off..]);
            b.off += n;
            return;
        }

        // newBlock allocates a new block, from hc's free list if possible.
        private static ref block newBlock(this ref halfConn hc)
        {
            var b = hc.bfree;
            if (b == null)
            {
                return @new<block>();
            }
            hc.bfree = b.link;
            b.link = null;
            b.resize(0L);
            return b;
        }

        // freeBlock returns a block to hc's free list.
        // The protocol is such that each side only has a block or two on
        // its free list at a time, so there's no need to worry about
        // trimming the list, etc.
        private static void freeBlock(this ref halfConn hc, ref block b)
        {
            b.link = hc.bfree;
            hc.bfree = b;
        }

        // splitBlock splits a block after the first n bytes,
        // returning a block with those n bytes and a
        // block with the remainder.  the latter may be nil.
        private static (ref block, ref block) splitBlock(this ref halfConn hc, ref block b, long n)
        {
            if (len(b.data) <= n)
            {
                return (b, null);
            }
            var bb = hc.newBlock();
            bb.resize(len(b.data) - n);
            copy(bb.data, b.data[n..]);
            b.data = b.data[0L..n];
            return (b, bb);
        }

        // RecordHeaderError results when a TLS record header is invalid.
        public partial struct RecordHeaderError
        {
            public @string Msg; // RecordHeader contains the five bytes of TLS record header that
// triggered the error.
            public array<byte> RecordHeader;
        }

        public static @string Error(this RecordHeaderError e)
        {
            return "tls: " + e.Msg;
        }

        private static RecordHeaderError newRecordHeaderError(this ref Conn c, @string msg)
        {
            err.Msg = msg;
            copy(err.RecordHeader[..], c.rawInput.data);
            return err;
        }

        // readRecord reads the next TLS record from the connection
        // and updates the record layer state.
        // c.in.Mutex <= L; c.input == nil.
        private static error readRecord(this ref Conn c, recordType want)
        { 
            // Caller must be in sync with connection:
            // handshake data if handshake not yet completed,
            // else application data.

            if (want == recordTypeHandshake || want == recordTypeChangeCipherSpec) 
                if (c.handshakeComplete)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(c.@in.setErrorLocked(errors.New("tls: handshake or ChangeCipherSpec requested while not in handshake")));
                }
            else if (want == recordTypeApplicationData) 
                if (!c.handshakeComplete)
                {
                    c.sendAlert(alertInternalError);
                    return error.As(c.@in.setErrorLocked(errors.New("tls: application data record requested while in handshake")));
                }
            else 
                c.sendAlert(alertInternalError);
                return error.As(c.@in.setErrorLocked(errors.New("tls: unknown record type requested")));
            Again:
            if (c.rawInput == null)
            {
                c.rawInput = c.@in.newBlock();
            }
            var b = c.rawInput; 

            // Read header, payload.
            {
                var err__prev1 = err;

                var err = b.readFromUntil(c.conn, recordHeaderLen);

                if (err != null)
                { 
                    // RFC suggests that EOF without an alertCloseNotify is
                    // an error, but popular web sites seem to do this,
                    // so we can't make it an error.
                    // if err == io.EOF {
                    //     err = io.ErrUnexpectedEOF
                    // }
                    {
                        net.Error e__prev2 = e;

                        net.Error (e, ok) = err._<net.Error>();

                        if (!ok || !e.Temporary())
                        {
                            c.@in.setErrorLocked(err);
                        }

                        e = e__prev2;

                    }
                    return error.As(err);
                }

                err = err__prev1;

            }
            var typ = recordType(b.data[0L]); 

            // No valid TLS record has a type of 0x80, however SSLv2 handshakes
            // start with a uint16 length where the MSB is set and the first record
            // is always < 256 bytes long. Therefore typ == 0x80 strongly suggests
            // an SSLv2 client.
            if (want == recordTypeHandshake && typ == 0x80UL)
            {
                c.sendAlert(alertProtocolVersion);
                return error.As(c.@in.setErrorLocked(c.newRecordHeaderError("unsupported SSLv2 handshake received")));
            }
            var vers = uint16(b.data[1L]) << (int)(8L) | uint16(b.data[2L]);
            var n = int(b.data[3L]) << (int)(8L) | int(b.data[4L]);
            if (c.haveVers && vers != c.vers)
            {
                c.sendAlert(alertProtocolVersion);
                var msg = fmt.Sprintf("received record with version %x when expecting version %x", vers, c.vers);
                return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(msg)));
            }
            if (n > maxCiphertext)
            {
                c.sendAlert(alertRecordOverflow);
                msg = fmt.Sprintf("oversized record received with length %d", n);
                return error.As(c.@in.setErrorLocked(c.newRecordHeaderError(msg)));
            }
            if (!c.haveVers)
            { 
                // First message, be extra suspicious: this might not be a TLS
                // client. Bail out before reading a full 'body', if possible.
                // The current max version is 3.3 so if the version is >= 16.0,
                // it's probably not real.
                if ((typ != recordTypeAlert && typ != want) || vers >= 0x1000UL)
                {
                    c.sendAlert(alertUnexpectedMessage);
                    return error.As(c.@in.setErrorLocked(c.newRecordHeaderError("first record does not look like a TLS handshake")));
                }
            }
            {
                var err__prev1 = err;

                err = b.readFromUntil(c.conn, recordHeaderLen + n);

                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }
                    {
                        net.Error e__prev2 = e;

                        (e, ok) = err._<net.Error>();

                        if (!ok || !e.Temporary())
                        {
                            c.@in.setErrorLocked(err);
                        }

                        e = e__prev2;

                    }
                    return error.As(err);
                } 

                // Process message.

                err = err__prev1;

            } 

            // Process message.
            b, c.rawInput = c.@in.splitBlock(b, recordHeaderLen + n);
            var (ok, off, alertValue) = c.@in.decrypt(b);
            if (!ok)
            {
                c.@in.freeBlock(b);
                return error.As(c.@in.setErrorLocked(c.sendAlert(alertValue)));
            }
            b.off = off;
            var data = b.data[b.off..];
            if (len(data) > maxPlaintext)
            {
                err = c.sendAlert(alertRecordOverflow);
                c.@in.freeBlock(b);
                return error.As(c.@in.setErrorLocked(err));
            }
            if (typ != recordTypeAlert && len(data) > 0L)
            { 
                // this is a valid non-alert message: reset the count of alerts
                c.warnCount = 0L;
            }

            if (typ == recordTypeAlert) 
                if (len(data) != 2L)
                {
                    c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                    break;
                }
                if (alert(data[1L]) == alertCloseNotify)
                {
                    c.@in.setErrorLocked(io.EOF);
                    break;
                }

                if (data[0L] == alertLevelWarning) 
                    // drop on the floor
                    c.@in.freeBlock(b);

                    c.warnCount++;
                    if (c.warnCount > maxWarnAlertCount)
                    {
                        c.sendAlert(alertUnexpectedMessage);
                        return error.As(c.@in.setErrorLocked(errors.New("tls: too many warn alerts")));
                    }
                    goto Again;
                else if (data[0L] == alertLevelError) 
                    c.@in.setErrorLocked(ref new net.OpError(Op:"remote error",Err:alert(data[1])));
                else 
                    c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                            else if (typ == recordTypeChangeCipherSpec) 
                if (typ != want || len(data) != 1L || data[0L] != 1L)
                {
                    c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                    break;
                } 
                // Handshake messages are not allowed to fragment across the CCS
                if (c.hand.Len() > 0L)
                {
                    c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                    break;
                }
                err = c.@in.changeCipherSpec();
                if (err != null)
                {
                    c.@in.setErrorLocked(c.sendAlert(err._<alert>()));
                }
            else if (typ == recordTypeApplicationData) 
                if (typ != want)
                {
                    c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                    break;
                }
                c.input = b;
                b = null;
            else if (typ == recordTypeHandshake) 
                // TODO(rsc): Should at least pick off connection close.
                if (typ != want && !(c.isClient && c.config.Renegotiation != RenegotiateNever))
                {
                    return error.As(c.@in.setErrorLocked(c.sendAlert(alertNoRenegotiation)));
                }
                c.hand.Write(data);
            else 
                c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage));
                        if (b != null)
            {
                c.@in.freeBlock(b);
            }
            return error.As(c.@in.err);
        }

        // sendAlert sends a TLS alert message.
        // c.out.Mutex <= L.
        private static error sendAlertLocked(this ref Conn c, alert err)
        {

            if (err == alertNoRenegotiation || err == alertCloseNotify) 
                c.tmp[0L] = alertLevelWarning;
            else 
                c.tmp[0L] = alertLevelError;
                        c.tmp[1L] = byte(err);

            var (_, writeErr) = c.writeRecordLocked(recordTypeAlert, c.tmp[0L..2L]);
            if (err == alertCloseNotify)
            { 
                // closeNotify is a special case in that it isn't an error.
                return error.As(writeErr);
            }
            return error.As(c.@out.setErrorLocked(ref new net.OpError(Op:"local error",Err:err)));
        }

        // sendAlert sends a TLS alert message.
        // L < c.out.Mutex.
        private static error sendAlert(this ref Conn _c, alert err) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.@out.Lock();
            defer(c.@out.Unlock());
            return error.As(c.sendAlertLocked(err));
        });

 
        // tcpMSSEstimate is a conservative estimate of the TCP maximum segment
        // size (MSS). A constant is used, rather than querying the kernel for
        // the actual MSS, to avoid complexity. The value here is the IPv6
        // minimum MTU (1280 bytes) minus the overhead of an IPv6 header (40
        // bytes) and a TCP header with timestamps (32 bytes).
        private static readonly long tcpMSSEstimate = 1208L; 

        // recordSizeBoostThreshold is the number of bytes of application data
        // sent after which the TLS record size will be increased to the
        // maximum.
        private static readonly long recordSizeBoostThreshold = 128L * 1024L;

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
        //
        // c.out.Mutex <= L.
        private static long maxPayloadSizeForWrite(this ref Conn _c, recordType typ, long explicitIVLen) => func(_c, (ref Conn c, Defer _, Panic panic, Recover __) =>
        {
            if (c.config.DynamicRecordSizingDisabled || typ != recordTypeApplicationData)
            {
                return maxPlaintext;
            }
            if (c.bytesSent >= recordSizeBoostThreshold)
            {
                return maxPlaintext;
            } 

            // Subtract TLS overheads to get the maximum payload size.
            long macSize = 0L;
            if (c.@out.mac != null)
            {
                macSize = c.@out.mac.Size();
            }
            var payloadBytes = tcpMSSEstimate - recordHeaderLen - explicitIVLen;
            if (c.@out.cipher != null)
            {
                switch (c.@out.cipher.type())
                {
                    case cipher.Stream ciph:
                        payloadBytes -= macSize;
                        break;
                    case cipher.AEAD ciph:
                        payloadBytes -= ciph.Overhead();
                        break;
                    case cbcMode ciph:
                        var blockSize = ciph.BlockSize(); 
                        // The payload must fit in a multiple of blockSize, with
                        // room for at least one padding byte.
                        payloadBytes = (payloadBytes & ~(blockSize - 1L)) - 1L; 
                        // The MAC is appended before padding so affects the
                        // payload size directly.
                        payloadBytes -= macSize;
                        break;
                    default:
                    {
                        var ciph = c.@out.cipher.type();
                        panic("unknown cipher type");
                        break;
                    }
                }
            } 

            // Allow packet growth in arithmetic progression up to max.
            var pkt = c.packetsSent;
            c.packetsSent++;
            if (pkt > 1000L)
            {
                return maxPlaintext; // avoid overflow in multiply below
            }
            var n = payloadBytes * int(pkt + 1L);
            if (n > maxPlaintext)
            {
                n = maxPlaintext;
            }
            return n;
        });

        // c.out.Mutex <= L.
        private static (long, error) write(this ref Conn c, slice<byte> data)
        {
            if (c.buffering)
            {
                c.sendBuf = append(c.sendBuf, data);
                return (len(data), null);
            }
            var (n, err) = c.conn.Write(data);
            c.bytesSent += int64(n);
            return (n, err);
        }

        private static (long, error) flush(this ref Conn c)
        {
            if (len(c.sendBuf) == 0L)
            {
                return (0L, null);
            }
            var (n, err) = c.conn.Write(c.sendBuf);
            c.bytesSent += int64(n);
            c.sendBuf = null;
            c.buffering = false;
            return (n, err);
        }

        // writeRecordLocked writes a TLS record with the given type and payload to the
        // connection and updates the record layer state.
        // c.out.Mutex <= L.
        private static (long, error) writeRecordLocked(this ref Conn _c, recordType typ, slice<byte> data) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            var b = c.@out.newBlock();
            defer(c.@out.freeBlock(b));

            long n = default;
            while (len(data) > 0L)
            {
                long explicitIVLen = 0L;
                var explicitIVIsSeq = false;

                cbcMode cbc = default;
                if (c.@out.version >= VersionTLS11)
                {
                    bool ok = default;
                    cbc, ok = c.@out.cipher._<cbcMode>();

                    if (ok)
                    {
                        explicitIVLen = cbc.BlockSize();
                    }
                }
                if (explicitIVLen == 0L)
                {
                    {
                        aead (c, ok) = c.@out.cipher._<aead>();

                        if (ok)
                        {
                            explicitIVLen = c.explicitNonceLen(); 

                            // The AES-GCM construction in TLS has an
                            // explicit nonce so that the nonce can be
                            // random. However, the nonce is only 8 bytes
                            // which is too small for a secure, random
                            // nonce. Therefore we use the sequence number
                            // as the nonce.
                            explicitIVIsSeq = explicitIVLen > 0L;
                        }

                    }
                }
                var m = len(data);
                {
                    var maxPayload = c.maxPayloadSizeForWrite(typ, explicitIVLen);

                    if (m > maxPayload)
                    {
                        m = maxPayload;
                    }

                }
                b.resize(recordHeaderLen + explicitIVLen + m);
                b.data[0L] = byte(typ);
                var vers = c.vers;
                if (vers == 0L)
                { 
                    // Some TLS servers fail if the record version is
                    // greater than TLS 1.0 for the initial ClientHello.
                    vers = VersionTLS10;
                }
                b.data[1L] = byte(vers >> (int)(8L));
                b.data[2L] = byte(vers);
                b.data[3L] = byte(m >> (int)(8L));
                b.data[4L] = byte(m);
                if (explicitIVLen > 0L)
                {
                    var explicitIV = b.data[recordHeaderLen..recordHeaderLen + explicitIVLen];
                    if (explicitIVIsSeq)
                    {
                        copy(explicitIV, c.@out.seq[..]);
                    }
                    else
                    {
                        {
                            var (_, err) = io.ReadFull(c.config.rand(), explicitIV);

                            if (err != null)
                            {
                                return (n, err);
                            }

                        }
                    }
                }
                copy(b.data[recordHeaderLen + explicitIVLen..], data);
                c.@out.encrypt(b, explicitIVLen);
                {
                    (_, err) = c.write(b.data);

                    if (err != null)
                    {
                        return (n, err);
                    }

                }
                n += m;
                data = data[m..];
            }


            if (typ == recordTypeChangeCipherSpec)
            {
                {
                    var err = c.@out.changeCipherSpec();

                    if (err != null)
                    {
                        return (n, c.sendAlertLocked(err._<alert>()));
                    }

                }
            }
            return (n, null);
        });

        // writeRecord writes a TLS record with the given type and payload to the
        // connection and updates the record layer state.
        // L < c.out.Mutex.
        private static (long, error) writeRecord(this ref Conn _c, recordType typ, slice<byte> data) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.@out.Lock();
            defer(c.@out.Unlock());

            return c.writeRecordLocked(typ, data);
        });

        // readHandshake reads the next handshake message from
        // the record layer.
        // c.in.Mutex < L; c.out.Mutex < L.
        private static (object, error) readHandshake(this ref Conn c)
        {
            while (c.hand.Len() < 4L)
            {
                {
                    var err__prev1 = err;

                    var err = c.@in.err;

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = c.readRecord(recordTypeHandshake);

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev1;

                }
            }


            var data = c.hand.Bytes();
            var n = int(data[1L]) << (int)(16L) | int(data[2L]) << (int)(8L) | int(data[3L]);
            if (n > maxHandshake)
            {
                c.sendAlertLocked(alertInternalError);
                return (null, c.@in.setErrorLocked(fmt.Errorf("tls: handshake message of length %d bytes exceeds maximum of %d bytes", n, maxHandshake)));
            }
            while (c.hand.Len() < 4L + n)
            {
                {
                    var err__prev1 = err;

                    err = c.@in.err;

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev1;

                }
                {
                    var err__prev1 = err;

                    err = c.readRecord(recordTypeHandshake);

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev1;

                }
            }

            data = c.hand.Next(4L + n);
            handshakeMessage m = default;

            if (data[0L] == typeHelloRequest) 
                m = @new<helloRequestMsg>();
            else if (data[0L] == typeClientHello) 
                m = @new<clientHelloMsg>();
            else if (data[0L] == typeServerHello) 
                m = @new<serverHelloMsg>();
            else if (data[0L] == typeNewSessionTicket) 
                m = @new<newSessionTicketMsg>();
            else if (data[0L] == typeCertificate) 
                m = @new<certificateMsg>();
            else if (data[0L] == typeCertificateRequest) 
                m = ref new certificateRequestMsg(hasSignatureAndHash:c.vers>=VersionTLS12,);
            else if (data[0L] == typeCertificateStatus) 
                m = @new<certificateStatusMsg>();
            else if (data[0L] == typeServerKeyExchange) 
                m = @new<serverKeyExchangeMsg>();
            else if (data[0L] == typeServerHelloDone) 
                m = @new<serverHelloDoneMsg>();
            else if (data[0L] == typeClientKeyExchange) 
                m = @new<clientKeyExchangeMsg>();
            else if (data[0L] == typeCertificateVerify) 
                m = ref new certificateVerifyMsg(hasSignatureAndHash:c.vers>=VersionTLS12,);
            else if (data[0L] == typeNextProtocol) 
                m = @new<nextProtoMsg>();
            else if (data[0L] == typeFinished) 
                m = @new<finishedMsg>();
            else 
                return (null, c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)));
            // The handshake message unmarshalers
            // expect to be able to keep references to data,
            // so pass in a fresh copy that won't be overwritten.
            data = append((slice<byte>)null, data);

            if (!m.unmarshal(data))
            {
                return (null, c.@in.setErrorLocked(c.sendAlert(alertUnexpectedMessage)));
            }
            return (m, null);
        }

        private static var errClosed = errors.New("tls: use of closed connection");        private static var errShutdown = errors.New("tls: protocol is shutdown");

        // Write writes data to the connection.
        private static (long, error) Write(this ref Conn _c, slice<byte> b) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        { 
            // interlock with Close below
            while (true)
            {
                var x = atomic.LoadInt32(ref c.activeCall);
                if (x & 1L != 0L)
                {
                    return (0L, errClosed);
                }
                if (atomic.CompareAndSwapInt32(ref c.activeCall, x, x + 2L))
                {
                    defer(atomic.AddInt32(ref c.activeCall, -2L));
                    break;
                }
            }


            {
                var err__prev1 = err;

                var err = c.Handshake();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }

            c.@out.Lock();
            defer(c.@out.Unlock());

            {
                var err__prev1 = err;

                err = c.@out.err;

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }

            if (!c.handshakeComplete)
            {
                return (0L, alertInternalError);
            }
            if (c.closeNotifySent)
            {
                return (0L, errShutdown);
            } 

            // SSL 3.0 and TLS 1.0 are susceptible to a chosen-plaintext
            // attack when using block mode ciphers due to predictable IVs.
            // This can be prevented by splitting each Application Data
            // record into two records, effectively randomizing the IV.
            //
            // http://www.openssl.org/~bodo/tls-cbc.txt
            // https://bugzilla.mozilla.org/show_bug.cgi?id=665814
            // http://www.imperialviolet.org/2012/01/15/beastfollowup.html
            long m = default;
            if (len(b) > 1L && c.vers <= VersionTLS10)
            {
                {
                    cipher.BlockMode (_, ok) = c.@out.cipher._<cipher.BlockMode>();

                    if (ok)
                    {
                        var (n, err) = c.writeRecordLocked(recordTypeApplicationData, b[..1L]);
                        if (err != null)
                        {
                            return (n, c.@out.setErrorLocked(err));
                        }
                        m = 1L;
                        b = b[1L..];
                    }

                }
            }
            (n, err) = c.writeRecordLocked(recordTypeApplicationData, b);
            return (n + m, c.@out.setErrorLocked(err));
        });

        // handleRenegotiation processes a HelloRequest handshake message.
        // c.in.Mutex <= L
        private static error handleRenegotiation(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            var (msg, err) = c.readHandshake();
            if (err != null)
            {
                return error.As(err);
            }
            ref helloRequestMsg (_, ok) = msg._<ref helloRequestMsg>();
            if (!ok)
            {
                c.sendAlert(alertUnexpectedMessage);
                return error.As(alertUnexpectedMessage);
            }
            if (!c.isClient)
            {
                return error.As(c.sendAlert(alertNoRenegotiation));
            }

            if (c.config.Renegotiation == RenegotiateNever) 
                return error.As(c.sendAlert(alertNoRenegotiation));
            else if (c.config.Renegotiation == RenegotiateOnceAsClient) 
                if (c.handshakes > 1L)
                {
                    return error.As(c.sendAlert(alertNoRenegotiation));
                }
            else if (c.config.Renegotiation == RenegotiateFreelyAsClient)             else 
                c.sendAlert(alertInternalError);
                return error.As(errors.New("tls: unknown Renegotiation value"));
                        c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());

            c.handshakeComplete = false;
            c.handshakeErr = c.clientHandshake();

            if (c.handshakeErr == null)
            {
                c.handshakes++;
            }
            return error.As(c.handshakeErr);
        });

        // Read can be made to time out and return a net.Error with Timeout() == true
        // after a fixed time limit; see SetDeadline and SetReadDeadline.
        private static (long, error) Read(this ref Conn _c, slice<byte> b) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            err = c.Handshake();

            if (err != null)
            {
                return;
            }
            if (len(b) == 0L)
            { 
                // Put this after Handshake, in case people were calling
                // Read(nil) for the side effect of the Handshake.
                return;
            }
            c.@in.Lock();
            defer(c.@in.Unlock()); 

            // Some OpenSSL servers send empty records in order to randomize the
            // CBC IV. So this loop ignores a limited number of empty records.
            const long maxConsecutiveEmptyRecords = 100L;

            for (long emptyRecordCount = 0L; emptyRecordCount <= maxConsecutiveEmptyRecords; emptyRecordCount++)
            {
                while (c.input == null && c.@in.err == null)
                {
                    {
                        var err__prev1 = err;

                        var err = c.readRecord(recordTypeApplicationData);

                        if (err != null)
                        { 
                            // Soft error, like EAGAIN
                            return (0L, err);
                        }

                        err = err__prev1;

                    }
                    if (c.hand.Len() > 0L)
                    { 
                        // We received handshake bytes, indicating the
                        // start of a renegotiation.
                        {
                            var err__prev2 = err;

                            err = c.handleRenegotiation();

                            if (err != null)
                            {
                                return (0L, err);
                            }

                            err = err__prev2;

                        }
                    }
                }

                {
                    var err__prev1 = err;

                    err = c.@in.err;

                    if (err != null)
                    {
                        return (0L, err);
                    }

                    err = err__prev1;

                }

                n, err = c.input.Read(b);
                if (c.input.off >= len(c.input.data))
                {
                    c.@in.freeBlock(c.input);
                    c.input = null;
                } 

                // If a close-notify alert is waiting, read it so that
                // we can return (n, EOF) instead of (n, nil), to signal
                // to the HTTP response reading goroutine that the
                // connection is now closed. This eliminates a race
                // where the HTTP response reading goroutine would
                // otherwise not observe the EOF until its next read,
                // by which time a client goroutine might have already
                // tried to reuse the HTTP connection for a new
                // request.
                // See https://codereview.appspot.com/76400046
                // and https://golang.org/issue/3514
                {
                    var ri = c.rawInput;

                    if (ri != null && n != 0L && err == null && c.input == null && len(ri.data) > 0L && recordType(ri.data[0L]) == recordTypeAlert)
                    {
                        {
                            var recErr = c.readRecord(recordTypeApplicationData);

                            if (recErr != null)
                            {
                                err = recErr; // will be io.EOF on closeNotify
                            }

                        }
                    }

                }

                if (n != 0L || err != null)
                {
                    return (n, err);
                }
            }


            return (0L, io.ErrNoProgress);
        });

        // Close closes the connection.
        private static error Close(this ref Conn c)
        { 
            // Interlock with Conn.Write above.
            int x = default;
            while (true)
            {
                x = atomic.LoadInt32(ref c.activeCall);
                if (x & 1L != 0L)
                {
                    return error.As(errClosed);
                }
                if (atomic.CompareAndSwapInt32(ref c.activeCall, x, x | 1L))
                {
                    break;
                }
            }

            if (x != 0L)
            { 
                // io.Writer and io.Closer should not be used concurrently.
                // If Close is called while a Write is currently in-flight,
                // interpret that as a sign that this Close is really just
                // being used to break the Write and/or clean up resources and
                // avoid sending the alertCloseNotify, which may block
                // waiting on handshakeMutex or the c.out mutex.
                return error.As(c.conn.Close());
            }
            error alertErr = default;

            c.handshakeMutex.Lock();
            if (c.handshakeComplete)
            {
                alertErr = error.As(c.closeNotify());
            }
            c.handshakeMutex.Unlock();

            {
                var err = c.conn.Close();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(alertErr);
        }

        private static var errEarlyCloseWrite = errors.New("tls: CloseWrite called before handshake complete");

        // CloseWrite shuts down the writing side of the connection. It should only be
        // called once the handshake has completed and does not call CloseWrite on the
        // underlying connection. Most callers should just use Close.
        private static error CloseWrite(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());
            if (!c.handshakeComplete)
            {
                return error.As(errEarlyCloseWrite);
            }
            return error.As(c.closeNotify());
        });

        private static error closeNotify(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.@out.Lock();
            defer(c.@out.Unlock());

            if (!c.closeNotifySent)
            {
                c.closeNotifyErr = c.sendAlertLocked(alertCloseNotify);
                c.closeNotifySent = true;
            }
            return error.As(c.closeNotifyErr);
        });

        // Handshake runs the client or server handshake
        // protocol if it has not yet been run.
        // Most uses of this package need not call Handshake
        // explicitly: the first Read or Write will call it automatically.
        private static error Handshake(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic panic, Recover _) =>
        { 
            // c.handshakeErr and c.handshakeComplete are protected by
            // c.handshakeMutex. In order to perform a handshake, we need to lock
            // c.in also and c.handshakeMutex must be locked after c.in.
            //
            // However, if a Read() operation is hanging then it'll be holding the
            // lock on c.in and so taking it here would cause all operations that
            // need to check whether a handshake is pending (such as Write) to
            // block.
            //
            // Thus we first take c.handshakeMutex to check whether a handshake is
            // needed.
            //
            // If so then, previously, this code would unlock handshakeMutex and
            // then lock c.in and handshakeMutex in the correct order to run the
            // handshake. The problem was that it was possible for a Read to
            // complete the handshake once handshakeMutex was unlocked and then
            // keep c.in while waiting for network data. Thus a concurrent
            // operation could be blocked on c.in.
            //
            // Thus handshakeCond is used to signal that a goroutine is committed
            // to running the handshake and other goroutines can wait on it if they
            // need. handshakeCond is protected by handshakeMutex.
            c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());

            while (true)
            {
                {
                    var err = c.handshakeErr;

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
                if (c.handshakeComplete)
                {
                    return error.As(null);
                }
                if (c.handshakeCond == null)
                {
                    break;
                }
                c.handshakeCond.Wait();
            } 

            // Set handshakeCond to indicate that this goroutine is committing to
            // running the handshake.
 

            // Set handshakeCond to indicate that this goroutine is committing to
            // running the handshake.
            c.handshakeCond = sync.NewCond(ref c.handshakeMutex);
            c.handshakeMutex.Unlock();

            c.@in.Lock();
            defer(c.@in.Unlock());

            c.handshakeMutex.Lock(); 

            // The handshake cannot have completed when handshakeMutex was unlocked
            // because this goroutine set handshakeCond.
            if (c.handshakeErr != null || c.handshakeComplete)
            {
                panic("handshake should not have been able to complete after handshakeCond was set");
            }
            if (c.isClient)
            {
                c.handshakeErr = c.clientHandshake();
            }
            else
            {
                c.handshakeErr = c.serverHandshake();
            }
            if (c.handshakeErr == null)
            {
                c.handshakes++;
            }
            else
            { 
                // If an error occurred during the hadshake try to flush the
                // alert that might be left in the buffer.
                c.flush();
            }
            if (c.handshakeErr == null && !c.handshakeComplete)
            {
                panic("handshake should have had a result.");
            } 

            // Wake any other goroutines that are waiting for this handshake to
            // complete.
            c.handshakeCond.Broadcast();
            c.handshakeCond = null;

            return error.As(c.handshakeErr);
        });

        // ConnectionState returns basic TLS details about the connection.
        private static ConnectionState ConnectionState(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());

            ConnectionState state = default;
            state.HandshakeComplete = c.handshakeComplete;
            state.ServerName = c.serverName;

            if (c.handshakeComplete)
            {
                state.Version = c.vers;
                state.NegotiatedProtocol = c.clientProtocol;
                state.DidResume = c.didResume;
                state.NegotiatedProtocolIsMutual = !c.clientProtocolFallback;
                state.CipherSuite = c.cipherSuite;
                state.PeerCertificates = c.peerCertificates;
                state.VerifiedChains = c.verifiedChains;
                state.SignedCertificateTimestamps = c.scts;
                state.OCSPResponse = c.ocspResponse;
                if (!c.didResume)
                {
                    if (c.clientFinishedIsFirst)
                    {
                        state.TLSUnique = c.clientFinished[..];
                    }
                    else
                    {
                        state.TLSUnique = c.serverFinished[..];
                    }
                }
            }
            return state;
        });

        // OCSPResponse returns the stapled OCSP response from the TLS server, if
        // any. (Only valid for client connections.)
        private static slice<byte> OCSPResponse(this ref Conn _c) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());

            return c.ocspResponse;
        });

        // VerifyHostname checks that the peer certificate chain is valid for
        // connecting to host. If so, it returns nil; if not, it returns an error
        // describing the problem.
        private static error VerifyHostname(this ref Conn _c, @string host) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            c.handshakeMutex.Lock();
            defer(c.handshakeMutex.Unlock());
            if (!c.isClient)
            {
                return error.As(errors.New("tls: VerifyHostname called on TLS server connection"));
            }
            if (!c.handshakeComplete)
            {
                return error.As(errors.New("tls: handshake has not yet been performed"));
            }
            if (len(c.verifiedChains) == 0L)
            {
                return error.As(errors.New("tls: handshake did not verify certificate chain"));
            }
            return error.As(c.peerCertificates[0L].VerifyHostname(host));
        });
    }
}}
