// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pem implements the PEM data encoding, which originated in Privacy
// Enhanced Mail. The most common use of PEM encoding today is in TLS keys and
// certificates. See RFC 1421.
namespace go.encoding;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;

partial class pem_package {

// A Block represents a PEM encoded structure.
//
// The encoded form is:
//
//	-----BEGIN Type-----
//	Headers
//	base64-encoded Bytes
//	-----END Type-----
//
// where [Block.Headers] is a possibly empty sequence of Key: Value lines.
[GoType] partial struct Block {
    public @string Type;           // The type, taken from the preamble (i.e. "RSA PRIVATE KEY").
    public map<@string, @string> Headers; // Optional headers.
    public slice<byte> Bytes;       // The decoded bytes of the contents. Typically a DER encoded ASN.1 structure.
}

// getLine results the first \r\n or \n delineated line from the given byte
// array. The line does not include trailing whitespace or the trailing new
// line bytes. The remainder of the byte array (also not including the new line
// bytes) is also returned and this will always be smaller than the original
// argument.
internal static (slice<byte> line, slice<byte> rest) getLine(slice<byte> data) {
    slice<byte> line = default!;
    slice<byte> rest = default!;

    nint i = bytes.IndexByte(data, (rune)'\n');
    nint j = default!;
    if (i < 0){
        i = len(data);
        j = i;
    } else {
        j = i + 1;
        if (i > 0 && data[i - 1] == (rune)'\r') {
            i--;
        }
    }
    return (bytes.TrimRight(data[0..(int)(i)], " \t"u8), data[(int)(j)..]);
}

// removeSpacesAndTabs returns a copy of its input with all spaces and tabs
// removed, if there were any. Otherwise, the input is returned unchanged.
//
// The base64 decoder already skips newline characters, so we don't need to
// filter them out here.
internal static slice<byte> removeSpacesAndTabs(slice<byte> data) {
    if (!bytes.ContainsAny(data, " \t"u8)) {
        // Fast path; most base64 data within PEM contains newlines, but
        // no spaces nor tabs. Skip the extra alloc and work.
        return data;
    }
    var result = new slice<byte>(len(data));
    nint n = 0;
    foreach (var (_, b) in data) {
        if (b == (rune)' ' || b == (rune)'\t') {
            continue;
        }
        result[n] = b;
        n++;
    }
    return result[0..(int)(n)];
}

internal static slice<byte> pemStart = slice<byte>("\n-----BEGIN ");

internal static slice<byte> pemEnd = slice<byte>("\n-----END ");

internal static slice<byte> pemEndOfLine = slice<byte>("-----");

internal static slice<byte> colon = slice<byte>(":");

// Decode will find the next PEM formatted block (certificate, private key
// etc) in the input. It returns that block and the remainder of the input. If
// no PEM data is found, p is nil and the whole of the input is returned in
// rest.
public static (ж<Block> p, slice<byte> rest) Decode(slice<byte> data) {
    ж<Block> p = default!;
    slice<byte> rest = default!;

    // pemStart begins with a newline. However, at the very beginning of
    // the byte array, we'll accept the start string without it.
    rest = data;
    while (ᐧ) {
        if (bytes.HasPrefix(rest, pemStart[1..])){
            rest = rest[(int)(len(pemStart) - 1)..];
        } else 
        {
            var (_, after, ok) = bytes.Cut(rest, pemStart); if (ok){
                rest = after;
            } else {
                return (default!, data);
            }
        }
        slice<byte> typeLine = default!;
        (typeLine, rest) = getLine(rest);
        if (!bytes.HasSuffix(typeLine, pemEndOfLine)) {
            continue;
        }
        typeLine = typeLine[0..(int)(len(typeLine) - len(pemEndOfLine))];
        p = Ꮡ(new Block(
            Headers: new map<@string, @string>(),
            Type: ((@string)typeLine)
        ));
        while (ᐧ) {
            // This loop terminates because getLine's second result is
            // always smaller than its argument.
            if (len(rest) == 0) {
                return (default!, data);
            }
            (line, next) = getLine(rest);
            var (key, val, ok) = bytes.Cut(line, colon);
            if (!ok) {
                break;
            }
            // TODO(agl): need to cope with values that spread across lines.
            key = bytes.TrimSpace(key);
            val = bytes.TrimSpace(val);
            (~p).Headers[((@string)key)] = ((@string)val);
            rest = next;
        }
        nint endIndex = default!;
        nint endTrailerIndex = default!;
        // If there were no headers, the END line might occur
        // immediately, without a leading newline.
        if (len((~p).Headers) == 0 && bytes.HasPrefix(rest, pemEnd[1..])){
            endIndex = 0;
            endTrailerIndex = len(pemEnd) - 1;
        } else {
            endIndex = bytes.Index(rest, pemEnd);
            endTrailerIndex = endIndex + len(pemEnd);
        }
        if (endIndex < 0) {
            continue;
        }
        // After the "-----" of the ending line, there should be the same type
        // and then a final five dashes.
        var endTrailer = rest[(int)(endTrailerIndex)..];
        nint endTrailerLen = len(typeLine) + len(pemEndOfLine);
        if (len(endTrailer) < endTrailerLen) {
            continue;
        }
        var restOfEndLine = endTrailer[(int)(endTrailerLen)..];
        endTrailer = endTrailer[..(int)(endTrailerLen)];
        if (!bytes.HasPrefix(endTrailer, typeLine) || !bytes.HasSuffix(endTrailer, pemEndOfLine)) {
            continue;
        }
        // The line must end with only whitespace.
        {
            (s, _) = getLine(restOfEndLine); if (len(s) != 0) {
                continue;
            }
        }
        var base64Data = removeSpacesAndTabs(rest[..(int)(endIndex)]);
        p.val.Bytes = new slice<byte>(base64.StdEncoding.DecodedLen(len(base64Data)));
        var (n, err) = base64.StdEncoding.Decode((~p).Bytes, base64Data);
        if (err != default!) {
            continue;
        }
        p.val.Bytes = (~p).Bytes[..(int)(n)];
        // the -1 is because we might have only matched pemEnd without the
        // leading newline if the PEM block was empty.
        (_, rest) = getLine(rest[(int)(endIndex + len(pemEnd) - 1)..]);
        return (p, rest);
    }
}

internal static readonly UntypedInt pemLineLength = 64;

[GoType] partial struct lineBreaker {
    internal array<byte> line = new(pemLineLength);
    internal nint used;
    internal io_package.Writer @out;
}

internal static slice<byte> nl = new byte[]{(rune)'\n'}.slice();

[GoRecv] internal static (nint n, error err) Write(this ref lineBreaker l, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (l.used + len(b) < pemLineLength) {
        copy(l.line[(int)(l.used)..], b);
        l.used += len(b);
        return (len(b), default!);
    }
    (n, err) = l.@out.Write(l.line[0..(int)(l.used)]);
    if (err != default!) {
        return (n, err);
    }
    nint excess = pemLineLength - l.used;
    l.used = 0;
    (n, err) = l.@out.Write(b[0..(int)(excess)]);
    if (err != default!) {
        return (n, err);
    }
    (n, err) = l.@out.Write(nl);
    if (err != default!) {
        return (n, err);
    }
    return l.Write(b[(int)(excess)..]);
}

[GoRecv] internal static error /*err*/ Close(this ref lineBreaker l) {
    error err = default!;

    if (l.used > 0) {
        (_, err) = l.@out.Write(l.line[0..(int)(l.used)]);
        if (err != default!) {
            return err;
        }
        (_, err) = l.@out.Write(nl);
    }
    return err;
}

internal static error writeHeader(io.Writer @out, @string k, @string v) {
    var (_, err) = @out.Write(slice<byte>(k + ": "u8 + v + "\n"u8));
    return err;
}

// Encode writes the PEM encoding of b to out.
public static error Encode(io.Writer @out, ж<Block> Ꮡb) {
    ref var b = ref Ꮡb.val;

    // Check for invalid block before writing any output.
    foreach (var (k, _) in b.Headers) {
        if (strings.Contains(k, ":"u8)) {
            return errors.New("pem: cannot encode a header key that contains a colon"u8);
        }
    }
    // All errors below are relayed from underlying io.Writer,
    // so it is now safe to write data.
    {
        var (_, errΔ1) = @out.Write(pemStart[1..]); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = @out.Write(slice<byte>(b.Type + "-----\n"u8)); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    if (len(b.Headers) > 0) {
        @string procType = "Proc-Type"u8;
        var h = new slice<@string>(0, len(b.Headers));
        var hasProcType = false;
        foreach (var (k, _) in b.Headers) {
            if (k == procType) {
                hasProcType = true;
                continue;
            }
            h = append(h, k);
        }
        // The Proc-Type header must be written first.
        // See RFC 1421, section 4.6.1.1
        if (hasProcType) {
            {
                var errΔ3 = writeHeader(@out, procType, b.Headers[procType]); if (errΔ3 != default!) {
                    return errΔ3;
                }
            }
        }
        // For consistency of output, write other headers sorted by key.
        slices.Sort(h);
        foreach (var (_, k) in h) {
            {
                var errΔ4 = writeHeader(@out, k, b.Headers[k]); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
        }
        {
            var (_, errΔ5) = @out.Write(nl); if (errΔ5 != default!) {
                return errΔ5;
            }
        }
    }
    ref var breaker = ref heap(new lineBreaker(), out var Ꮡbreaker);
    breaker.@out = @out;
    var b64 = base64.NewEncoder(base64.StdEncoding, ~Ꮡbreaker);
    {
        var (_, errΔ6) = b64.Write(b.Bytes); if (errΔ6 != default!) {
            return errΔ6;
        }
    }
    b64.Close();
    breaker.Close();
    {
        var (_, errΔ7) = @out.Write(pemEnd[1..]); if (errΔ7 != default!) {
            return errΔ7;
        }
    }
    var (_, err) = @out.Write(slice<byte>(b.Type + "-----\n"u8));
    return err;
}

// EncodeToMemory returns the PEM encoding of b.
//
// If b has invalid headers and cannot be encoded,
// EncodeToMemory returns nil. If it is important to
// report details about this error case, use [Encode] instead.
public static slice<byte> EncodeToMemory(ж<Block> Ꮡb) {
    ref var b = ref Ꮡb.val;

    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    {
        var err = Encode(~Ꮡbuf, Ꮡb); if (err != default!) {
            return default!;
        }
    }
    return buf.Bytes();
}

} // end pem_package
