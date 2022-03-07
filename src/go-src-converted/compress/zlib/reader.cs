// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package zlib implements reading and writing of zlib format compressed data,
as specified in RFC 1950.

The implementation provides filters that uncompress during reading
and compress during writing.  For example, to write compressed data
to a buffer:

    var b bytes.Buffer
    w := zlib.NewWriter(&b)
    w.Write([]byte("hello, world\n"))
    w.Close()

and to read that data back:

    r, err := zlib.NewReader(&b)
    io.Copy(os.Stdout, r)
    r.Close()
*/
// package zlib -- go2cs converted at 2022 March 06 22:32:04 UTC
// import "compress/zlib" ==> using zlib = go.compress.zlib_package
// Original source: C:\Program Files\Go\src\compress\zlib\reader.go
using bufio = go.bufio_package;
using flate = go.compress.flate_package;
using errors = go.errors_package;
using hash = go.hash_package;
using adler32 = go.hash.adler32_package;
using io = go.io_package;

namespace go.compress;

public static partial class zlib_package {

private static readonly nint zlibDeflate = 8;



 
// ErrChecksum is returned when reading ZLIB data that has an invalid checksum.
public static var ErrChecksum = errors.New("zlib: invalid checksum");public static var ErrDictionary = errors.New("zlib: invalid dictionary");public static var ErrHeader = errors.New("zlib: invalid header");

private partial struct reader {
    public flate.Reader r;
    public io.ReadCloser decompressor;
    public hash.Hash32 digest;
    public error err;
    public array<byte> scratch;
}

// Resetter resets a ReadCloser returned by NewReader or NewReaderDict
// to switch to a new underlying Reader. This permits reusing a ReadCloser
// instead of allocating a new one.
public partial interface Resetter {
    error Reset(io.Reader r, slice<byte> dict);
}

// NewReader creates a new ReadCloser.
// Reads from the returned ReadCloser read and decompress data from r.
// If r does not implement io.ByteReader, the decompressor may read more
// data than necessary from r.
// It is the caller's responsibility to call Close on the ReadCloser when done.
//
// The ReadCloser returned by NewReader also implements Resetter.
public static (io.ReadCloser, error) NewReader(io.Reader r) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;

    return NewReaderDict(r, null);
}

// NewReaderDict is like NewReader but uses a preset dictionary.
// NewReaderDict ignores the dictionary if the compressed data does not refer to it.
// If the compressed data refers to a different dictionary, NewReaderDict returns ErrDictionary.
//
// The ReadCloser returned by NewReaderDict also implements Resetter.
public static (io.ReadCloser, error) NewReaderDict(io.Reader r, slice<byte> dict) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;

    ptr<reader> z = @new<reader>();
    var err = z.Reset(r, dict);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (z, error.As(null!)!);

}

private static (nint, error) Read(this ptr<reader> _addr_z, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref reader z = ref _addr_z.val;

    if (z.err != null) {
        return (0, error.As(z.err)!);
    }
    nint n = default;
    n, z.err = z.decompressor.Read(p);
    z.digest.Write(p[(int)0..(int)n]);
    if (z.err != io.EOF) { 
        // In the normal case we return here.
        return (n, error.As(z.err)!);

    }
    {
        var (_, err) = io.ReadFull(z.r, z.scratch[(int)0..(int)4]);

        if (err != null) {
            if (err == io.EOF) {
                err = io.ErrUnexpectedEOF;
            }
            z.err = err;
            return (n, error.As(z.err)!);
        }
    } 
    // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).
    var checksum = uint32(z.scratch[0]) << 24 | uint32(z.scratch[1]) << 16 | uint32(z.scratch[2]) << 8 | uint32(z.scratch[3]);
    if (checksum != z.digest.Sum32()) {
        z.err = ErrChecksum;
        return (n, error.As(z.err)!);
    }
    return (n, error.As(io.EOF)!);

}

// Calling Close does not close the wrapped io.Reader originally passed to NewReader.
// In order for the ZLIB checksum to be verified, the reader must be
// fully consumed until the io.EOF.
private static error Close(this ptr<reader> _addr_z) {
    ref reader z = ref _addr_z.val;

    if (z.err != null && z.err != io.EOF) {
        return error.As(z.err)!;
    }
    z.err = z.decompressor.Close();
    return error.As(z.err)!;

}

private static error Reset(this ptr<reader> _addr_z, io.Reader r, slice<byte> dict) {
    ref reader z = ref _addr_z.val;

    z.val = new reader(decompressor:z.decompressor);
    {
        flate.Reader (fr, ok) = r._<flate.Reader>();

        if (ok) {
            z.r = fr;
        }
        else
 {
            z.r = bufio.NewReader(r);
        }
    } 

    // Read the header (RFC 1950 section 2.2.).
    _, z.err = io.ReadFull(z.r, z.scratch[(int)0..(int)2]);
    if (z.err != null) {
        if (z.err == io.EOF) {
            z.err = io.ErrUnexpectedEOF;
        }
        return error.As(z.err)!;

    }
    var h = uint(z.scratch[0]) << 8 | uint(z.scratch[1]);
    if ((z.scratch[0] & 0x0f != zlibDeflate) || (h % 31 != 0)) {
        z.err = ErrHeader;
        return error.As(z.err)!;
    }
    var haveDict = z.scratch[1] & 0x20 != 0;
    if (haveDict) {
        _, z.err = io.ReadFull(z.r, z.scratch[(int)0..(int)4]);
        if (z.err != null) {
            if (z.err == io.EOF) {
                z.err = io.ErrUnexpectedEOF;
            }
            return error.As(z.err)!;
        }
        var checksum = uint32(z.scratch[0]) << 24 | uint32(z.scratch[1]) << 16 | uint32(z.scratch[2]) << 8 | uint32(z.scratch[3]);
        if (checksum != adler32.Checksum(dict)) {
            z.err = ErrDictionary;
            return error.As(z.err)!;
        }
    }
    if (z.decompressor == null) {
        if (haveDict) {
            z.decompressor = flate.NewReaderDict(z.r, dict);
        }
        else
 {
            z.decompressor = flate.NewReader(z.r);
        }
    }
    else
 {
        z.decompressor._<flate.Resetter>().Reset(z.r, dict);
    }
    z.digest = adler32.New();
    return error.As(null!)!;

}

} // end zlib_package
