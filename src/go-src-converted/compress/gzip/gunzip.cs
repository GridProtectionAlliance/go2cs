// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gzip implements reading and writing of gzip format compressed files,
// as specified in RFC 1952.

// package gzip -- go2cs converted at 2022 March 13 05:28:52 UTC
// import "compress/gzip" ==> using gzip = go.compress.gzip_package
// Original source: C:\Program Files\Go\src\compress\gzip\gunzip.go
namespace go.compress;

using bufio = bufio_package;
using flate = compress.flate_package;
using binary = encoding.binary_package;
using errors = errors_package;
using crc32 = hash.crc32_package;
using io = io_package;
using time = time_package;

public static partial class gzip_package {

private static readonly nuint gzipID1 = 0x1f;
private static readonly nuint gzipID2 = 0x8b;
private static readonly nint gzipDeflate = 8;
private static readonly nint flagText = 1 << 0;
private static readonly nint flagHdrCrc = 1 << 1;
private static readonly nint flagExtra = 1 << 2;
private static readonly nint flagName = 1 << 3;
private static readonly nint flagComment = 1 << 4;

 
// ErrChecksum is returned when reading GZIP data that has an invalid checksum.
public static var ErrChecksum = errors.New("gzip: invalid checksum");public static var ErrHeader = errors.New("gzip: invalid header");

private static var le = binary.LittleEndian;

// noEOF converts io.EOF to io.ErrUnexpectedEOF.
private static error noEOF(error err) {
    if (err == io.EOF) {
        return error.As(io.ErrUnexpectedEOF)!;
    }
    return error.As(err)!;
}

// The gzip file stores a header giving metadata about the compressed file.
// That header is exposed as the fields of the Writer and Reader structs.
//
// Strings must be UTF-8 encoded and may only contain Unicode code points
// U+0001 through U+00FF, due to limitations of the GZIP file format.
public partial struct Header {
    public @string Comment; // comment
    public slice<byte> Extra; // "extra data"
    public time.Time ModTime; // modification time
    public @string Name; // file name
    public byte OS; // operating system type
}

// A Reader is an io.Reader that can be read to retrieve
// uncompressed data from a gzip-format compressed file.
//
// In general, a gzip file can be a concatenation of gzip files,
// each with its own header. Reads from the Reader
// return the concatenation of the uncompressed data of each.
// Only the first header is recorded in the Reader fields.
//
// Gzip files store a length and checksum of the uncompressed data.
// The Reader will return an ErrChecksum when Read
// reaches the end of the uncompressed data if it does not
// have the expected length or checksum. Clients should treat data
// returned by Read as tentative until they receive the io.EOF
// marking the end of the data.
public partial struct Reader {
    public ref Header Header => ref Header_val; // valid after NewReader or Reader.Reset
    public flate.Reader r;
    public io.ReadCloser decompressor;
    public uint digest; // CRC-32, IEEE polynomial (section 8)
    public uint size; // Uncompressed size (section 2.3.1)
    public array<byte> buf;
    public error err;
    public bool multistream;
}

// NewReader creates a new Reader reading the given reader.
// If r does not also implement io.ByteReader,
// the decompressor may read more data than necessary from r.
//
// It is the caller's responsibility to call Close on the Reader when done.
//
// The Reader.Header fields will be valid in the Reader returned.
public static (ptr<Reader>, error) NewReader(io.Reader r) {
    ptr<Reader> _p0 = default!;
    error _p0 = default!;

    ptr<Reader> z = @new<Reader>();
    {
        var err = z.Reset(r);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    return (_addr_z!, error.As(null!)!);
}

// Reset discards the Reader z's state and makes it equivalent to the
// result of its original state from NewReader, but reading from r instead.
// This permits reusing a Reader rather than allocating a new one.
private static error Reset(this ptr<Reader> _addr_z, io.Reader r) {
    ref Reader z = ref _addr_z.val;

    z.val = new Reader(decompressor:z.decompressor,multistream:true,);
    {
        flate.Reader (rr, ok) = r._<flate.Reader>();

        if (ok) {
            z.r = rr;
        }
        else
 {
            z.r = bufio.NewReader(r);
        }
    }
    z.Header, z.err = z.readHeader();
    return error.As(z.err)!;
}

// Multistream controls whether the reader supports multistream files.
//
// If enabled (the default), the Reader expects the input to be a sequence
// of individually gzipped data streams, each with its own header and
// trailer, ending at EOF. The effect is that the concatenation of a sequence
// of gzipped files is treated as equivalent to the gzip of the concatenation
// of the sequence. This is standard behavior for gzip readers.
//
// Calling Multistream(false) disables this behavior; disabling the behavior
// can be useful when reading file formats that distinguish individual gzip
// data streams or mix gzip data streams with other data streams.
// In this mode, when the Reader reaches the end of the data stream,
// Read returns io.EOF. The underlying reader must implement io.ByteReader
// in order to be left positioned just after the gzip stream.
// To start the next stream, call z.Reset(r) followed by z.Multistream(false).
// If there is no next stream, z.Reset(r) will return io.EOF.
private static void Multistream(this ptr<Reader> _addr_z, bool ok) {
    ref Reader z = ref _addr_z.val;

    z.multistream = ok;
}

// readString reads a NUL-terminated string from z.r.
// It treats the bytes read as being encoded as ISO 8859-1 (Latin-1) and
// will output a string encoded using UTF-8.
// This method always updates z.digest with the data read.
private static (@string, error) readString(this ptr<Reader> _addr_z) {
    @string _p0 = default;
    error _p0 = default!;
    ref Reader z = ref _addr_z.val;

    error err = default!;
    var needConv = false;
    for (nint i = 0; ; i++) {
        if (i >= len(z.buf)) {
            return ("", error.As(ErrHeader)!);
        }
        z.buf[i], err = z.r.ReadByte();
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (z.buf[i] > 0x7f) {
            needConv = true;
        }
        if (z.buf[i] == 0) { 
            // Digest covers the NUL terminator.
            z.digest = crc32.Update(z.digest, crc32.IEEETable, z.buf[..(int)i + 1]); 

            // Strings are ISO 8859-1, Latin-1 (RFC 1952, section 2.3.1).
            if (needConv) {
                var s = make_slice<int>(0, i);
                foreach (var (_, v) in z.buf[..(int)i]) {
                    s = append(s, rune(v));
                }
                return (string(s), error.As(null!)!);
            }
            return (string(z.buf[..(int)i]), error.As(null!)!);
        }
    }
}

// readHeader reads the GZIP header according to section 2.3.1.
// This method does not set z.err.
private static (Header, error) readHeader(this ptr<Reader> _addr_z) {
    Header hdr = default;
    error err = default!;
    ref Reader z = ref _addr_z.val;

    _, err = io.ReadFull(z.r, z.buf[..(int)10]);

    if (err != null) { 
        // RFC 1952, section 2.2, says the following:
        //    A gzip file consists of a series of "members" (compressed data sets).
        //
        // Other than this, the specification does not clarify whether a
        // "series" is defined as "one or more" or "zero or more". To err on the
        // side of caution, Go interprets this to mean "zero or more".
        // Thus, it is okay to return io.EOF here.
        return (hdr, error.As(err)!);
    }
    if (z.buf[0] != gzipID1 || z.buf[1] != gzipID2 || z.buf[2] != gzipDeflate) {
        return (hdr, error.As(ErrHeader)!);
    }
    var flg = z.buf[3];
    {
        var t = int64(le.Uint32(z.buf[(int)4..(int)8]));

        if (t > 0) { 
            // Section 2.3.1, the zero value for MTIME means that the
            // modified time is not set.
            hdr.ModTime = time.Unix(t, 0);
        }
    } 
    // z.buf[8] is XFL and is currently ignored.
    hdr.OS = z.buf[9];
    z.digest = crc32.ChecksumIEEE(z.buf[..(int)10]);

    if (flg & flagExtra != 0) {
        _, err = io.ReadFull(z.r, z.buf[..(int)2]);

        if (err != null) {
            return (hdr, error.As(noEOF(err))!);
        }
        z.digest = crc32.Update(z.digest, crc32.IEEETable, z.buf[..(int)2]);
        var data = make_slice<byte>(le.Uint16(z.buf[..(int)2]));
        _, err = io.ReadFull(z.r, data);

        if (err != null) {
            return (hdr, error.As(noEOF(err))!);
        }
        z.digest = crc32.Update(z.digest, crc32.IEEETable, data);
        hdr.Extra = data;
    }
    @string s = default;
    if (flg & flagName != 0) {
        s, err = z.readString();

        if (err != null) {
            return (hdr, error.As(err)!);
        }
        hdr.Name = s;
    }
    if (flg & flagComment != 0) {
        s, err = z.readString();

        if (err != null) {
            return (hdr, error.As(err)!);
        }
        hdr.Comment = s;
    }
    if (flg & flagHdrCrc != 0) {
        _, err = io.ReadFull(z.r, z.buf[..(int)2]);

        if (err != null) {
            return (hdr, error.As(noEOF(err))!);
        }
        var digest = le.Uint16(z.buf[..(int)2]);
        if (digest != uint16(z.digest)) {
            return (hdr, error.As(ErrHeader)!);
        }
    }
    z.digest = 0;
    if (z.decompressor == null) {
        z.decompressor = flate.NewReader(z.r);
    }
    else
 {
        z.decompressor._<flate.Resetter>().Reset(z.r, null);
    }
    return (hdr, error.As(null!)!);
}

// Read implements io.Reader, reading uncompressed bytes from its underlying Reader.
private static (nint, error) Read(this ptr<Reader> _addr_z, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Reader z = ref _addr_z.val;

    if (z.err != null) {
        return (0, error.As(z.err)!);
    }
    n, z.err = z.decompressor.Read(p);
    z.digest = crc32.Update(z.digest, crc32.IEEETable, p[..(int)n]);
    z.size += uint32(n);
    if (z.err != io.EOF) { 
        // In the normal case we return here.
        return (n, error.As(z.err)!);
    }
    {
        var (_, err) = io.ReadFull(z.r, z.buf[..(int)8]);

        if (err != null) {
            z.err = noEOF(err);
            return (n, error.As(z.err)!);
        }
    }
    var digest = le.Uint32(z.buf[..(int)4]);
    var size = le.Uint32(z.buf[(int)4..(int)8]);
    if (digest != z.digest || size != z.size) {
        z.err = ErrChecksum;
        return (n, error.As(z.err)!);
    }
    (z.digest, z.size) = (0, 0);    if (!z.multistream) {
        return (n, error.As(io.EOF)!);
    }
    z.err = null; // Remove io.EOF

    _, z.err = z.readHeader();

    if (z.err != null) {
        return (n, error.As(z.err)!);
    }
    if (n > 0) {
        return (n, error.As(null!)!);
    }
    return z.Read(p);
}

// Close closes the Reader. It does not close the underlying io.Reader.
// In order for the GZIP checksum to be verified, the reader must be
// fully consumed until the io.EOF.
private static error Close(this ptr<Reader> _addr_z) {
    ref Reader z = ref _addr_z.val;

    return error.As(z.decompressor.Close())!;
}

} // end gzip_package
