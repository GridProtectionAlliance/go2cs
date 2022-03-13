// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base32 implements base32 encoding as specified by RFC 4648.

// package base32 -- go2cs converted at 2022 March 13 05:39:25 UTC
// import "encoding/base32" ==> using base32 = go.encoding.base32_package
// Original source: C:\Program Files\Go\src\encoding\base32\base32.go
namespace go.encoding;

using io = io_package;
using strconv = strconv_package;


/*
 * Encodings
 */

// An Encoding is a radix 32 encoding/decoding scheme, defined by a
// 32-character alphabet. The most common is the "base32" encoding
// introduced for SASL GSSAPI and standardized in RFC 4648.
// The alternate "base32hex" encoding is used in DNSSEC.

public static partial class base32_package {

public partial struct Encoding {
    public array<byte> encode;
    public array<byte> decodeMap;
    public int padChar;
}

public static readonly int StdPadding = '='; // Standard padding character
public static readonly int NoPadding = -1; // No padding

private static readonly @string encodeStd = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

private static readonly @string encodeHex = "0123456789ABCDEFGHIJKLMNOPQRSTUV";

// NewEncoding returns a new Encoding defined by the given alphabet,
// which must be a 32-byte string.


// NewEncoding returns a new Encoding defined by the given alphabet,
// which must be a 32-byte string.
public static ptr<Encoding> NewEncoding(@string encoder) => func((_, panic, _) => {
    if (len(encoder) != 32) {
        panic("encoding alphabet is not 32-bytes long");
    }
    ptr<Encoding> e = @new<Encoding>();
    copy(e.encode[..], encoder);
    e.padChar = StdPadding;

    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(e.decodeMap); i++) {
            e.decodeMap[i] = 0xFF;
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < len(encoder); i++) {
            e.decodeMap[encoder[i]] = byte(i);
        }

        i = i__prev1;
    }
    return _addr_e!;
});

// StdEncoding is the standard base32 encoding, as defined in
// RFC 4648.
public static var StdEncoding = NewEncoding(encodeStd);

// HexEncoding is the ``Extended Hex Alphabet'' defined in RFC 4648.
// It is typically used in DNS.
public static var HexEncoding = NewEncoding(encodeHex);

// WithPadding creates a new encoding identical to enc except
// with a specified padding character, or NoPadding to disable padding.
// The padding character must not be '\r' or '\n', must not
// be contained in the encoding's alphabet and must be a rune equal or
// below '\xff'.
public static ptr<Encoding> WithPadding(this Encoding enc, int padding) => func((_, panic, _) => {
    if (padding == '\r' || padding == '\n' || padding > 0xff) {
        panic("invalid padding");
    }
    for (nint i = 0; i < len(enc.encode); i++) {
        if (rune(enc.encode[i]) == padding) {
            panic("padding contained in alphabet");
        }
    }

    enc.padChar = padding;
    return _addr__addr_enc!;
});

/*
 * Encoder
 */

// Encode encodes src using the encoding enc, writing
// EncodedLen(len(src)) bytes to dst.
//
// The encoding pads the output to a multiple of 8 bytes,
// so Encode is not appropriate for use on individual blocks
// of a large data stream. Use NewEncoder() instead.
private static void Encode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src) {
    ref Encoding enc = ref _addr_enc.val;

    while (len(src) > 0) {
        array<byte> b = new array<byte>(8); 

        // Unpack 8x 5-bit source blocks into a 5 byte
        // destination quantum

        if (len(src) == 4)
        {
            b[6] |= (src[3] << 3) & 0x1F;
            b[5] = (src[3] >> 2) & 0x1F;
            b[4] = src[3] >> 7;
            fallthrough = true;
        }
        if (fallthrough || len(src) == 3)
        {
            b[4] |= (src[2] << 1) & 0x1F;
            b[3] = (src[2] >> 4) & 0x1F;
            fallthrough = true;
        }
        if (fallthrough || len(src) == 2)
        {
            b[3] |= (src[1] << 4) & 0x1F;
            b[2] = (src[1] >> 1) & 0x1F;
            b[1] = (src[1] >> 6) & 0x1F;
            fallthrough = true;
        }
        if (fallthrough || len(src) == 1)
        {
            b[1] |= (src[0] << 2) & 0x1F;
            b[0] = src[0] >> 3;
            goto __switch_break0;
        }
        // default: 
            b[7] = src[4] & 0x1F;
            b[6] = src[4] >> 5;

        __switch_break0:; 

        // Encode 5-bit blocks using the base32 alphabet
        var size = len(dst);
        if (size >= 8) { 
            // Common case, unrolled for extra performance
            dst[0] = enc.encode[b[0] & 31];
            dst[1] = enc.encode[b[1] & 31];
            dst[2] = enc.encode[b[2] & 31];
            dst[3] = enc.encode[b[3] & 31];
            dst[4] = enc.encode[b[4] & 31];
            dst[5] = enc.encode[b[5] & 31];
            dst[6] = enc.encode[b[6] & 31];
            dst[7] = enc.encode[b[7] & 31];
        }
        else
 {
            for (nint i = 0; i < size; i++) {
                dst[i] = enc.encode[b[i] & 31];
            }
        }
        if (len(src) < 5) {
            if (enc.padChar == NoPadding) {
                break;
            }
            dst[7] = byte(enc.padChar);
            if (len(src) < 4) {
                dst[6] = byte(enc.padChar);
                dst[5] = byte(enc.padChar);
                if (len(src) < 3) {
                    dst[4] = byte(enc.padChar);
                    if (len(src) < 2) {
                        dst[3] = byte(enc.padChar);
                        dst[2] = byte(enc.padChar);
                    }
                }
            }
            break;
        }
        src = src[(int)5..];
        dst = dst[(int)8..];
    }
}

// EncodeToString returns the base32 encoding of src.
private static @string EncodeToString(this ptr<Encoding> _addr_enc, slice<byte> src) {
    ref Encoding enc = ref _addr_enc.val;

    var buf = make_slice<byte>(enc.EncodedLen(len(src)));
    enc.Encode(buf, src);
    return string(buf);
}

private partial struct encoder {
    public error err;
    public ptr<Encoding> enc;
    public io.Writer w;
    public array<byte> buf; // buffered data waiting to be encoded
    public nint nbuf; // number of bytes in buf
    public array<byte> @out; // output buffer
}

private static (nint, error) Write(this ptr<encoder> _addr_e, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref encoder e = ref _addr_e.val;

    if (e.err != null) {
        return (0, error.As(e.err)!);
    }
    if (e.nbuf > 0) {
        nint i = default;
        for (i = 0; i < len(p) && e.nbuf < 5; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p[(int)i..];
        if (e.nbuf < 5) {
            return ;
        }
        e.enc.Encode(e.@out[(int)0..], e.buf[(int)0..]);
        _, e.err = e.w.Write(e.@out[(int)0..(int)8]);

        if (e.err != null) {
            return (n, error.As(e.err)!);
        }
        e.nbuf = 0;
    }
    while (len(p) >= 5) {
        var nn = len(e.@out) / 8 * 5;
        if (nn > len(p)) {
            nn = len(p);
            nn -= nn % 5;
        }
        e.enc.Encode(e.@out[(int)0..], p[(int)0..(int)nn]);
        _, e.err = e.w.Write(e.@out[(int)0..(int)nn / 5 * 8]);

        if (e.err != null) {
            return (n, error.As(e.err)!);
        }
        n += nn;
        p = p[(int)nn..];
    } 

    // Trailing fringe.
    {
        nint i__prev1 = i;

        for (i = 0; i < len(p); i++) {
            e.buf[i] = p[i];
        }

        i = i__prev1;
    }
    e.nbuf = len(p);
    n += len(p);
    return ;
}

// Close flushes any pending output from the encoder.
// It is an error to call Write after calling Close.
private static error Close(this ptr<encoder> _addr_e) {
    ref encoder e = ref _addr_e.val;
 
    // If there's anything left in the buffer, flush it out
    if (e.err == null && e.nbuf > 0) {
        e.enc.Encode(e.@out[(int)0..], e.buf[(int)0..(int)e.nbuf]);
        var encodedLen = e.enc.EncodedLen(e.nbuf);
        e.nbuf = 0;
        _, e.err = e.w.Write(e.@out[(int)0..(int)encodedLen]);
    }
    return error.As(e.err)!;
}

// NewEncoder returns a new base32 stream encoder. Data written to
// the returned writer will be encoded using enc and then written to w.
// Base32 encodings operate in 5-byte blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// partially written blocks.
public static io.WriteCloser NewEncoder(ptr<Encoding> _addr_enc, io.Writer w) {
    ref Encoding enc = ref _addr_enc.val;

    return addr(new encoder(enc:enc,w:w));
}

// EncodedLen returns the length in bytes of the base32 encoding
// of an input buffer of length n.
private static nint EncodedLen(this ptr<Encoding> _addr_enc, nint n) {
    ref Encoding enc = ref _addr_enc.val;

    if (enc.padChar == NoPadding) {
        return (n * 8 + 4) / 5;
    }
    return (n + 4) / 5 * 8;
}

/*
 * Decoder
 */

public partial struct CorruptInputError { // : long
}

public static @string Error(this CorruptInputError e) {
    return "illegal base32 data at input byte " + strconv.FormatInt(int64(e), 10);
}

// decode is like Decode but returns an additional 'end' value, which
// indicates if end-of-message padding was encountered and thus any
// additional data is an error. This method assumes that src has been
// stripped of all supported whitespace ('\r' and '\n').
private static (nint, bool, error) decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src) {
    nint n = default;
    bool end = default;
    error err = default!;
    ref Encoding enc = ref _addr_enc.val;
 
    // Lift the nil check outside of the loop.
    _ = enc.decodeMap;

    nint dsti = 0;
    var olen = len(src);

    while (len(src) > 0 && !end) { 
        // Decode quantum using the base32 alphabet
        array<byte> dbuf = new array<byte>(8);
        nint dlen = 8;

        {
            nint j = 0;

            while (j < 8) {
                if (len(src) == 0) {
                    if (enc.padChar != NoPadding) { 
                        // We have reached the end and are missing padding
                        return (n, false, error.As(CorruptInputError(olen - len(src) - j))!);
                    } 
                    // We have reached the end and are not expecting any padding
                    (dlen, end) = (j, true);                    break;
                }
                var @in = src[0];
                src = src[(int)1..];
                if (in == byte(enc.padChar) && j >= 2 && len(src) < 8) { 
                    // We've reached the end and there's padding
                    if (len(src) + j < 8 - 1) { 
                        // not enough padding
                        return (n, false, error.As(CorruptInputError(olen))!);
                    }
                    for (nint k = 0; k < 8 - 1 - j; k++) {
                        if (len(src) > k && src[k] != byte(enc.padChar)) { 
                            // incorrect padding
                            return (n, false, error.As(CorruptInputError(olen - len(src) + k - 1))!);
                        }
                    }

                    (dlen, end) = (j, true);                    if (dlen == 1 || dlen == 3 || dlen == 6) {
                        return (n, false, error.As(CorruptInputError(olen - len(src) - 1))!);
                    }
                    break;
                }
                dbuf[j] = enc.decodeMap[in];
                if (dbuf[j] == 0xFF) {
                    return (n, false, error.As(CorruptInputError(olen - len(src) - 1))!);
                }
                j++;
            } 

            // Pack 8x 5-bit source blocks into 5 byte destination
            // quantum

        } 

        // Pack 8x 5-bit source blocks into 5 byte destination
        // quantum

        if (dlen == 8)
        {
            dst[dsti + 4] = dbuf[6] << 5 | dbuf[7];
            n++;
            fallthrough = true;
        }
        if (fallthrough || dlen == 7)
        {
            dst[dsti + 3] = dbuf[4] << 7 | dbuf[5] << 2 | dbuf[6] >> 3;
            n++;
            fallthrough = true;
        }
        if (fallthrough || dlen == 5)
        {
            dst[dsti + 2] = dbuf[3] << 4 | dbuf[4] >> 1;
            n++;
            fallthrough = true;
        }
        if (fallthrough || dlen == 4)
        {
            dst[dsti + 1] = dbuf[1] << 6 | dbuf[2] << 1 | dbuf[3] >> 4;
            n++;
            fallthrough = true;
        }
        if (fallthrough || dlen == 2)
        {
            dst[dsti + 0] = dbuf[0] << 3 | dbuf[1] >> 2;
            n++;
            goto __switch_break1;
        }

        __switch_break1:;
        dsti += 5;
    }
    return (n, end, error.As(null!)!);
}

// Decode decodes src using the encoding enc. It writes at most
// DecodedLen(len(src)) bytes to dst and returns the number of bytes
// written. If src contains invalid base32 data, it will return the
// number of bytes successfully written and CorruptInputError.
// New line characters (\r and \n) are ignored.
private static (nint, error) Decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src) {
    nint n = default;
    error err = default!;
    ref Encoding enc = ref _addr_enc.val;

    var buf = make_slice<byte>(len(src));
    var l = stripNewlines(buf, src);
    n, _, err = enc.decode(dst, buf[..(int)l]);
    return ;
}

// DecodeString returns the bytes represented by the base32 string s.
private static (slice<byte>, error) DecodeString(this ptr<Encoding> _addr_enc, @string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Encoding enc = ref _addr_enc.val;

    slice<byte> buf = (slice<byte>)s;
    var l = stripNewlines(buf, buf);
    var (n, _, err) = enc.decode(buf, buf[..(int)l]);
    return (buf[..(int)n], error.As(err)!);
}

private partial struct decoder {
    public error err;
    public ptr<Encoding> enc;
    public io.Reader r;
    public bool end; // saw end of message
    public array<byte> buf; // leftover input
    public nint nbuf;
    public slice<byte> @out; // leftover decoded output
    public array<byte> outbuf;
}

private static (nint, error) readEncodedData(io.Reader r, slice<byte> buf, nint min, bool expectsPadding) {
    nint n = default;
    error err = default!;

    while (n < min && err == null) {
        nint nn = default;
        nn, err = r.Read(buf[(int)n..]);
        n += nn;
    } 
    // data was read, less than min bytes could be read
    if (n < min && n > 0 && err == io.EOF) {
        err = io.ErrUnexpectedEOF;
    }
    if (expectsPadding && min < 8 && n == 0 && err == io.EOF) {
        err = io.ErrUnexpectedEOF;
    }
    return ;
}

private static (nint, error) Read(this ptr<decoder> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref decoder d = ref _addr_d.val;
 
    // Use leftover decoded output from last read.
    if (len(d.@out) > 0) {
        n = copy(p, d.@out);
        d.@out = d.@out[(int)n..];
        if (len(d.@out) == 0) {
            return (n, error.As(d.err)!);
        }
        return (n, error.As(null!)!);
    }
    if (d.err != null) {
        return (0, error.As(d.err)!);
    }
    var nn = len(p) / 5 * 8;
    if (nn < 8) {
        nn = 8;
    }
    if (nn > len(d.buf)) {
        nn = len(d.buf);
    }
    nint min = default;
    bool expectsPadding = default;
    if (d.enc.padChar == NoPadding) {
        min = 1;
        expectsPadding = false;
    }
    else
 {
        min = 8 - d.nbuf;
        expectsPadding = true;
    }
    nn, d.err = readEncodedData(d.r, d.buf[(int)d.nbuf..(int)nn], min, expectsPadding);
    d.nbuf += nn;
    if (d.nbuf < min) {
        return (0, error.As(d.err)!);
    }
    nint nr = default;
    if (d.enc.padChar == NoPadding) {
        nr = d.nbuf;
    }
    else
 {
        nr = d.nbuf / 8 * 8;
    }
    var nw = d.enc.DecodedLen(d.nbuf);

    if (nw > len(p)) {
        nw, d.end, err = d.enc.decode(d.outbuf[(int)0..], d.buf[(int)0..(int)nr]);
        d.@out = d.outbuf[(int)0..(int)nw];
        n = copy(p, d.@out);
        d.@out = d.@out[(int)n..];
    }
    else
 {
        n, d.end, err = d.enc.decode(p, d.buf[(int)0..(int)nr]);
    }
    d.nbuf -= nr;
    for (nint i = 0; i < d.nbuf; i++) {
        d.buf[i] = d.buf[i + nr];
    }

    if (err != null && (d.err == null || d.err == io.EOF)) {
        d.err = err;
    }
    if (len(d.@out) > 0) { 
        // We cannot return all the decoded bytes to the caller in this
        // invocation of Read, so we return a nil error to ensure that Read
        // will be called again.  The error stored in d.err, if any, will be
        // returned with the last set of decoded bytes.
        return (n, error.As(null!)!);
    }
    return (n, error.As(d.err)!);
}

private partial struct newlineFilteringReader {
    public io.Reader wrapped;
}

// stripNewlines removes newline characters and returns the number
// of non-newline characters copied to dst.
private static nint stripNewlines(slice<byte> dst, slice<byte> src) {
    nint offset = 0;
    foreach (var (_, b) in src) {
        if (b == '\r' || b == '\n') {
            continue;
        }
        dst[offset] = b;
        offset++;
    }    return offset;
}

private static (nint, error) Read(this ptr<newlineFilteringReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref newlineFilteringReader r = ref _addr_r.val;

    var (n, err) = r.wrapped.Read(p);
    while (n > 0) {
        var s = p[(int)0..(int)n];
        var offset = stripNewlines(s, s);
        if (err != null || offset > 0) {
            return (offset, error.As(err)!);
        }
        n, err = r.wrapped.Read(p);
    }
    return (n, error.As(err)!);
}

// NewDecoder constructs a new base32 stream decoder.
public static io.Reader NewDecoder(ptr<Encoding> _addr_enc, io.Reader r) {
    ref Encoding enc = ref _addr_enc.val;

    return addr(new decoder(enc:enc,r:&newlineFilteringReader{r}));
}

// DecodedLen returns the maximum length in bytes of the decoded data
// corresponding to n bytes of base32-encoded data.
private static nint DecodedLen(this ptr<Encoding> _addr_enc, nint n) {
    ref Encoding enc = ref _addr_enc.val;

    if (enc.padChar == NoPadding) {
        return n * 5 / 8;
    }
    return n / 8 * 5;
}

} // end base32_package
