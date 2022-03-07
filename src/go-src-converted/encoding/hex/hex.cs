// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hex implements hexadecimal encoding and decoding.
// package hex -- go2cs converted at 2022 March 06 22:19:44 UTC
// import "encoding/hex" ==> using hex = go.encoding.hex_package
// Original source: C:\Program Files\Go\src\encoding\hex\hex.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;

namespace go.encoding;

public static partial class hex_package {

private static readonly @string hextable = "0123456789abcdef";

// EncodedLen returns the length of an encoding of n source bytes.
// Specifically, it returns n * 2.


// EncodedLen returns the length of an encoding of n source bytes.
// Specifically, it returns n * 2.
public static nint EncodedLen(nint n) {
    return n * 2;
}

// Encode encodes src into EncodedLen(len(src))
// bytes of dst. As a convenience, it returns the number
// of bytes written to dst, but this value is always EncodedLen(len(src)).
// Encode implements hexadecimal encoding.
public static nint Encode(slice<byte> dst, slice<byte> src) {
    nint j = 0;
    foreach (var (_, v) in src) {
        dst[j] = hextable[v >> 4];
        dst[j + 1] = hextable[v & 0x0f];
        j += 2;
    }    return len(src) * 2;
}

// ErrLength reports an attempt to decode an odd-length input
// using Decode or DecodeString.
// The stream-based Decoder returns io.ErrUnexpectedEOF instead of ErrLength.
public static var ErrLength = errors.New("encoding/hex: odd length hex string");

// InvalidByteError values describe errors resulting from an invalid byte in a hex string.
public partial struct InvalidByteError { // : byte
}

public static @string Error(this InvalidByteError e) {
    return fmt.Sprintf("encoding/hex: invalid byte: %#U", rune(e));
}

// DecodedLen returns the length of a decoding of x source bytes.
// Specifically, it returns x / 2.
public static nint DecodedLen(nint x) {
    return x / 2;
}

// Decode decodes src into DecodedLen(len(src)) bytes,
// returning the actual number of bytes written to dst.
//
// Decode expects that src contains only hexadecimal
// characters and that src has even length.
// If the input is malformed, Decode returns the number
// of bytes decoded before the error.
public static (nint, error) Decode(slice<byte> dst, slice<byte> src) {
    nint _p0 = default;
    error _p0 = default!;

    nint i = 0;
    nint j = 1;
    while (j < len(src)) {
        var (a, ok) = fromHexChar(src[j - 1]);
        if (!ok) {
            return (i, error.As(InvalidByteError(src[j - 1]))!);
        j += 2;
        }
        var (b, ok) = fromHexChar(src[j]);
        if (!ok) {
            return (i, error.As(InvalidByteError(src[j]))!);
        }
        dst[i] = (a << 4) | b;
        i++;

    }
    if (len(src) % 2 == 1) { 
        // Check for invalid char before reporting bad length,
        // since the invalid char (if present) is an earlier problem.
        {
            var (_, ok) = fromHexChar(src[j - 1]);

            if (!ok) {
                return (i, error.As(InvalidByteError(src[j - 1]))!);
            }

        }

        return (i, error.As(ErrLength)!);

    }
    return (i, error.As(null!)!);

}

// fromHexChar converts a hex character into its value and a success flag.
private static (byte, bool) fromHexChar(byte c) {
    byte _p0 = default;
    bool _p0 = default;


    if ('0' <= c && c <= '9') 
        return (c - '0', true);
    else if ('a' <= c && c <= 'f') 
        return (c - 'a' + 10, true);
    else if ('A' <= c && c <= 'F') 
        return (c - 'A' + 10, true);
        return (0, false);

}

// EncodeToString returns the hexadecimal encoding of src.
public static @string EncodeToString(slice<byte> src) {
    var dst = make_slice<byte>(EncodedLen(len(src)));
    Encode(dst, src);
    return string(dst);
}

// DecodeString returns the bytes represented by the hexadecimal string s.
//
// DecodeString expects that src contains only hexadecimal
// characters and that src has even length.
// If the input is malformed, DecodeString returns
// the bytes decoded before the error.
public static (slice<byte>, error) DecodeString(@string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    slice<byte> src = (slice<byte>)s; 
    // We can use the source slice itself as the destination
    // because the decode loop increments by one and then the 'seen' byte is not used anymore.
    var (n, err) = Decode(src, src);
    return (src[..(int)n], error.As(err)!);

}

// Dump returns a string that contains a hex dump of the given data. The format
// of the hex dump matches the output of `hexdump -C` on the command line.
public static @string Dump(slice<byte> data) {
    if (len(data) == 0) {
        return "";
    }
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf); 
    // Dumper will write 79 bytes per complete 16 byte chunk, and at least
    // 64 bytes for whatever remains. Round the allocation up, since only a
    // maximum of 15 bytes will be wasted.
    buf.Grow((1 + ((len(data) - 1) / 16)) * 79);

    var dumper = Dumper(_addr_buf);
    dumper.Write(data);
    dumper.Close();
    return buf.String();

}

// bufferSize is the number of hexadecimal characters to buffer in encoder and decoder.
private static readonly nint bufferSize = 1024;



private partial struct encoder {
    public io.Writer w;
    public error err;
    public array<byte> @out; // output buffer
}

// NewEncoder returns an io.Writer that writes lowercase hexadecimal characters to w.
public static io.Writer NewEncoder(io.Writer w) {
    return addr(new encoder(w:w));
}

private static (nint, error) Write(this ptr<encoder> _addr_e, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref encoder e = ref _addr_e.val;

    while (len(p) > 0 && e.err == null) {
        var chunkSize = bufferSize / 2;
        if (len(p) < chunkSize) {
            chunkSize = len(p);
        }
        nint written = default;
        var encoded = Encode(e.@out[..], p[..(int)chunkSize]);
        written, e.err = e.w.Write(e.@out[..(int)encoded]);
        n += written / 2;
        p = p[(int)chunkSize..];

    }
    return (n, error.As(e.err)!);

}

private partial struct decoder {
    public io.Reader r;
    public error err;
    public slice<byte> @in; // input buffer (encoded form)
    public array<byte> arr; // backing array for in
}

// NewDecoder returns an io.Reader that decodes hexadecimal characters from r.
// NewDecoder expects that r contain only an even number of hexadecimal characters.
public static io.Reader NewDecoder(io.Reader r) {
    return addr(new decoder(r:r));
}

private static (nint, error) Read(this ptr<decoder> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref decoder d = ref _addr_d.val;
 
    // Fill internal buffer with sufficient bytes to decode
    if (len(d.@in) < 2 && d.err == null) {
        nint numCopy = default;        nint numRead = default;

        numCopy = copy(d.arr[..], d.@in); // Copies either 0 or 1 bytes
        numRead, d.err = d.r.Read(d.arr[(int)numCopy..]);
        d.@in = d.arr[..(int)numCopy + numRead];
        if (d.err == io.EOF && len(d.@in) % 2 != 0) {
            {
                var (_, ok) = fromHexChar(d.@in[len(d.@in) - 1]);

                if (!ok) {
                    d.err = InvalidByteError(d.@in[len(d.@in) - 1]);
                }
                else
 {
                    d.err = io.ErrUnexpectedEOF;
                }

            }

        }
    }
    {
        var numAvail = len(d.@in) / 2;

        if (len(p) > numAvail) {
            p = p[..(int)numAvail];
        }
    }

    var (numDec, err) = Decode(p, d.@in[..(int)len(p) * 2]);
    d.@in = d.@in[(int)2 * numDec..];
    if (err != null) {
        (d.@in, d.err) = (null, err);
    }
    if (len(d.@in) < 2) {
        return (numDec, error.As(d.err)!); // Only expose errors when buffer fully consumed
    }
    return (numDec, error.As(null!)!);

}

// Dumper returns a WriteCloser that writes a hex dump of all written data to
// w. The format of the dump matches the output of `hexdump -C` on the command
// line.
public static io.WriteCloser Dumper(io.Writer w) {
    return addr(new dumper(w:w));
}

private partial struct dumper {
    public io.Writer w;
    public array<byte> rightChars;
    public array<byte> buf;
    public nint used; // number of bytes in the current line
    public nuint n; // number of bytes, total
    public bool closed;
}

private static byte toChar(byte b) {
    if (b < 32 || b > 126) {
        return '.';
    }
    return b;

}

private static (nint, error) Write(this ptr<dumper> _addr_h, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref dumper h = ref _addr_h.val;

    if (h.closed) {
        return (0, error.As(errors.New("encoding/hex: dumper closed"))!);
    }
    foreach (var (i) in data) {
        if (h.used == 0) { 
            // At the beginning of a line we print the current
            // offset in hex.
            h.buf[0] = byte(h.n >> 24);
            h.buf[1] = byte(h.n >> 16);
            h.buf[2] = byte(h.n >> 8);
            h.buf[3] = byte(h.n);
            Encode(h.buf[(int)4..], h.buf[..(int)4]);
            h.buf[12] = ' ';
            h.buf[13] = ' ';
            _, err = h.w.Write(h.buf[(int)4..]);
            if (err != null) {
                return ;
            }

        }
        Encode(h.buf[..], data[(int)i..(int)i + 1]);
        h.buf[2] = ' ';
        nint l = 3;
        if (h.used == 7) { 
            // There's an additional space after the 8th byte.
            h.buf[3] = ' ';
            l = 4;

        }
        else if (h.used == 15) { 
            // At the end of the line there's an extra space and
            // the bar for the right column.
            h.buf[3] = ' ';
            h.buf[4] = '|';
            l = 5;

        }
        _, err = h.w.Write(h.buf[..(int)l]);
        if (err != null) {
            return ;
        }
        n++;
        h.rightChars[h.used] = toChar(data[i]);
        h.used++;
        h.n++;
        if (h.used == 16) {
            h.rightChars[16] = '|';
            h.rightChars[17] = '\n';
            _, err = h.w.Write(h.rightChars[..]);
            if (err != null) {
                return ;
            }
            h.used = 0;
        }
    }    return ;

}

private static error Close(this ptr<dumper> _addr_h) {
    error err = default!;
    ref dumper h = ref _addr_h.val;
 
    // See the comments in Write() for the details of this format.
    if (h.closed) {
        return ;
    }
    h.closed = true;
    if (h.used == 0) {
        return ;
    }
    h.buf[0] = ' ';
    h.buf[1] = ' ';
    h.buf[2] = ' ';
    h.buf[3] = ' ';
    h.buf[4] = '|';
    var nBytes = h.used;
    while (h.used < 16) {
        nint l = 3;
        if (h.used == 7) {
            l = 4;
        }
        else if (h.used == 15) {
            l = 5;
        }
        _, err = h.w.Write(h.buf[..(int)l]);
        if (err != null) {
            return ;
        }
        h.used++;

    }
    h.rightChars[nBytes] = '|';
    h.rightChars[nBytes + 1] = '\n';
    _, err = h.w.Write(h.rightChars[..(int)nBytes + 2]);
    return ;

}

} // end hex_package
