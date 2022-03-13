// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base64 implements base64 encoding as specified by RFC 4648.

// package base64 -- go2cs converted at 2022 March 13 05:30:20 UTC
// import "encoding/base64" ==> using base64 = go.encoding.base64_package
// Original source: C:\Program Files\Go\src\encoding\base64\base64.go
namespace go.encoding;

using binary = encoding.binary_package;
using io = io_package;
using strconv = strconv_package;


/*
 * Encodings
 */

// An Encoding is a radix 64 encoding/decoding scheme, defined by a
// 64-character alphabet. The most common encoding is the "base64"
// encoding defined in RFC 4648 and used in MIME (RFC 2045) and PEM
// (RFC 1421).  RFC 4648 also defines an alternate encoding, which is
// the standard encoding with - and _ substituted for + and /.

public static partial class base64_package {

public partial struct Encoding {
    public array<byte> encode;
    public array<byte> decodeMap;
    public int padChar;
    public bool strict;
}

public static readonly int StdPadding = '='; // Standard padding character
public static readonly int NoPadding = -1; // No padding

private static readonly @string encodeStd = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

private static readonly @string encodeURL = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

// NewEncoding returns a new padded Encoding defined by the given alphabet,
// which must be a 64-byte string that does not contain the padding character
// or CR / LF ('\r', '\n').
// The resulting Encoding uses the default padding character ('='),
// which may be changed or disabled via WithPadding.


// NewEncoding returns a new padded Encoding defined by the given alphabet,
// which must be a 64-byte string that does not contain the padding character
// or CR / LF ('\r', '\n').
// The resulting Encoding uses the default padding character ('='),
// which may be changed or disabled via WithPadding.
public static ptr<Encoding> NewEncoding(@string encoder) => func((_, panic, _) => {
    if (len(encoder) != 64) {
        panic("encoding alphabet is not 64-bytes long");
    }
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(encoder); i++) {
            if (encoder[i] == '\n' || encoder[i] == '\r') {
                panic("encoding alphabet contains newline character");
            }
        }

        i = i__prev1;
    }

    ptr<Encoding> e = @new<Encoding>();
    e.padChar = StdPadding;
    copy(e.encode[..], encoder);

    {
        nint i__prev1 = i;

        for (i = 0; i < len(e.decodeMap); i++) {
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

// Strict creates a new encoding identical to enc except with
// strict decoding enabled. In this mode, the decoder requires that
// trailing padding bits are zero, as described in RFC 4648 section 3.5.
//
// Note that the input is still malleable, as new line characters
// (CR and LF) are still ignored.
public static ptr<Encoding> Strict(this Encoding enc) {
    enc.strict = true;
    return _addr__addr_enc!;
}

// StdEncoding is the standard base64 encoding, as defined in
// RFC 4648.
public static var StdEncoding = NewEncoding(encodeStd);

// URLEncoding is the alternate base64 encoding defined in RFC 4648.
// It is typically used in URLs and file names.
public static var URLEncoding = NewEncoding(encodeURL);

// RawStdEncoding is the standard raw, unpadded base64 encoding,
// as defined in RFC 4648 section 3.2.
// This is the same as StdEncoding but omits padding characters.
public static var RawStdEncoding = StdEncoding.WithPadding(NoPadding);

// RawURLEncoding is the unpadded alternate base64 encoding defined in RFC 4648.
// It is typically used in URLs and file names.
// This is the same as URLEncoding but omits padding characters.
public static var RawURLEncoding = URLEncoding.WithPadding(NoPadding);

/*
 * Encoder
 */

// Encode encodes src using the encoding enc, writing
// EncodedLen(len(src)) bytes to dst.
//
// The encoding pads the output to a multiple of 4 bytes,
// so Encode is not appropriate for use on individual blocks
// of a large data stream. Use NewEncoder() instead.
private static void Encode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src) {
    ref Encoding enc = ref _addr_enc.val;

    if (len(src) == 0) {
        return ;
    }
    _ = enc.encode;

    nint di = 0;
    nint si = 0;
    var n = (len(src) / 3) * 3;
    while (si < n) { 
        // Convert 3x 8bit source bytes into 4 bytes
        var val = uint(src[si + 0]) << 16 | uint(src[si + 1]) << 8 | uint(src[si + 2]);

        dst[di + 0] = enc.encode[val >> 18 & 0x3F];
        dst[di + 1] = enc.encode[val >> 12 & 0x3F];
        dst[di + 2] = enc.encode[val >> 6 & 0x3F];
        dst[di + 3] = enc.encode[val & 0x3F];

        si += 3;
        di += 4;
    }

    var remain = len(src) - si;
    if (remain == 0) {
        return ;
    }
    val = uint(src[si + 0]) << 16;
    if (remain == 2) {
        val |= uint(src[si + 1]) << 8;
    }
    dst[di + 0] = enc.encode[val >> 18 & 0x3F];
    dst[di + 1] = enc.encode[val >> 12 & 0x3F];

    switch (remain) {
        case 2: 
            dst[di + 2] = enc.encode[val >> 6 & 0x3F];
            if (enc.padChar != NoPadding) {
                dst[di + 3] = byte(enc.padChar);
            }
            break;
        case 1: 
            if (enc.padChar != NoPadding) {
                dst[di + 2] = byte(enc.padChar);
                dst[di + 3] = byte(enc.padChar);
            }
            break;
    }
}

// EncodeToString returns the base64 encoding of src.
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
        for (i = 0; i < len(p) && e.nbuf < 3; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p[(int)i..];
        if (e.nbuf < 3) {
            return ;
        }
        e.enc.Encode(e.@out[..], e.buf[..]);
        _, e.err = e.w.Write(e.@out[..(int)4]);

        if (e.err != null) {
            return (n, error.As(e.err)!);
        }
        e.nbuf = 0;
    }
    while (len(p) >= 3) {
        var nn = len(e.@out) / 4 * 3;
        if (nn > len(p)) {
            nn = len(p);
            nn -= nn % 3;
        }
        e.enc.Encode(e.@out[..], p[..(int)nn]);
        _, e.err = e.w.Write(e.@out[(int)0..(int)nn / 3 * 4]);

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
        e.enc.Encode(e.@out[..], e.buf[..(int)e.nbuf]);
        _, e.err = e.w.Write(e.@out[..(int)e.enc.EncodedLen(e.nbuf)]);
        e.nbuf = 0;
    }
    return error.As(e.err)!;
}

// NewEncoder returns a new base64 stream encoder. Data written to
// the returned writer will be encoded using enc and then written to w.
// Base64 encodings operate in 4-byte blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// partially written blocks.
public static io.WriteCloser NewEncoder(ptr<Encoding> _addr_enc, io.Writer w) {
    ref Encoding enc = ref _addr_enc.val;

    return addr(new encoder(enc:enc,w:w));
}

// EncodedLen returns the length in bytes of the base64 encoding
// of an input buffer of length n.
private static nint EncodedLen(this ptr<Encoding> _addr_enc, nint n) {
    ref Encoding enc = ref _addr_enc.val;

    if (enc.padChar == NoPadding) {
        return (n * 8 + 5) / 6; // minimum # chars at 6 bits per char
    }
    return (n + 2) / 3 * 4; // minimum # 4-char quanta, 3 bytes each
}

/*
 * Decoder
 */

public partial struct CorruptInputError { // : long
}

public static @string Error(this CorruptInputError e) {
    return "illegal base64 data at input byte " + strconv.FormatInt(int64(e), 10);
}

// decodeQuantum decodes up to 4 base64 bytes. The received parameters are
// the destination buffer dst, the source buffer src and an index in the
// source buffer si.
// It returns the number of bytes read from src, the number of bytes written
// to dst, and an error, if any.
private static (nint, nint, error) decodeQuantum(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src, nint si) {
    nint nsi = default;
    nint n = default;
    error err = default!;
    ref Encoding enc = ref _addr_enc.val;
 
    // Decode quantum using the base64 alphabet
    array<byte> dbuf = new array<byte>(4);
    nint dlen = 4; 

    // Lift the nil check outside of the loop.
    _ = enc.decodeMap;

    for (nint j = 0; j < len(dbuf); j++) {
        if (len(src) == si) {

            if (j == 0) 
                return (si, 0, error.As(null!)!);
            else if (j == 1 || enc.padChar != NoPadding) 
                return (si, 0, error.As(CorruptInputError(si - j))!);
                        dlen = j;
            break;
        }
        var @in = src[si];
        si++;

        var @out = enc.decodeMap[in];
        if (out != 0xff) {
            dbuf[j] = out;
            continue;
        }
        if (in == '\n' || in == '\r') {
            j--;
            continue;
        }
        if (rune(in) != enc.padChar) {
            return (si, 0, error.As(CorruptInputError(si - 1))!);
        }
        switch (j) {
            case 0: 
                // incorrect padding

            case 1: 
                // incorrect padding
                return (si, 0, error.As(CorruptInputError(si - 1))!);
                break;
            case 2: 
                // "==" is expected, the first "=" is already consumed.
                // skip over newlines
                while (si < len(src) && (src[si] == '\n' || src[si] == '\r')) {
                    si++;
                }

                if (si == len(src)) { 
                    // not enough padding
                    return (si, 0, error.As(CorruptInputError(len(src)))!);
                }
                if (rune(src[si]) != enc.padChar) { 
                    // incorrect padding
                    return (si, 0, error.As(CorruptInputError(si - 1))!);
                }
                si++;
                break;
        } 

        // skip over newlines
        while (si < len(src) && (src[si] == '\n' || src[si] == '\r')) {
            si++;
        }
        if (si < len(src)) { 
            // trailing garbage
            err = CorruptInputError(si);
        }
        dlen = j;
        break;
    } 

    // Convert 4x 6bit source bytes into 3 bytes
    var val = uint(dbuf[0]) << 18 | uint(dbuf[1]) << 12 | uint(dbuf[2]) << 6 | uint(dbuf[3]);
    (dbuf[2], dbuf[1], dbuf[0]) = (byte(val >> 0), byte(val >> 8), byte(val >> 16));
    if (dlen == 4)
    {
        dst[2] = dbuf[2];
        dbuf[2] = 0;
        fallthrough = true;
    }
    if (fallthrough || dlen == 3)
    {
        dst[1] = dbuf[1];
        if (enc.strict && dbuf[2] != 0) {
            return (si, 0, error.As(CorruptInputError(si - 1))!);
        }
        dbuf[1] = 0;
        fallthrough = true;
    }
    if (fallthrough || dlen == 2)
    {
        dst[0] = dbuf[0];
        if (enc.strict && (dbuf[1] != 0 || dbuf[2] != 0)) {
            return (si, 0, error.As(CorruptInputError(si - 2))!);
        }
        goto __switch_break0;
    }

    __switch_break0:;

    return (si, dlen - 1, error.As(err)!);
}

// DecodeString returns the bytes represented by the base64 string s.
private static (slice<byte>, error) DecodeString(this ptr<Encoding> _addr_enc, @string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Encoding enc = ref _addr_enc.val;

    var dbuf = make_slice<byte>(enc.DecodedLen(len(s)));
    var (n, err) = enc.Decode(dbuf, (slice<byte>)s);
    return (dbuf[..(int)n], error.As(err)!);
}

private partial struct decoder {
    public error err;
    public error readErr; // error from r.Read
    public ptr<Encoding> enc;
    public io.Reader r;
    public array<byte> buf; // leftover input
    public nint nbuf;
    public slice<byte> @out; // leftover decoded output
    public array<byte> outbuf;
}

private static (nint, error) Read(this ptr<decoder> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref decoder d = ref _addr_d.val;
 
    // Use leftover decoded output from last read.
    if (len(d.@out) > 0) {
        n = copy(p, d.@out);
        d.@out = d.@out[(int)n..];
        return (n, error.As(null!)!);
    }
    if (d.err != null) {
        return (0, error.As(d.err)!);
    }
    while (d.nbuf < 4 && d.readErr == null) {
        var nn = len(p) / 3 * 4;
        if (nn < 4) {
            nn = 4;
        }
        if (nn > len(d.buf)) {
            nn = len(d.buf);
        }
        nn, d.readErr = d.r.Read(d.buf[(int)d.nbuf..(int)nn]);
        d.nbuf += nn;
    }

    if (d.nbuf < 4) {
        if (d.enc.padChar == NoPadding && d.nbuf > 0) { 
            // Decode final fragment, without padding.
            nint nw = default;
            nw, d.err = d.enc.Decode(d.outbuf[..], d.buf[..(int)d.nbuf]);
            d.nbuf = 0;
            d.@out = d.outbuf[..(int)nw];
            n = copy(p, d.@out);
            d.@out = d.@out[(int)n..];
            if (n > 0 || len(p) == 0 && len(d.@out) > 0) {
                return (n, error.As(null!)!);
            }
            if (d.err != null) {
                return (0, error.As(d.err)!);
            }
        }
        d.err = d.readErr;
        if (d.err == io.EOF && d.nbuf > 0) {
            d.err = io.ErrUnexpectedEOF;
        }
        return (0, error.As(d.err)!);
    }
    var nr = d.nbuf / 4 * 4;
    nw = d.nbuf / 4 * 3;
    if (nw > len(p)) {
        nw, d.err = d.enc.Decode(d.outbuf[..], d.buf[..(int)nr]);
        d.@out = d.outbuf[..(int)nw];
        n = copy(p, d.@out);
        d.@out = d.@out[(int)n..];
    }
    else
 {
        n, d.err = d.enc.Decode(p, d.buf[..(int)nr]);
    }
    d.nbuf -= nr;
    copy(d.buf[..(int)d.nbuf], d.buf[(int)nr..]);
    return (n, error.As(d.err)!);
}

// Decode decodes src using the encoding enc. It writes at most
// DecodedLen(len(src)) bytes to dst and returns the number of bytes
// written. If src contains invalid base64 data, it will return the
// number of bytes successfully written and CorruptInputError.
// New line characters (\r and \n) are ignored.
private static (nint, error) Decode(this ptr<Encoding> _addr_enc, slice<byte> dst, slice<byte> src) {
    nint n = default;
    error err = default!;
    ref Encoding enc = ref _addr_enc.val;

    if (len(src) == 0) {
        return (0, error.As(null!)!);
    }
    _ = enc.decodeMap;

    nint si = 0;
    while (strconv.IntSize >= 64 && len(src) - si >= 8 && len(dst) - n >= 8) {
        var src2 = src[(int)si..(int)si + 8];
        {
            var dn__prev1 = dn;

            var (dn, ok) = assemble64(enc.decodeMap[src2[0]], enc.decodeMap[src2[1]], enc.decodeMap[src2[2]], enc.decodeMap[src2[3]], enc.decodeMap[src2[4]], enc.decodeMap[src2[5]], enc.decodeMap[src2[6]], enc.decodeMap[src2[7]]);

            if (ok) {
                binary.BigEndian.PutUint64(dst[(int)n..], dn);
                n += 6;
                si += 8;
            }
            else
 {
                nint ninc = default;
                si, ninc, err = enc.decodeQuantum(dst[(int)n..], src, si);
                n += ninc;
                if (err != null) {
                    return (n, error.As(err)!);
                }
            }

            dn = dn__prev1;

        }
    }

    while (len(src) - si >= 4 && len(dst) - n >= 4) {
        src2 = src[(int)si..(int)si + 4];
        {
            var dn__prev1 = dn;

            (dn, ok) = assemble32(enc.decodeMap[src2[0]], enc.decodeMap[src2[1]], enc.decodeMap[src2[2]], enc.decodeMap[src2[3]]);

            if (ok) {
                binary.BigEndian.PutUint32(dst[(int)n..], dn);
                n += 3;
                si += 4;
            }
            else
 {
                ninc = default;
                si, ninc, err = enc.decodeQuantum(dst[(int)n..], src, si);
                n += ninc;
                if (err != null) {
                    return (n, error.As(err)!);
                }
            }

            dn = dn__prev1;

        }
    }

    while (si < len(src)) {
        ninc = default;
        si, ninc, err = enc.decodeQuantum(dst[(int)n..], src, si);
        n += ninc;
        if (err != null) {
            return (n, error.As(err)!);
        }
    }
    return (n, error.As(err)!);
}

// assemble32 assembles 4 base64 digits into 3 bytes.
// Each digit comes from the decode map, and will be 0xff
// if it came from an invalid character.
private static (uint, bool) assemble32(byte n1, byte n2, byte n3, byte n4) {
    uint dn = default;
    bool ok = default;
 
    // Check that all the digits are valid. If any of them was 0xff, their
    // bitwise OR will be 0xff.
    if (n1 | n2 | n3 | n4 == 0xff) {
        return (0, false);
    }
    return (uint32(n1) << 26 | uint32(n2) << 20 | uint32(n3) << 14 | uint32(n4) << 8, true);
}

// assemble64 assembles 8 base64 digits into 6 bytes.
// Each digit comes from the decode map, and will be 0xff
// if it came from an invalid character.
private static (ulong, bool) assemble64(byte n1, byte n2, byte n3, byte n4, byte n5, byte n6, byte n7, byte n8) {
    ulong dn = default;
    bool ok = default;
 
    // Check that all the digits are valid. If any of them was 0xff, their
    // bitwise OR will be 0xff.
    if (n1 | n2 | n3 | n4 | n5 | n6 | n7 | n8 == 0xff) {
        return (0, false);
    }
    return (uint64(n1) << 58 | uint64(n2) << 52 | uint64(n3) << 46 | uint64(n4) << 40 | uint64(n5) << 34 | uint64(n6) << 28 | uint64(n7) << 22 | uint64(n8) << 16, true);
}

private partial struct newlineFilteringReader {
    public io.Reader wrapped;
}

private static (nint, error) Read(this ptr<newlineFilteringReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref newlineFilteringReader r = ref _addr_r.val;

    var (n, err) = r.wrapped.Read(p);
    while (n > 0) {
        nint offset = 0;
        foreach (var (i, b) in p[..(int)n]) {
            if (b != '\r' && b != '\n') {
                if (i != offset) {
                    p[offset] = b;
                }
                offset++;
            }
        }        if (offset > 0) {
            return (offset, error.As(err)!);
        }
        n, err = r.wrapped.Read(p);
    }
    return (n, error.As(err)!);
}

// NewDecoder constructs a new base64 stream decoder.
public static io.Reader NewDecoder(ptr<Encoding> _addr_enc, io.Reader r) {
    ref Encoding enc = ref _addr_enc.val;

    return addr(new decoder(enc:enc,r:&newlineFilteringReader{r}));
}

// DecodedLen returns the maximum length in bytes of the decoded data
// corresponding to n bytes of base64-encoded data.
private static nint DecodedLen(this ptr<Encoding> _addr_enc, nint n) {
    ref Encoding enc = ref _addr_enc.val;

    if (enc.padChar == NoPadding) { 
        // Unpadded data may end with partial block of 2-3 characters.
        return n * 6 / 8;
    }
    return n / 4 * 3;
}

} // end base64_package
