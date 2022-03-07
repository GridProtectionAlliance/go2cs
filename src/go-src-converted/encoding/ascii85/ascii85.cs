// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ascii85 implements the ascii85 data encoding
// as used in the btoa tool and Adobe's PostScript and PDF document formats.
// package ascii85 -- go2cs converted at 2022 March 06 22:24:52 UTC
// import "encoding/ascii85" ==> using ascii85 = go.encoding.ascii85_package
// Original source: C:\Program Files\Go\src\encoding\ascii85\ascii85.go
using io = go.io_package;
using strconv = go.strconv_package;

namespace go.encoding;

public static partial class ascii85_package {

    /*
     * Encoder
     */

    // Encode encodes src into at most MaxEncodedLen(len(src))
    // bytes of dst, returning the actual number of bytes written.
    //
    // The encoding handles 4-byte chunks, using a special encoding
    // for the last fragment, so Encode is not appropriate for use on
    // individual blocks of a large data stream. Use NewEncoder() instead.
    //
    // Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
    // Encode does not add these.
public static nint Encode(slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return 0;
    }
    nint n = 0;
    while (len(src) > 0) {
        dst[0] = 0;
        dst[1] = 0;
        dst[2] = 0;
        dst[3] = 0;
        dst[4] = 0; 

        // Unpack 4 bytes into uint32 to repack into base 85 5-byte.
        uint v = default;

        if (len(src) == 3)
        {
            v |= uint32(src[2]) << 8;
            fallthrough = true;
        }
        if (fallthrough || len(src) == 2)
        {
            v |= uint32(src[1]) << 16;
            fallthrough = true;
        }
        if (fallthrough || len(src) == 1)
        {
            v |= uint32(src[0]) << 24;
            goto __switch_break0;
        }
        // default: 
            v |= uint32(src[3]);

        __switch_break0:; 

        // Special case: zero (!!!!!) shortens to z.
        if (v == 0 && len(src) >= 4) {
            dst[0] = 'z';
            dst = dst[(int)1..];
            src = src[(int)4..];
            n++;
            continue;
        }
        for (nint i = 4; i >= 0; i--) {
            dst[i] = '!' + byte(v % 85);
            v /= 85;
        } 

        // If src was short, discard the low destination bytes.
        nint m = 5;
        if (len(src) < 4) {
            m -= 4 - len(src);
            src = null;
        }
        else
 {
            src = src[(int)4..];
        }
        dst = dst[(int)m..];
        n += m;

    }
    return n;

}

// MaxEncodedLen returns the maximum length of an encoding of n source bytes.
public static nint MaxEncodedLen(nint n) {
    return (n + 3) / 4 * 5;
}

// NewEncoder returns a new ascii85 stream encoder. Data written to
// the returned writer will be encoded and then written to w.
// Ascii85 encodings operate in 32-bit blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// trailing partial block.
public static io.WriteCloser NewEncoder(io.Writer w) {
    return addr(new encoder(w:w));
}

private partial struct encoder {
    public error err;
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
        for (i = 0; i < len(p) && e.nbuf < 4; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p[(int)i..];
        if (e.nbuf < 4) {
            return ;
        }
        var nout = Encode(e.@out[(int)0..], e.buf[(int)0..]);
        _, e.err = e.w.Write(e.@out[(int)0..(int)nout]);

        if (e.err != null) {
            return (n, error.As(e.err)!);
        }
        e.nbuf = 0;

    }
    while (len(p) >= 4) {
        var nn = len(e.@out) / 5 * 4;
        if (nn > len(p)) {
            nn = len(p);
        }
        nn -= nn % 4;
        if (nn > 0) {
            nout = Encode(e.@out[(int)0..], p[(int)0..(int)nn]);
            _, e.err = e.w.Write(e.@out[(int)0..(int)nout]);

            if (e.err != null) {
                return (n, error.As(e.err)!);
            }

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
        var nout = Encode(e.@out[(int)0..], e.buf[(int)0..(int)e.nbuf]);
        e.nbuf = 0;
        _, e.err = e.w.Write(e.@out[(int)0..(int)nout]);
    }
    return error.As(e.err)!;

}

/*
 * Decoder
 */

public partial struct CorruptInputError { // : long
}

public static @string Error(this CorruptInputError e) {
    return "illegal ascii85 data at input byte " + strconv.FormatInt(int64(e), 10);
}

// Decode decodes src into dst, returning both the number
// of bytes written to dst and the number consumed from src.
// If src contains invalid ascii85 data, Decode will return the
// number of bytes successfully written and a CorruptInputError.
// Decode ignores space and control characters in src.
// Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
// Decode expects these to have been stripped by the caller.
//
// If flush is true, Decode assumes that src represents the
// end of the input stream and processes it completely rather
// than wait for the completion of another 32-bit block.
//
// NewDecoder wraps an io.Reader interface around Decode.
//
public static (nint, nint, error) Decode(slice<byte> dst, slice<byte> src, bool flush) {
    nint ndst = default;
    nint nsrc = default;
    error err = default!;

    uint v = default;
    nint nb = default;
    {
        var i__prev1 = i;

        foreach (var (__i, __b) in src) {
            i = __i;
            b = __b;
            if (len(dst) - ndst < 4) {
                return ;
            }

            if (b <= ' ') 
                continue;
            else if (b == 'z' && nb == 0) 
                nb = 5;
                v = 0;
            else if ('!' <= b && b <= 'u') 
                v = v * 85 + uint32(b - '!');
                nb++;
            else 
                return (0, 0, error.As(CorruptInputError(i))!);
                        if (nb == 5) {
                nsrc = i + 1;
                dst[ndst] = byte(v >> 24);
                dst[ndst + 1] = byte(v >> 16);
                dst[ndst + 2] = byte(v >> 8);
                dst[ndst + 3] = byte(v);
                ndst += 4;
                nb = 0;
                v = 0;
            }

        }
        i = i__prev1;
    }

    if (flush) {
        nsrc = len(src);
        if (nb > 0) { 
            // The number of output bytes in the last fragment
            // is the number of leftover input bytes - 1:
            // the extra byte provides enough bits to cover
            // the inefficiency of the encoding for the block.
            if (nb == 1) {
                return (0, 0, error.As(CorruptInputError(len(src)))!);
            }

            {
                var i__prev1 = i;

                for (var i = nb; i < 5; i++) { 
                    // The short encoding truncated the output value.
                    // We have to assume the worst case values (digit 84)
                    // in order to ensure that the top bits are correct.
                    v = v * 85 + 84;

                }


                i = i__prev1;
            }
            {
                var i__prev1 = i;

                for (i = 0; i < nb - 1; i++) {
                    dst[ndst] = byte(v >> 24);
                    v<<=8;
                    ndst++;
                }


                i = i__prev1;
            }

        }
    }
    return ;

}

// NewDecoder constructs a new ascii85 stream decoder.
public static io.Reader NewDecoder(io.Reader r) {
    return addr(new decoder(r:r));
}

private partial struct decoder {
    public error err;
    public error readErr;
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

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    if (d.err != null) {
        return (0, error.As(d.err)!);
    }
    while (true) { 
        // Copy leftover output from last decode.
        if (len(d.@out) > 0) {
            n = copy(p, d.@out);
            d.@out = d.@out[(int)n..];
            return ;
        }
        nint nn = default;        nint nsrc = default;        nint ndst = default;

        if (d.nbuf > 0) {
            ndst, nsrc, d.err = Decode(d.outbuf[(int)0..], d.buf[(int)0..(int)d.nbuf], d.readErr != null);
            if (ndst > 0) {
                d.@out = d.outbuf[(int)0..(int)ndst];
                d.nbuf = copy(d.buf[(int)0..], d.buf[(int)nsrc..(int)d.nbuf]);
                continue; // copy out and return
            }

            if (ndst == 0 && d.err == null) { 
                // Special case: input buffer is mostly filled with non-data bytes.
                // Filter out such bytes to make room for more input.
                nint off = 0;
                for (nint i = 0; i < d.nbuf; i++) {
                    if (d.buf[i] > ' ') {
                        d.buf[off] = d.buf[i];
                        off++;
                    }
                }

                d.nbuf = off;

            }

        }
        if (d.err != null) {
            return (0, error.As(d.err)!);
        }
        if (d.readErr != null) {
            d.err = d.readErr;
            return (0, error.As(d.err)!);
        }
        nn, d.readErr = d.r.Read(d.buf[(int)d.nbuf..]);
        d.nbuf += nn;

    }

}

} // end ascii85_package
