// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2022 March 06 22:25:23 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\stream.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;

namespace go.encoding;

public static partial class json_package {

    // A Decoder reads and decodes JSON values from an input stream.
public partial struct Decoder {
    public io.Reader r;
    public slice<byte> buf;
    public decodeState d;
    public nint scanp; // start of unread data in buf
    public long scanned; // amount of data already scanned
    public scanner scan;
    public error err;
    public nint tokenState;
    public slice<nint> tokenStack;
}

// NewDecoder returns a new decoder that reads from r.
//
// The decoder introduces its own buffering and may
// read data from r beyond the JSON values requested.
public static ptr<Decoder> NewDecoder(io.Reader r) {
    return addr(new Decoder(r:r));
}

// UseNumber causes the Decoder to unmarshal a number into an interface{} as a
// Number instead of as a float64.
private static void UseNumber(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    dec.d.useNumber = true;
}

// DisallowUnknownFields causes the Decoder to return an error when the destination
// is a struct and the input contains object keys which do not match any
// non-ignored, exported fields in the destination.
private static void DisallowUnknownFields(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    dec.d.disallowUnknownFields = true;
}

// Decode reads the next JSON-encoded value from its
// input and stores it in the value pointed to by v.
//
// See the documentation for Unmarshal for details about
// the conversion of JSON into a Go value.
private static error Decode(this ptr<Decoder> _addr_dec, object v) {
    ref Decoder dec = ref _addr_dec.val;

    if (dec.err != null) {
        return error.As(dec.err)!;
    }
    {
        var err = dec.tokenPrepareForDecode();

        if (err != null) {
            return error.As(err)!;
        }
    }


    if (!dec.tokenValueAllowed()) {
        return error.As(addr(new SyntaxError(msg:"not at beginning of value",Offset:dec.InputOffset()))!)!;
    }
    var (n, err) = dec.readValue();
    if (err != null) {
        return error.As(err)!;
    }
    dec.d.init(dec.buf[(int)dec.scanp..(int)dec.scanp + n]);
    dec.scanp += n; 

    // Don't save err from unmarshal into dec.err:
    // the connection is still usable since we read a complete JSON
    // object from it before the error happened.
    err = dec.d.unmarshal(v); 

    // fixup token streaming state
    dec.tokenValueEnd();

    return error.As(err)!;

}

// Buffered returns a reader of the data remaining in the Decoder's
// buffer. The reader is valid until the next call to Decode.
private static io.Reader Buffered(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    return bytes.NewReader(dec.buf[(int)dec.scanp..]);
}

// readValue reads a JSON value into dec.buf.
// It returns the length of the encoding.
private static (nint, error) readValue(this ptr<Decoder> _addr_dec) {
    nint _p0 = default;
    error _p0 = default!;
    ref Decoder dec = ref _addr_dec.val;

    dec.scan.reset();

    var scanp = dec.scanp;
    error err = default!;
Input:
    while (scanp >= 0) {
        // Look in the buffer for a new value.
        while (scanp < len(dec.buf)) {
            var c = dec.buf[scanp];
            dec.scan.bytes++;

            if (dec.scan.step(_addr_dec.scan, c) == scanEnd) 
                // scanEnd is delayed one byte so we decrement
                // the scanner bytes count by 1 to ensure that
                // this value is correct in the next call of Decode.
                dec.scan.bytes--;
                _breakInput = true;
                break;
            else if (dec.scan.step(_addr_dec.scan, c) == scanEndObject || dec.scan.step(_addr_dec.scan, c) == scanEndArray) 
                // scanEnd is delayed one byte.
                // We might block trying to get that byte from src,
                // so instead invent a space byte.
                if (stateEndValue(_addr_dec.scan, ' ') == scanEnd) {
                    scanp++;
                    _breakInput = true;
                    break;
            scanp++;
                }

            else if (dec.scan.step(_addr_dec.scan, c) == scanError) 
                dec.err = dec.scan.err;
                return (0, error.As(dec.scan.err)!);
            
        } 

        // Did the last read have an error?
        // Delayed until now to allow buffer scan.
        if (err != null) {
            if (err == io.EOF) {
                if (dec.scan.step(_addr_dec.scan, ' ') == scanEnd) {
                    _breakInput = true;
                    break;
                }

                if (nonSpace(dec.buf)) {
                    err = error.As(io.ErrUnexpectedEOF)!;
                }

            }

            dec.err = err;
            return (0, error.As(err)!);

        }
        var n = scanp - dec.scanp;
        err = error.As(dec.refill())!;
        scanp = dec.scanp + n;

    }
    return (scanp - dec.scanp, error.As(null!)!);

}

private static error refill(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;
 
    // Make room to read more into the buffer.
    // First slide down data already consumed.
    if (dec.scanp > 0) {
        dec.scanned += int64(dec.scanp);
        var n = copy(dec.buf, dec.buf[(int)dec.scanp..]);
        dec.buf = dec.buf[..(int)n];
        dec.scanp = 0;
    }
    const nint minRead = 512;

    if (cap(dec.buf) - len(dec.buf) < minRead) {
        var newBuf = make_slice<byte>(len(dec.buf), 2 * cap(dec.buf) + minRead);
        copy(newBuf, dec.buf);
        dec.buf = newBuf;
    }
    var (n, err) = dec.r.Read(dec.buf[(int)len(dec.buf)..(int)cap(dec.buf)]);
    dec.buf = dec.buf[(int)0..(int)len(dec.buf) + n];

    return error.As(err)!;

}

private static bool nonSpace(slice<byte> b) {
    foreach (var (_, c) in b) {
        if (!isSpace(c)) {
            return true;
        }
    }    return false;

}

// An Encoder writes JSON values to an output stream.
public partial struct Encoder {
    public io.Writer w;
    public error err;
    public bool escapeHTML;
    public ptr<bytes.Buffer> indentBuf;
    public @string indentPrefix;
    public @string indentValue;
}

// NewEncoder returns a new encoder that writes to w.
public static ptr<Encoder> NewEncoder(io.Writer w) {
    return addr(new Encoder(w:w,escapeHTML:true));
}

// Encode writes the JSON encoding of v to the stream,
// followed by a newline character.
//
// See the documentation for Marshal for details about the
// conversion of Go values to JSON.
private static error Encode(this ptr<Encoder> _addr_enc, object v) {
    ref Encoder enc = ref _addr_enc.val;

    if (enc.err != null) {
        return error.As(enc.err)!;
    }
    var e = newEncodeState();
    var err = e.marshal(v, new encOpts(escapeHTML:enc.escapeHTML));
    if (err != null) {
        return error.As(err)!;
    }
    e.WriteByte('\n');

    var b = e.Bytes();
    if (enc.indentPrefix != "" || enc.indentValue != "") {
        if (enc.indentBuf == null) {
            enc.indentBuf = @new<bytes.Buffer>();
        }
        enc.indentBuf.Reset();
        err = Indent(enc.indentBuf, b, enc.indentPrefix, enc.indentValue);
        if (err != null) {
            return error.As(err)!;
        }
        b = enc.indentBuf.Bytes();

    }
    _, err = enc.w.Write(b);

    if (err != null) {
        enc.err = err;
    }
    encodeStatePool.Put(e);
    return error.As(err)!;

}

// SetIndent instructs the encoder to format each subsequent encoded
// value as if indented by the package-level function Indent(dst, src, prefix, indent).
// Calling SetIndent("", "") disables indentation.
private static void SetIndent(this ptr<Encoder> _addr_enc, @string prefix, @string indent) {
    ref Encoder enc = ref _addr_enc.val;

    enc.indentPrefix = prefix;
    enc.indentValue = indent;
}

// SetEscapeHTML specifies whether problematic HTML characters
// should be escaped inside JSON quoted strings.
// The default behavior is to escape &, <, and > to \u0026, \u003c, and \u003e
// to avoid certain safety problems that can arise when embedding JSON in HTML.
//
// In non-HTML settings where the escaping interferes with the readability
// of the output, SetEscapeHTML(false) disables this behavior.
private static void SetEscapeHTML(this ptr<Encoder> _addr_enc, bool on) {
    ref Encoder enc = ref _addr_enc.val;

    enc.escapeHTML = on;
}

// RawMessage is a raw encoded JSON value.
// It implements Marshaler and Unmarshaler and can
// be used to delay JSON decoding or precompute a JSON encoding.
public partial struct RawMessage { // : slice<byte>
}

// MarshalJSON returns m as the JSON encoding of m.
public static (slice<byte>, error) MarshalJSON(this RawMessage m) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (m == null) {
        return ((slice<byte>)"null", error.As(null!)!);
    }
    return (m, error.As(null!)!);

}

// UnmarshalJSON sets *m to a copy of data.
private static error UnmarshalJSON(this ptr<RawMessage> _addr_m, slice<byte> data) {
    ref RawMessage m = ref _addr_m.val;

    if (m == null) {
        return error.As(errors.New("json.RawMessage: UnmarshalJSON on nil pointer"))!;
    }
    m.val = append((m.val)[(int)0..(int)0], data);
    return error.As(null!)!;

}

private static Marshaler _ = (RawMessage.val)(null);
private static Unmarshaler _ = (RawMessage.val)(null);

// A Token holds a value of one of these types:
//
//    Delim, for the four JSON delimiters [ ] { }
//    bool, for JSON booleans
//    float64, for JSON numbers
//    Number, for JSON numbers
//    string, for JSON string literals
//    nil, for JSON null
//
public partial interface Token {
}

private static readonly var tokenTopValue = iota;
private static readonly var tokenArrayStart = 0;
private static readonly var tokenArrayValue = 1;
private static readonly var tokenArrayComma = 2;
private static readonly var tokenObjectStart = 3;
private static readonly var tokenObjectKey = 4;
private static readonly var tokenObjectColon = 5;
private static readonly var tokenObjectValue = 6;
private static readonly var tokenObjectComma = 7;


// advance tokenstate from a separator state to a value state
private static error tokenPrepareForDecode(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;
 
    // Note: Not calling peek before switch, to avoid
    // putting peek into the standard Decode path.
    // peek is only called when using the Token API.

    if (dec.tokenState == tokenArrayComma) 
        var (c, err) = dec.peek();
        if (err != null) {
            return error.As(err)!;
        }
        if (c != ',') {
            return error.As(addr(new SyntaxError("expected comma after array element",dec.InputOffset()))!)!;
        }
        dec.scanp++;
        dec.tokenState = tokenArrayValue;
    else if (dec.tokenState == tokenObjectColon) 
        (c, err) = dec.peek();
        if (err != null) {
            return error.As(err)!;
        }
        if (c != ':') {
            return error.As(addr(new SyntaxError("expected colon after object key",dec.InputOffset()))!)!;
        }
        dec.scanp++;
        dec.tokenState = tokenObjectValue;
        return error.As(null!)!;

}

private static bool tokenValueAllowed(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;


    if (dec.tokenState == tokenTopValue || dec.tokenState == tokenArrayStart || dec.tokenState == tokenArrayValue || dec.tokenState == tokenObjectValue) 
        return true;
        return false;

}

private static void tokenValueEnd(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;


    if (dec.tokenState == tokenArrayStart || dec.tokenState == tokenArrayValue) 
        dec.tokenState = tokenArrayComma;
    else if (dec.tokenState == tokenObjectValue) 
        dec.tokenState = tokenObjectComma;
    
}

// A Delim is a JSON array or object delimiter, one of [ ] { or }.
public partial struct Delim { // : int
}

public static @string String(this Delim d) {
    return string(d);
}

// Token returns the next JSON token in the input stream.
// At the end of the input stream, Token returns nil, io.EOF.
//
// Token guarantees that the delimiters [ ] { } it returns are
// properly nested and matched: if Token encounters an unexpected
// delimiter in the input, it will return an error.
//
// The input stream consists of basic JSON values—bool, string,
// number, and null—along with delimiters [ ] { } of type Delim
// to mark the start and end of arrays and objects.
// Commas and colons are elided.
private static (Token, error) Token(this ptr<Decoder> _addr_dec) {
    Token _p0 = default;
    error _p0 = default!;
    ref Decoder dec = ref _addr_dec.val;

    while (true) {
        var (c, err) = dec.peek();
        if (err != null) {
            return (null, error.As(err)!);
        }

        if (c == '[')
        {
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenStack = append(dec.tokenStack, dec.tokenState);
            dec.tokenState = tokenArrayStart;
            return (Delim('['), error.As(null!)!);
            goto __switch_break0;
        }
        if (c == ']')
        {
            if (dec.tokenState != tokenArrayStart && dec.tokenState != tokenArrayComma) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = dec.tokenStack[len(dec.tokenStack) - 1];
            dec.tokenStack = dec.tokenStack[..(int)len(dec.tokenStack) - 1];
            dec.tokenValueEnd();
            return (Delim(']'), error.As(null!)!);
            goto __switch_break0;
        }
        if (c == '{')
        {
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenStack = append(dec.tokenStack, dec.tokenState);
            dec.tokenState = tokenObjectStart;
            return (Delim('{'), error.As(null!)!);
            goto __switch_break0;
        }
        if (c == '}')
        {
            if (dec.tokenState != tokenObjectStart && dec.tokenState != tokenObjectComma) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = dec.tokenStack[len(dec.tokenStack) - 1];
            dec.tokenStack = dec.tokenStack[..(int)len(dec.tokenStack) - 1];
            dec.tokenValueEnd();
            return (Delim('}'), error.As(null!)!);
            goto __switch_break0;
        }
        if (c == ':')
        {
            if (dec.tokenState != tokenObjectColon) {
                return dec.tokenError(c);
            }
            dec.scanp++;
            dec.tokenState = tokenObjectValue;
            continue;
            goto __switch_break0;
        }
        if (c == ',')
        {
            if (dec.tokenState == tokenArrayComma) {
                dec.scanp++;
                dec.tokenState = tokenArrayValue;
                continue;
            }
            if (dec.tokenState == tokenObjectComma) {
                dec.scanp++;
                dec.tokenState = tokenObjectKey;
                continue;
            }
            return dec.tokenError(c);
            goto __switch_break0;
        }
        if (c == '"')
        {
            if (dec.tokenState == tokenObjectStart || dec.tokenState == tokenObjectKey) {
                ref @string x = ref heap(out ptr<@string> _addr_x);
                var old = dec.tokenState;
                dec.tokenState = tokenTopValue;
                var err = dec.Decode(_addr_x);
                dec.tokenState = old;
                if (err != null) {
                    return (null, error.As(err)!);
                }
                dec.tokenState = tokenObjectColon;
                return (x, error.As(null!)!);
            }
        }
        // default: 
            if (!dec.tokenValueAllowed()) {
                return dec.tokenError(c);
            }
            x = default;
            {
                var err__prev1 = err;

                err = dec.Decode(_addr_x);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            return (x, error.As(null!)!);

        __switch_break0:;

    }

}

private static (Token, error) tokenError(this ptr<Decoder> _addr_dec, byte c) {
    Token _p0 = default;
    error _p0 = default!;
    ref Decoder dec = ref _addr_dec.val;

    @string context = default;

    if (dec.tokenState == tokenTopValue) 
        context = " looking for beginning of value";
    else if (dec.tokenState == tokenArrayStart || dec.tokenState == tokenArrayValue || dec.tokenState == tokenObjectValue) 
        context = " looking for beginning of value";
    else if (dec.tokenState == tokenArrayComma) 
        context = " after array element";
    else if (dec.tokenState == tokenObjectKey) 
        context = " looking for beginning of object key string";
    else if (dec.tokenState == tokenObjectColon) 
        context = " after object key";
    else if (dec.tokenState == tokenObjectComma) 
        context = " after object key:value pair";
        return (null, error.As(addr(new SyntaxError("invalid character "+quoteChar(c)+context,dec.InputOffset()))!)!);

}

// More reports whether there is another element in the
// current array or object being parsed.
private static bool More(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    var (c, err) = dec.peek();
    return err == null && c != ']' && c != '}';
}

private static (byte, error) peek(this ptr<Decoder> _addr_dec) {
    byte _p0 = default;
    error _p0 = default!;
    ref Decoder dec = ref _addr_dec.val;

    error err = default!;
    while (true) {
        for (var i = dec.scanp; i < len(dec.buf); i++) {
            var c = dec.buf[i];
            if (isSpace(c)) {
                continue;
            }
            dec.scanp = i;
            return (c, error.As(null!)!);
        } 
        // buffer has been scanned, now report any error
        if (err != null) {
            return (0, error.As(err)!);
        }
        err = error.As(dec.refill())!;

    }

}

// InputOffset returns the input stream byte offset of the current decoder position.
// The offset gives the location of the end of the most recently returned token
// and the beginning of the next token.
private static long InputOffset(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    return dec.scanned + int64(dec.scanp);
}

} // end json_package
