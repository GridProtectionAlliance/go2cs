// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fcgi implements the FastCGI protocol.
//
// See https://fast-cgi.github.io/ for an unofficial mirror of the
// original documentation.
//
// Currently only the responder role is supported.

// package fcgi -- go2cs converted at 2022 March 13 05:38:18 UTC
// import "net/http/fcgi" ==> using fcgi = go.net.http.fcgi_package
// Original source: C:\Program Files\Go\src\net\http\fcgi\fcgi.go
namespace go.net.http;
// This file defines the raw protocol and some utilities used by the child and
// the host.


using bufio = bufio_package;
using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;


// recType is a record type, as defined by
// https://web.archive.org/web/20150420080736/http://www.fastcgi.com/drupal/node/6?q=node/22#S8

public static partial class fcgi_package {

private partial struct recType { // : byte
}

private static readonly recType typeBeginRequest = 1;
private static readonly recType typeAbortRequest = 2;
private static readonly recType typeEndRequest = 3;
private static readonly recType typeParams = 4;
private static readonly recType typeStdin = 5;
private static readonly recType typeStdout = 6;
private static readonly recType typeStderr = 7;
private static readonly recType typeData = 8;
private static readonly recType typeGetValues = 9;
private static readonly recType typeGetValuesResult = 10;
private static readonly recType typeUnknownType = 11;

// keep the connection between web-server and responder open after request
private static readonly nint flagKeepConn = 1;



private static readonly nint maxWrite = 65535; // maximum record body
private static readonly nint maxPad = 255;

private static readonly var roleResponder = iota + 1; // only Responders are implemented.
private static readonly var roleAuthorizer = 0;
private static readonly var roleFilter = 1;

private static readonly var statusRequestComplete = iota;
private static readonly var statusCantMultiplex = 0;
private static readonly var statusOverloaded = 1;
private static readonly var statusUnknownRole = 2;

private partial struct header {
    public byte Version;
    public recType Type;
    public ushort Id;
    public ushort ContentLength;
    public byte PaddingLength;
    public byte Reserved;
}

private partial struct beginRequest {
    public ushort role;
    public byte flags;
    public array<byte> reserved;
}

private static error read(this ptr<beginRequest> _addr_br, slice<byte> content) {
    ref beginRequest br = ref _addr_br.val;

    if (len(content) != 8) {
        return error.As(errors.New("fcgi: invalid begin request record"))!;
    }
    br.role = binary.BigEndian.Uint16(content);
    br.flags = content[2];
    return error.As(null!)!;
}

// for padding so we don't have to allocate all the time
// not synchronized because we don't care what the contents are
private static array<byte> pad = new array<byte>(maxPad);

private static void init(this ptr<header> _addr_h, recType recType, ushort reqId, nint contentLength) {
    ref header h = ref _addr_h.val;

    h.Version = 1;
    h.Type = recType;
    h.Id = reqId;
    h.ContentLength = uint16(contentLength);
    h.PaddingLength = uint8(-contentLength & 7);
}

// conn sends records over rwc
private partial struct conn {
    public sync.Mutex mutex;
    public io.ReadWriteCloser rwc; // to avoid allocations
    public bytes.Buffer buf;
    public header h;
}

private static ptr<conn> newConn(io.ReadWriteCloser rwc) {
    return addr(new conn(rwc:rwc));
}

private static error Close(this ptr<conn> _addr_c) => func((defer, _, _) => {
    ref conn c = ref _addr_c.val;

    c.mutex.Lock();
    defer(c.mutex.Unlock());
    return error.As(c.rwc.Close())!;
});

private partial struct record {
    public header h;
    public array<byte> buf;
}

private static error read(this ptr<record> _addr_rec, io.Reader r) {
    error err = default!;
    ref record rec = ref _addr_rec.val;

    err = binary.Read(r, binary.BigEndian, _addr_rec.h);

    if (err != null) {
        return error.As(err)!;
    }
    if (rec.h.Version != 1) {
        return error.As(errors.New("fcgi: invalid header version"))!;
    }
    var n = int(rec.h.ContentLength) + int(rec.h.PaddingLength);
    _, err = io.ReadFull(r, rec.buf[..(int)n]);

    if (err != null) {
        return error.As(err)!;
    }
    return error.As(null!)!;
}

private static slice<byte> content(this ptr<record> _addr_r) {
    ref record r = ref _addr_r.val;

    return r.buf[..(int)r.h.ContentLength];
}

// writeRecord writes and sends a single record.
private static error writeRecord(this ptr<conn> _addr_c, recType recType, ushort reqId, slice<byte> b) => func((defer, _, _) => {
    ref conn c = ref _addr_c.val;

    c.mutex.Lock();
    defer(c.mutex.Unlock());
    c.buf.Reset();
    c.h.init(recType, reqId, len(b));
    {
        var err = binary.Write(_addr_c.buf, binary.BigEndian, c.h);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        var (_, err) = c.buf.Write(b);

        if (err != null) {
            return error.As(err)!;
        }
    }
    {
        (_, err) = c.buf.Write(pad[..(int)c.h.PaddingLength]);

        if (err != null) {
            return error.As(err)!;
        }
    }
    (_, err) = c.rwc.Write(c.buf.Bytes());
    return error.As(err)!;
});

private static error writeEndRequest(this ptr<conn> _addr_c, ushort reqId, nint appStatus, byte protocolStatus) {
    ref conn c = ref _addr_c.val;

    var b = make_slice<byte>(8);
    binary.BigEndian.PutUint32(b, uint32(appStatus));
    b[4] = protocolStatus;
    return error.As(c.writeRecord(typeEndRequest, reqId, b))!;
}

private static error writePairs(this ptr<conn> _addr_c, recType recType, ushort reqId, map<@string, @string> pairs) {
    ref conn c = ref _addr_c.val;

    var w = newWriter(_addr_c, recType, reqId);
    var b = make_slice<byte>(8);
    foreach (var (k, v) in pairs) {
        var n = encodeSize(b, uint32(len(k)));
        n += encodeSize(b[(int)n..], uint32(len(v)));
        {
            var (_, err) = w.Write(b[..(int)n]);

            if (err != null) {
                return error.As(err)!;
            }

        }
        {
            (_, err) = w.WriteString(k);

            if (err != null) {
                return error.As(err)!;
            }

        }
        {
            (_, err) = w.WriteString(v);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }    w.Close();
    return error.As(null!)!;
}

private static (uint, nint) readSize(slice<byte> s) {
    uint _p0 = default;
    nint _p0 = default;

    if (len(s) == 0) {
        return (0, 0);
    }
    var size = uint32(s[0]);
    nint n = 1;
    if (size & (1 << 7) != 0) {
        if (len(s) < 4) {
            return (0, 0);
        }
        n = 4;
        size = binary.BigEndian.Uint32(s);
        size &= 1 << 31;
    }
    return (size, n);
}

private static @string readString(slice<byte> s, uint size) {
    if (size > uint32(len(s))) {
        return "";
    }
    return string(s[..(int)size]);
}

private static nint encodeSize(slice<byte> b, uint size) {
    if (size > 127) {
        size |= 1 << 31;
        binary.BigEndian.PutUint32(b, size);
        return 4;
    }
    b[0] = byte(size);
    return 1;
}

// bufWriter encapsulates bufio.Writer but also closes the underlying stream when
// Closed.
private partial struct bufWriter {
    public io.Closer closer;
    public ref ptr<bufio.Writer> Writer> => ref Writer>_ptr;
}

private static error Close(this ptr<bufWriter> _addr_w) {
    ref bufWriter w = ref _addr_w.val;

    {
        var err = w.Writer.Flush();

        if (err != null) {
            w.closer.Close();
            return error.As(err)!;
        }
    }
    return error.As(w.closer.Close())!;
}

private static ptr<bufWriter> newWriter(ptr<conn> _addr_c, recType recType, ushort reqId) {
    ref conn c = ref _addr_c.val;

    ptr<streamWriter> s = addr(new streamWriter(c:c,recType:recType,reqId:reqId));
    var w = bufio.NewWriterSize(s, maxWrite);
    return addr(new bufWriter(s,w));
}

// streamWriter abstracts out the separation of a stream into discrete records.
// It only writes maxWrite bytes at a time.
private partial struct streamWriter {
    public ptr<conn> c;
    public recType recType;
    public ushort reqId;
}

private static (nint, error) Write(this ptr<streamWriter> _addr_w, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref streamWriter w = ref _addr_w.val;

    nint nn = 0;
    while (len(p) > 0) {
        var n = len(p);
        if (n > maxWrite) {
            n = maxWrite;
        }
        {
            var err = w.c.writeRecord(w.recType, w.reqId, p[..(int)n]);

            if (err != null) {
                return (nn, error.As(err)!);
            }

        }
        nn += n;
        p = p[(int)n..];
    }
    return (nn, error.As(null!)!);
}

private static error Close(this ptr<streamWriter> _addr_w) {
    ref streamWriter w = ref _addr_w.val;
 
    // send empty record to close the stream
    return error.As(w.c.writeRecord(w.recType, w.reqId, null))!;
}

} // end fcgi_package
