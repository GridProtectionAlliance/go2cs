// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package zlib -- go2cs converted at 2020 October 09 05:08:33 UTC
// import "compress/zlib" ==> using zlib = go.compress.zlib_package
// Original source: C:\Go\src\compress\zlib\writer.go
using flate = go.compress.flate_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using hash = go.hash_package;
using adler32 = go.hash.adler32_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class zlib_package
    {
        // These constants are copied from the flate package, so that code that imports
        // "compress/zlib" does not also have to import "compress/flate".
        public static readonly var NoCompression = flate.NoCompression;
        public static readonly var BestSpeed = flate.BestSpeed;
        public static readonly var BestCompression = flate.BestCompression;
        public static readonly var DefaultCompression = flate.DefaultCompression;
        public static readonly var HuffmanOnly = flate.HuffmanOnly;


        // A Writer takes data written to it and writes the compressed
        // form of that data to an underlying writer (see NewWriter).
        public partial struct Writer
        {
            public io.Writer w;
            public long level;
            public slice<byte> dict;
            public ptr<flate.Writer> compressor;
            public hash.Hash32 digest;
            public error err;
            public array<byte> scratch;
            public bool wroteHeader;
        }

        // NewWriter creates a new Writer.
        // Writes to the returned Writer are compressed and written to w.
        //
        // It is the caller's responsibility to call Close on the Writer when done.
        // Writes may be buffered and not flushed until Close.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            var (z, _) = NewWriterLevelDict(w, DefaultCompression, null);
            return _addr_z!;
        }

        // NewWriterLevel is like NewWriter but specifies the compression level instead
        // of assuming DefaultCompression.
        //
        // The compression level can be DefaultCompression, NoCompression, HuffmanOnly
        // or any integer value between BestSpeed and BestCompression inclusive.
        // The error returned will be nil if the level is valid.
        public static (ptr<Writer>, error) NewWriterLevel(io.Writer w, long level)
        {
            ptr<Writer> _p0 = default!;
            error _p0 = default!;

            return _addr_NewWriterLevelDict(w, level, null)!;
        }

        // NewWriterLevelDict is like NewWriterLevel but specifies a dictionary to
        // compress with.
        //
        // The dictionary may be nil. If not, its contents should not be modified until
        // the Writer is closed.
        public static (ptr<Writer>, error) NewWriterLevelDict(io.Writer w, long level, slice<byte> dict)
        {
            ptr<Writer> _p0 = default!;
            error _p0 = default!;

            if (level < HuffmanOnly || level > BestCompression)
            {
                return (_addr_null!, error.As(fmt.Errorf("zlib: invalid compression level: %d", level))!);
            }

            return (addr(new Writer(w:w,level:level,dict:dict,)), error.As(null!)!);

        }

        // Reset clears the state of the Writer z such that it is equivalent to its
        // initial state from NewWriterLevel or NewWriterLevelDict, but instead writing
        // to w.
        private static void Reset(this ptr<Writer> _addr_z, io.Writer w)
        {
            ref Writer z = ref _addr_z.val;

            z.w = w; 
            // z.level and z.dict left unchanged.
            if (z.compressor != null)
            {
                z.compressor.Reset(w);
            }

            if (z.digest != null)
            {
                z.digest.Reset();
            }

            z.err = null;
            z.scratch = new array<byte>(new byte[] {  });
            z.wroteHeader = false;

        }

        // writeHeader writes the ZLIB header.
        private static error writeHeader(this ptr<Writer> _addr_z) => func((_, panic, __) =>
        {
            error err = default!;
            ref Writer z = ref _addr_z.val;

            z.wroteHeader = true; 
            // ZLIB has a two-byte header (as documented in RFC 1950).
            // The first four bits is the CINFO (compression info), which is 7 for the default deflate window size.
            // The next four bits is the CM (compression method), which is 8 for deflate.
            z.scratch[0L] = 0x78UL; 
            // The next two bits is the FLEVEL (compression level). The four values are:
            // 0=fastest, 1=fast, 2=default, 3=best.
            // The next bit, FDICT, is set if a dictionary is given.
            // The final five FCHECK bits form a mod-31 checksum.
            switch (z.level)
            {
                case -2L: 

                case 0L: 

                case 1L: 
                    z.scratch[1L] = 0L << (int)(6L);
                    break;
                case 2L: 

                case 3L: 

                case 4L: 

                case 5L: 
                    z.scratch[1L] = 1L << (int)(6L);
                    break;
                case 6L: 

                case -1L: 
                    z.scratch[1L] = 2L << (int)(6L);
                    break;
                case 7L: 

                case 8L: 

                case 9L: 
                    z.scratch[1L] = 3L << (int)(6L);
                    break;
                default: 
                    panic("unreachable");
                    break;
            }
            if (z.dict != null)
            {
                z.scratch[1L] |= 1L << (int)(5L);
            }

            z.scratch[1L] += uint8(31L - (uint16(z.scratch[0L]) << (int)(8L) + uint16(z.scratch[1L])) % 31L);
            _, err = z.w.Write(z.scratch[0L..2L]);

            if (err != null)
            {
                return error.As(err)!;
            }

            if (z.dict != null)
            { 
                // The next four bytes are the Adler-32 checksum of the dictionary.
                binary.BigEndian.PutUint32(z.scratch[..], adler32.Checksum(z.dict));
                _, err = z.w.Write(z.scratch[0L..4L]);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            if (z.compressor == null)
            { 
                // Initialize deflater unless the Writer is being reused
                // after a Reset call.
                z.compressor, err = flate.NewWriterDict(z.w, z.level, z.dict);
                if (err != null)
                {
                    return error.As(err)!;
                }

                z.digest = adler32.New();

            }

            return error.As(null!)!;

        });

        // Write writes a compressed form of p to the underlying io.Writer. The
        // compressed bytes are not necessarily flushed until the Writer is closed or
        // explicitly flushed.
        private static (long, error) Write(this ptr<Writer> _addr_z, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref Writer z = ref _addr_z.val;

            if (!z.wroteHeader)
            {
                z.err = z.writeHeader();
            }

            if (z.err != null)
            {
                return (0L, error.As(z.err)!);
            }

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            n, err = z.compressor.Write(p);
            if (err != null)
            {
                z.err = err;
                return ;
            }

            z.digest.Write(p);
            return ;

        }

        // Flush flushes the Writer to its underlying io.Writer.
        private static error Flush(this ptr<Writer> _addr_z)
        {
            ref Writer z = ref _addr_z.val;

            if (!z.wroteHeader)
            {
                z.err = z.writeHeader();
            }

            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            z.err = z.compressor.Flush();
            return error.As(z.err)!;

        }

        // Close closes the Writer, flushing any unwritten data to the underlying
        // io.Writer, but does not close the underlying io.Writer.
        private static error Close(this ptr<Writer> _addr_z)
        {
            ref Writer z = ref _addr_z.val;

            if (!z.wroteHeader)
            {
                z.err = z.writeHeader();
            }

            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            z.err = z.compressor.Close();
            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            var checksum = z.digest.Sum32(); 
            // ZLIB (RFC 1950) is big-endian, unlike GZIP (RFC 1952).
            binary.BigEndian.PutUint32(z.scratch[..], checksum);
            _, z.err = z.w.Write(z.scratch[0L..4L]);
            return error.As(z.err)!;

        }
    }
}}
