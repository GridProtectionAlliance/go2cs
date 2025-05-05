// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fcgi implements the FastCGI protocol.
//
// See https://fast-cgi.github.io/ for an unofficial mirror of the
// original documentation.
//
// Currently only the responder role is supported.
namespace go.net.http;

// This file defines the raw protocol and some utilities used by the child and
// the host.
using bufio = bufio_package;
using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;
using encoding;

partial class fcgi_package {

[GoType("num:uint8")] partial struct recType;

internal static readonly recType typeBeginRequest = 1;
internal static readonly recType typeAbortRequest = 2;
internal static readonly recType typeEndRequest = 3;
internal static readonly recType typeParams = 4;
internal static readonly recType typeStdin = 5;
internal static readonly recType typeStdout = 6;
internal static readonly recType typeStderr = 7;
internal static readonly recType typeData = 8;
internal static readonly recType typeGetValues = 9;
internal static readonly recType typeGetValuesResult = 10;
internal static readonly recType typeUnknownType = 11;

// keep the connection between web-server and responder open after request
internal static readonly UntypedInt flagKeepConn = 1;

internal static readonly UntypedInt maxWrite = 65535; // maximum record body
internal static readonly UntypedInt maxPad = 255;

internal static readonly UntypedInt roleResponder = /* iota + 1 */ 1; // only Responders are implemented.
internal static readonly UntypedInt roleAuthorizer = 2;
internal static readonly UntypedInt roleFilter = 3;

internal static readonly UntypedInt statusRequestComplete = iota;
internal static readonly UntypedInt statusCantMultiplex = 1;
internal static readonly UntypedInt statusOverloaded = 2;
internal static readonly UntypedInt statusUnknownRole = 3;

[GoType] partial struct header {
    public uint8 Version;
    public recType Type;
    public uint16 Id;
    public uint16 ContentLength;
    public uint8 PaddingLength;
    public uint8 Reserved;
}

[GoType] partial struct beginRequest {
    internal uint16 role;
    internal uint8 flags;
    internal array<uint8> reserved = new(5);
}

[GoRecv] internal static error read(this ref beginRequest br, slice<byte> content) {
    if (len(content) != 8) {
        return errors.New("fcgi: invalid begin request record"u8);
    }
    br.role = binary.BigEndian.Uint16(content);
    br.flags = content[2];
    return default!;
}

// for padding so we don't have to allocate all the time
// not synchronized because we don't care what the contents are
internal static array<byte> pad;

[GoRecv] internal static void init(this ref header h, recType recType, uint16 reqId, nint contentLength) {
    h.Version = 1;
    h.Type = recType;
    h.Id = reqId;
    h.ContentLength = ((uint16)contentLength);
    h.PaddingLength = ((uint8)((nint)(-contentLength & 7)));
}

// conn sends records over rwc
[GoType] partial struct conn {
    internal sync_package.Mutex mutex;
    internal io_package.ReadWriteCloser rwc;
    internal error closeErr;
    internal bool closed;
    // to avoid allocations
    internal bytes_package.Buffer buf;
    internal header h;
}

internal static ж<conn> newConn(io.ReadWriteCloser rwc) {
    return Ꮡ(new conn(rwc: rwc));
}

// Close closes the conn if it is not already closed.
[GoRecv] internal static error Close(this ref conn c) => func((defer, _) => {
    c.mutex.Lock();
    defer(c.mutex.Unlock);
    if (!c.closed) {
        c.closeErr = c.rwc.Close();
        c.closed = true;
    }
    return c.closeErr;
});

[GoType] partial struct record {
    internal header h;
    internal array<byte> buf = new(maxWrite + maxPad);
}

[GoRecv] internal static error /*err*/ read(this ref record rec, io.Reader r) {
    error err = default!;

    {
        err = binary.Read(r, binary.BigEndian, Ꮡ(rec.h)); if (err != default!) {
            return err;
        }
    }
    if (rec.h.Version != 1) {
        return errors.New("fcgi: invalid header version"u8);
    }
    nint n = ((nint)rec.h.ContentLength) + ((nint)rec.h.PaddingLength);
    {
        (_, err) = io.ReadFull(r, rec.buf[..(int)(n)]); if (err != default!) {
            return err;
        }
    }
    return default!;
}

[GoRecv] internal static slice<byte> content(this ref record r) {
    return r.buf[..(int)(r.h.ContentLength)];
}

// writeRecord writes and sends a single record.
[GoRecv] internal static error writeRecord(this ref conn c, recType recType, uint16 reqId, slice<byte> b) => func((defer, _) => {
    c.mutex.Lock();
    defer(c.mutex.Unlock);
    c.buf.Reset();
    c.h.init(recType, reqId, len(b));
    {
        var errΔ1 = binary.Write(c.buf, binary.BigEndian, c.h); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var (_, errΔ2) = c.buf.Write(b); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    {
        var (_, errΔ3) = c.buf.Write(pad[..(int)(c.h.PaddingLength)]); if (errΔ3 != default!) {
            return errΔ3;
        }
    }
    var (_, err) = c.rwc.Write(c.buf.Bytes());
    return err;
});

[GoRecv] internal static error writeEndRequest(this ref conn c, uint16 reqId, nint appStatus, uint8 protocolStatus) {
    var b = new slice<byte>(8);
    binary.BigEndian.PutUint32(b, ((uint32)appStatus));
    b[4] = protocolStatus;
    return c.writeRecord(typeEndRequest, reqId, b);
}

[GoRecv] internal static error writePairs(this ref conn c, recType recType, uint16 reqId, map<@string, @string> pairs) {
    var w = newWriter(c, recType, reqId);
    var b = new slice<byte>(8);
    foreach (var (k, v) in pairs) {
        nint n = encodeSize(b, ((uint32)len(k)));
        n += encodeSize(b[(int)(n)..], ((uint32)len(v)));
        {
            var (_, err) = w.Write(b[..(int)(n)]); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = w.WriteString(k); if (err != default!) {
                return err;
            }
        }
        {
            var (_, err) = w.WriteString(v); if (err != default!) {
                return err;
            }
        }
    }
    w.Close();
    return default!;
}

internal static (uint32, nint) readSize(slice<byte> s) {
    if (len(s) == 0) {
        return (0, 0);
    }
    var size = ((uint32)s[0]);
    nint n = 1;
    if ((uint32)(size & (1 << (int)(7))) != 0) {
        if (len(s) < 4) {
            return (0, 0);
        }
        n = 4;
        size = binary.BigEndian.Uint32(s);
        size &= ~(uint32)(1 << (int)(31));
    }
    return (size, n);
}

internal static @string readString(slice<byte> s, uint32 size) {
    if (size > ((uint32)len(s))) {
        return ""u8;
    }
    return ((@string)(s[..(int)(size)]));
}

internal static nint encodeSize(slice<byte> b, uint32 size) {
    if (size > 127) {
        size |= (uint32)(1 << (int)(31));
        binary.BigEndian.PutUint32(b, size);
        return 4;
    }
    b[0] = ((byte)size);
    return 1;
}

// bufWriter encapsulates bufio.Writer but also closes the underlying stream when
// Closed.
[GoType] partial struct bufWriter {
    internal io_package.Closer closer;
    public partial ref ж<bufio_package.Writer> Writer { get; }
}

[GoRecv] internal static error Close(this ref bufWriter w) {
    {
        var err = w.Writer.Flush(); if (err != default!) {
            w.closer.Close();
            return err;
        }
    }
    return w.closer.Close();
}

internal static ж<bufWriter> newWriter(ж<conn> Ꮡc, recType recType, uint16 reqId) {
    ref var c = ref Ꮡc.val;

    var s = Ꮡ(new streamWriter(c: c, recType: recType, reqId: reqId));
    var w = bufio.NewWriterSize(~s, maxWrite);
    return Ꮡ(new bufWriter(s, w));
}

// streamWriter abstracts out the separation of a stream into discrete records.
// It only writes maxWrite bytes at a time.
[GoType] partial struct streamWriter {
    internal ж<conn> c;
    internal recType recType;
    internal uint16 reqId;
}

[GoRecv] internal static (nint, error) Write(this ref streamWriter w, slice<byte> p) {
    nint nn = 0;
    while (len(p) > 0) {
        nint n = len(p);
        if (n > maxWrite) {
            n = maxWrite;
        }
        {
            var err = w.c.writeRecord(w.recType, w.reqId, p[..(int)(n)]); if (err != default!) {
                return (nn, err);
            }
        }
        nn += n;
        p = p[(int)(n)..];
    }
    return (nn, default!);
}

[GoRecv] internal static error Close(this ref streamWriter w) {
    // send empty record to close the stream
    return w.c.writeRecord(w.recType, w.reqId, default!);
}

} // end fcgi_package
