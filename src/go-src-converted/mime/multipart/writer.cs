// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using rand = crypto.rand_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using textproto = net.textproto_package;
using slices = slices_package;
using strings = strings_package;
using crypto;
using net;

partial class multipart_package {

// A Writer generates multipart messages.
[GoType] partial struct Writer {
    internal io_package.Writer w;
    internal @string boundary;
    internal ж<part> lastpart;
}

// NewWriter returns a new multipart [Writer] with a random boundary,
// writing to w.
public static ж<Writer> NewWriter(io.Writer w) {
    return Ꮡ(new Writer(
        w: w,
        boundary: randomBoundary()
    ));
}

// Boundary returns the [Writer]'s boundary.
[GoRecv] public static @string Boundary(this ref Writer w) {
    return w.boundary;
}

// SetBoundary overrides the [Writer]'s default randomly-generated
// boundary separator with an explicit value.
//
// SetBoundary must be called before any parts are created, may only
// contain certain ASCII characters, and must be non-empty and
// at most 70 bytes long.
[GoRecv] public static error SetBoundary(this ref Writer w, @string boundary) {
    if (w.lastpart != nil) {
        return errors.New("mime: SetBoundary called after write"u8);
    }
    // rfc2046#section-5.1.1
    if (len(boundary) < 1 || len(boundary) > 70) {
        return errors.New("mime: invalid boundary length"u8);
    }
    nint end = len(boundary) - 1;
    foreach (var (i, b) in boundary) {
        if ((rune)'A' <= b && b <= (rune)'Z' || (rune)'a' <= b && b <= (rune)'z' || (rune)'0' <= b && b <= (rune)'9') {
            continue;
        }
        switch (b) {
        case (rune)'\'' or (rune)'(' or (rune)')' or (rune)'+' or (rune)'_' or (rune)',' or (rune)'-' or (rune)'.' or (rune)'/' or (rune)':' or (rune)'=' or (rune)'?': {
            continue;
            break;
        }
        case (rune)' ': {
            if (i != end) {
                continue;
            }
            break;
        }}

        return errors.New("mime: invalid boundary character"u8);
    }
    w.boundary = boundary;
    return default!;
}

// FormDataContentType returns the Content-Type for an HTTP
// multipart/form-data with this [Writer]'s Boundary.
[GoRecv] public static @string FormDataContentType(this ref Writer w) {
    @string b = w.boundary;
    // We must quote the boundary if it contains any of the
    // tspecials characters defined by RFC 2045, or space.
    if (strings.ContainsAny(b, @"()<>@,;:\""/[]?= "u8)) {
        b = @""""u8 + b + @""""u8;
    }
    return "multipart/form-data; boundary="u8 + b;
}

internal static @string randomBoundary() {
    array<byte> buf = new(30);
    var (_, err) = io.ReadFull(rand.Reader, buf[..]);
    if (err != default!) {
        throw panic(err);
    }
    return fmt.Sprintf("%x"u8, buf[..]);
}

// CreatePart creates a new multipart section with the provided
// header. The body of the part should be written to the returned
// [Writer]. After calling CreatePart, any previous part may no longer
// be written to.
[GoRecv] public static (io.Writer, error) CreatePart(this ref Writer w, textproto.MIMEHeader header) {
    if (w.lastpart != nil) {
        {
            var errΔ1 = w.lastpart.close(); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    ref var b = ref heap(new bytes_package.Buffer(), out var Ꮡb);
    if (w.lastpart != nil){
        fmt.Fprintf(~Ꮡb, "\r\n--%s\r\n"u8, w.boundary);
    } else {
        fmt.Fprintf(~Ꮡb, "--%s\r\n"u8, w.boundary);
    }
    var keys = new slice<@string>(0, len(header));
    foreach (var (k, _) in header) {
        keys = append(keys, k);
    }
    slices.Sort(keys);
    foreach (var (_, k) in keys) {
        foreach (var (_, v) in header[k]) {
            fmt.Fprintf(~Ꮡb, "%s: %s\r\n"u8, k, v);
        }
    }
    fmt.Fprintf(~Ꮡb, "\r\n"u8);
    var (_, err) = io.Copy(w.w, ~Ꮡb);
    if (err != default!) {
        return (default!, err);
    }
    var p = Ꮡ(new part(
        mw: w
    ));
    w.lastpart = p;
    return (~p, default!);
}

internal static ж<strings.Replacer> quoteEscaper = strings.NewReplacer("\\"u8, "\\\\", @"""", "\\\"");

internal static @string escapeQuotes(@string s) {
    return quoteEscaper.Replace(s);
}

// CreateFormFile is a convenience wrapper around [Writer.CreatePart]. It creates
// a new form-data header with the provided field name and file name.
[GoRecv] public static (io.Writer, error) CreateFormFile(this ref Writer w, @string fieldname, @string filename) {
    var h = new textproto.MIMEHeader();
    h.Set("Content-Disposition"u8,
        fmt.Sprintf(@"form-data; name=""%s""; filename=""%s"""u8,
            escapeQuotes(fieldname), escapeQuotes(filename)));
    h.Set("Content-Type"u8, "application/octet-stream"u8);
    return w.CreatePart(h);
}

// CreateFormField calls [Writer.CreatePart] with a header using the
// given field name.
[GoRecv] public static (io.Writer, error) CreateFormField(this ref Writer w, @string fieldname) {
    var h = new textproto.MIMEHeader();
    h.Set("Content-Disposition"u8,
        fmt.Sprintf(@"form-data; name=""%s"""u8, escapeQuotes(fieldname)));
    return w.CreatePart(h);
}

// WriteField calls [Writer.CreateFormField] and then writes the given value.
[GoRecv] public static error WriteField(this ref Writer w, @string fieldname, @string value) {
    (p, err) = w.CreateFormField(fieldname);
    if (err != default!) {
        return err;
    }
    (_, err) = p.Write(slice<byte>(value));
    return err;
}

// Close finishes the multipart message and writes the trailing
// boundary end line to the output.
[GoRecv] public static error Close(this ref Writer w) {
    if (w.lastpart != nil) {
        {
            var errΔ1 = w.lastpart.close(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        w.lastpart = default!;
    }
    var (_, err) = fmt.Fprintf(w.w, "\r\n--%s--\r\n"u8, w.boundary);
    return err;
}

[GoType] partial struct part {
    internal ж<Writer> mw;
    internal bool closed;
    internal error we; // last error that occurred writing
}

[GoRecv] internal static error close(this ref part p) {
    p.closed = true;
    return p.we;
}

[GoRecv] internal static (nint n, error err) Write(this ref part p, slice<byte> d) {
    nint n = default!;
    error err = default!;

    if (p.closed) {
        return (0, errors.New("multipart: can't write to finished part"u8));
    }
    (n, err) = p.mw.w.Write(d);
    if (err != default!) {
        p.we = err;
    }
    return (n, err);
}

} // end multipart_package
