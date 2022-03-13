// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package quotedprintable implements quoted-printable encoding as specified by
// RFC 2045.

// package quotedprintable -- go2cs converted at 2022 March 13 05:36:22 UTC
// import "mime/quotedprintable" ==> using quotedprintable = go.mime.quotedprintable_package
// Original source: C:\Program Files\Go\src\mime\quotedprintable\reader.go
namespace go.mime;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;


// Reader is a quoted-printable decoder.

public static partial class quotedprintable_package {

public partial struct Reader {
    public ptr<bufio.Reader> br;
    public error rerr; // last read error
    public slice<byte> line; // to be consumed before more of br
}

// NewReader returns a quoted-printable reader, decoding from r.
public static ptr<Reader> NewReader(io.Reader r) {
    return addr(new Reader(br:bufio.NewReader(r),));
}

private static (byte, error) fromHex(byte b) {
    byte _p0 = default;
    error _p0 = default!;


    if (b >= '0' && b <= '9') 
        return (b - '0', error.As(null!)!);
    else if (b >= 'A' && b <= 'F') 
        return (b - 'A' + 10, error.As(null!)!); 
        // Accept badly encoded bytes.
    else if (b >= 'a' && b <= 'f') 
        return (b - 'a' + 10, error.As(null!)!);
        return (0, error.As(fmt.Errorf("quotedprintable: invalid hex byte 0x%02x", b))!);
}

private static (byte, error) readHexByte(slice<byte> v) {
    byte b = default;
    error err = default!;

    if (len(v) < 2) {
        return (0, error.As(io.ErrUnexpectedEOF)!);
    }
    byte hb = default;    byte lb = default;

    hb, err = fromHex(v[0]);

    if (err != null) {
        return (0, error.As(err)!);
    }
    lb, err = fromHex(v[1]);

    if (err != null) {
        return (0, error.As(err)!);
    }
    return (hb << 4 | lb, error.As(null!)!);
}

private static bool isQPDiscardWhitespace(int r) {
    switch (r) {
        case '\n': 

        case '\r': 

        case ' ': 

        case '\t': 
            return true;
            break;
    }
    return false;
}

private static slice<byte> crlf = (slice<byte>)"\r\n";private static slice<byte> lf = (slice<byte>)"\n";private static slice<byte> softSuffix = (slice<byte>)"=";

// Read reads and decodes quoted-printable data from the underlying reader.
private static (nint, error) Read(this ptr<Reader> _addr_r, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;
 
    // Deviations from RFC 2045:
    // 1. in addition to "=\r\n", "=\n" is also treated as soft line break.
    // 2. it will pass through a '\r' or '\n' not preceded by '=', consistent
    //    with other broken QP encoders & decoders.
    // 3. it accepts soft line-break (=) at end of message (issue 15486); i.e.
    //    the final byte read from the underlying reader is allowed to be '=',
    //    and it will be silently ignored.
    // 4. it takes = as literal = if not followed by two hex digits
    //    but not at end of line (issue 13219).
    while (len(p) > 0) {
        if (len(r.line) == 0) {
            if (r.rerr != null) {
                return (n, error.As(r.rerr)!);
            }
            r.line, r.rerr = r.br.ReadSlice('\n'); 

            // Does the line end in CRLF instead of just LF?
            var hasLF = bytes.HasSuffix(r.line, lf);
            var hasCR = bytes.HasSuffix(r.line, crlf);
            var wholeLine = r.line;
            r.line = bytes.TrimRightFunc(wholeLine, isQPDiscardWhitespace);
            if (bytes.HasSuffix(r.line, softSuffix)) {
                var rightStripped = wholeLine[(int)len(r.line)..];
                r.line = r.line[..(int)len(r.line) - 1];
                if (!bytes.HasPrefix(rightStripped, lf) && !bytes.HasPrefix(rightStripped, crlf) && !(len(rightStripped) == 0 && len(r.line) > 0 && r.rerr == io.EOF)) {
                    r.rerr = fmt.Errorf("quotedprintable: invalid bytes after =: %q", rightStripped);
                }
            }
            else if (hasLF) {
                if (hasCR) {
                    r.line = append(r.line, '\r', '\n');
                }
                else
 {
                    r.line = append(r.line, '\n');
                }
            }
            continue;
        }
        var b = r.line[0];


        if (b == '=') 
            b, err = readHexByte(r.line[(int)1..]);
            if (err != null) {
                if (len(r.line) >= 2 && r.line[1] != '\r' && r.line[1] != '\n') { 
                    // Take the = as a literal =.
                    b = '=';
                    break;
                }
                return (n, error.As(err)!);
            }
            r.line = r.line[(int)2..]; // 2 of the 3; other 1 is done below
        else if (b == '\t' || b == '\r' || b == '\n') 
            break;
        else if (b >= 0x80) 
            // As an extension to RFC 2045, we accept
            // values >= 0x80 without complaint. Issue 22597.
            break;
        else if (b < ' ' || b > '~') 
            return (n, error.As(fmt.Errorf("quotedprintable: invalid unescaped byte 0x%02x in body", b))!);
                p[0] = b;
        p = p[(int)1..];
        r.line = r.line[(int)1..];
        n++;
    }
    return (n, error.As(null!)!);
}

} // end quotedprintable_package
