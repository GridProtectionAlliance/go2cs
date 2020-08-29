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
// package zlib -- go2cs converted at 2020 August 29 08:46:03 UTC
// import "compress/zlib" ==> using zlib = go.compress.zlib_package
// Original source: C:\Go\src\compress\zlib\reader.go
using bufio = go.bufio_package;
using flate = go.compress.flate_package;
using errors = go.errors_package;
using hash = go.hash_package;
using adler32 = go.hash.adler32_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class zlib_package
    {
        private static readonly long zlibDeflate = 8L;



 
        // ErrChecksum is returned when reading ZLIB data that has an invalid checksum.
        public static var ErrChecksum = errors.New("zlib: invalid checksum");        public static var ErrDictionary = errors.New("zlib: invalid dictionary");        public static var ErrHeader = errors.New("zlib: invalid header");

        private partial struct reader
        {
            public flate.Reader r;
            public io.ReadCloser decompressor;
            public hash.Hash32 digest;
            public error err;
            public array<byte> scratch;
        }

        // Resetter resets a ReadCloser returned by NewReader or NewReaderDict to
        // to switch to a new underlying Reader. This permits reusing a ReadCloser
        // instead of allocating a new one.
        public partial interface Resetter
        {
            error Reset(io.Reader r, slice<byte> dict);
        }

        // NewReader creates a new ReadCloser.
        // Reads from the returned ReadCloser read and decompress data from r.
        // If r does not implement io.ByteReader, the decompressor may read more
        // data than necessary from r.
        // It is the caller's responsibility to call Close on the ReadCloser when done.
        //
        // The ReadCloser returned by NewReader also implements Resetter.
        public static (io.ReadCloser, error) NewReader(io.Reader r)
        {
            return NewReaderDict(r, null);
        }

        // NewReaderDict is like NewReader but uses a preset dictionary.
        // NewReaderDict ignores the dictionary if the compressed data does not refer to it.
        // If the compressed data refers to a different dictionary, NewReaderDict returns ErrDictionary.
        //
        // The ReadCloser returned by NewReaderDict also implements Resetter.
        public static (io.ReadCloser, error) NewReaderDict(io.Reader r, slice<byte> dict)
        {
            ptr<object> z = @new<reader>();
            var err = z.Reset(r, dict);
            if (err != null)
            {
                return (null, err);
            }
            return (z, null);
        }

        private static (long, error) Read(this ref reader z, slice<byte> p)
        {
            if (z.err != null)
            {
                return (0L, z.err);
            }
            long n = default;
            n, z.err = z.decompressor.Read(p);
            z.digest.Write(p[0L..n]);
            if (z.err != io.EOF)
            { 
                // In the normal case we return here.
                return (n, z.err);
            } 

            // Finished file; check checksum.
            {
                var (_, err) = io.ReadFull(z.r, z.scratch[0L..4L]);

                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }
                    z.err = err;
                    return (n, z.err);
                } 
                // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).

            } 
            // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).
            var checksum = uint32(z.scratch[0L]) << (int)(24L) | uint32(z.scratch[1L]) << (int)(16L) | uint32(z.scratch[2L]) << (int)(8L) | uint32(z.scratch[3L]);
            if (checksum != z.digest.Sum32())
            {
                z.err = ErrChecksum;
                return (n, z.err);
            }
            return (n, io.EOF);
        }

        // Calling Close does not close the wrapped io.Reader originally passed to NewReader.
        // In order for the ZLIB checksum to be verified, the reader must be
        // fully consumed until the io.EOF.
        private static error Close(this ref reader z)
        {
            if (z.err != null && z.err != io.EOF)
            {
                return error.As(z.err);
            }
            z.err = z.decompressor.Close();
            return error.As(z.err);
        }

        private static error Reset(this ref reader z, io.Reader r, slice<byte> dict)
        {
            z.Value = new reader(decompressor:z.decompressor);
            {
                flate.Reader (fr, ok) = r._<flate.Reader>();

                if (ok)
                {
                    z.r = fr;
                }
                else
                {
                    z.r = bufio.NewReader(r);
                } 

                // Read the header (RFC 1950 section 2.2.).

            } 

            // Read the header (RFC 1950 section 2.2.).
            _, z.err = io.ReadFull(z.r, z.scratch[0L..2L]);
            if (z.err != null)
            {
                if (z.err == io.EOF)
                {
                    z.err = io.ErrUnexpectedEOF;
                }
                return error.As(z.err);
            }
            var h = uint(z.scratch[0L]) << (int)(8L) | uint(z.scratch[1L]);
            if ((z.scratch[0L] & 0x0fUL != zlibDeflate) || (h % 31L != 0L))
            {
                z.err = ErrHeader;
                return error.As(z.err);
            }
            var haveDict = z.scratch[1L] & 0x20UL != 0L;
            if (haveDict)
            {
                _, z.err = io.ReadFull(z.r, z.scratch[0L..4L]);
                if (z.err != null)
                {
                    if (z.err == io.EOF)
                    {
                        z.err = io.ErrUnexpectedEOF;
                    }
                    return error.As(z.err);
                }
                var checksum = uint32(z.scratch[0L]) << (int)(24L) | uint32(z.scratch[1L]) << (int)(16L) | uint32(z.scratch[2L]) << (int)(8L) | uint32(z.scratch[3L]);
                if (checksum != adler32.Checksum(dict))
                {
                    z.err = ErrDictionary;
                    return error.As(z.err);
                }
            }
            if (z.decompressor == null)
            {
                if (haveDict)
                {
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
            return error.As(null);
        }
    }
}}
