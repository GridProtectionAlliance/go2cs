// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gzip -- go2cs converted at 2020 October 08 03:31:03 UTC
// import "compress/gzip" ==> using gzip = go.compress.gzip_package
// Original source: C:\Go\src\compress\gzip\gzip.go
using flate = go.compress.flate_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using crc32 = go.hash.crc32_package;
using io = go.io_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class gzip_package
    {
        // These constants are copied from the flate package, so that code that imports
        // "compress/gzip" does not also have to import "compress/flate".
        public static readonly var NoCompression = (var)flate.NoCompression;
        public static readonly var BestSpeed = (var)flate.BestSpeed;
        public static readonly var BestCompression = (var)flate.BestCompression;
        public static readonly var DefaultCompression = (var)flate.DefaultCompression;
        public static readonly var HuffmanOnly = (var)flate.HuffmanOnly;


        // A Writer is an io.WriteCloser.
        // Writes to a Writer are compressed and written to w.
        public partial struct Writer
        {
            public ref Header Header => ref Header_val; // written at first call to Write, Flush, or Close
            public io.Writer w;
            public long level;
            public bool wroteHeader;
            public ptr<flate.Writer> compressor;
            public uint digest; // CRC-32, IEEE polynomial (section 8)
            public uint size; // Uncompressed size (section 2.3.1)
            public bool closed;
            public array<byte> buf;
            public error err;
        }

        // NewWriter returns a new Writer.
        // Writes to the returned writer are compressed and written to w.
        //
        // It is the caller's responsibility to call Close on the Writer when done.
        // Writes may be buffered and not flushed until Close.
        //
        // Callers that wish to set the fields in Writer.Header must do so before
        // the first call to Write, Flush, or Close.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            var (z, _) = NewWriterLevel(w, DefaultCompression);
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

            if (level < HuffmanOnly || level > BestCompression)
            {
                return (_addr_null!, error.As(fmt.Errorf("gzip: invalid compression level: %d", level))!);
            }

            ptr<Writer> z = @new<Writer>();
            z.init(w, level);
            return (_addr_z!, error.As(null!)!);

        }

        private static void init(this ptr<Writer> _addr_z, io.Writer w, long level)
        {
            ref Writer z = ref _addr_z.val;

            var compressor = z.compressor;
            if (compressor != null)
            {
                compressor.Reset(w);
            }

            z.val = new Writer(Header:Header{OS:255,},w:w,level:level,compressor:compressor,);

        }

        // Reset discards the Writer z's state and makes it equivalent to the
        // result of its original state from NewWriter or NewWriterLevel, but
        // writing to w instead. This permits reusing a Writer rather than
        // allocating a new one.
        private static void Reset(this ptr<Writer> _addr_z, io.Writer w)
        {
            ref Writer z = ref _addr_z.val;

            z.init(w, z.level);
        }

        // writeBytes writes a length-prefixed byte slice to z.w.
        private static error writeBytes(this ptr<Writer> _addr_z, slice<byte> b)
        {
            ref Writer z = ref _addr_z.val;

            if (len(b) > 0xffffUL)
            {
                return error.As(errors.New("gzip.Write: Extra data is too large"))!;
            }

            le.PutUint16(z.buf[..2L], uint16(len(b)));
            var (_, err) = z.w.Write(z.buf[..2L]);
            if (err != null)
            {
                return error.As(err)!;
            }

            _, err = z.w.Write(b);
            return error.As(err)!;

        }

        // writeString writes a UTF-8 string s in GZIP's format to z.w.
        // GZIP (RFC 1952) specifies that strings are NUL-terminated ISO 8859-1 (Latin-1).
        private static error writeString(this ptr<Writer> _addr_z, @string s)
        {
            error err = default!;
            ref Writer z = ref _addr_z.val;
 
            // GZIP stores Latin-1 strings; error if non-Latin-1; convert if non-ASCII.
            var needconv = false;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in s)
                {
                    v = __v;
                    if (v == 0L || v > 0xffUL)
                    {
                        return error.As(errors.New("gzip.Write: non-Latin-1 header string"))!;
                    }

                    if (v > 0x7fUL)
                    {
                        needconv = true;
                    }

                }

                v = v__prev1;
            }

            if (needconv)
            {
                var b = make_slice<byte>(0L, len(s));
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in s)
                    {
                        v = __v;
                        b = append(b, byte(v));
                    }
            else

                    v = v__prev1;
                }

                _, err = z.w.Write(b);

            }            {
                _, err = io.WriteString(z.w, s);
            }

            if (err != null)
            {
                return error.As(err)!;
            } 
            // GZIP strings are NUL-terminated.
            z.buf[0L] = 0L;
            _, err = z.w.Write(z.buf[..1L]);
            return error.As(err)!;

        }

        // Write writes a compressed form of p to the underlying io.Writer. The
        // compressed bytes are not necessarily flushed until the Writer is closed.
        private static (long, error) Write(this ptr<Writer> _addr_z, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Writer z = ref _addr_z.val;

            if (z.err != null)
            {
                return (0L, error.As(z.err)!);
            }

            long n = default; 
            // Write the GZIP header lazily.
            if (!z.wroteHeader)
            {
                z.wroteHeader = true;
                z.buf = new array<byte>(InitKeyedValues<byte>(10, (0, gzipID1), (1, gzipID2), (2, gzipDeflate)));
                if (z.Extra != null)
                {
                    z.buf[3L] |= 0x04UL;
                }

                if (z.Name != "")
                {
                    z.buf[3L] |= 0x08UL;
                }

                if (z.Comment != "")
                {
                    z.buf[3L] |= 0x10UL;
                }

                if (z.ModTime.After(time.Unix(0L, 0L)))
                { 
                    // Section 2.3.1, the zero value for MTIME means that the
                    // modified time is not set.
                    le.PutUint32(z.buf[4L..8L], uint32(z.ModTime.Unix()));

                }

                if (z.level == BestCompression)
                {
                    z.buf[8L] = 2L;
                }
                else if (z.level == BestSpeed)
                {
                    z.buf[8L] = 4L;
                }

                z.buf[9L] = z.OS;
                _, z.err = z.w.Write(z.buf[..10L]);
                if (z.err != null)
                {
                    return (0L, error.As(z.err)!);
                }

                if (z.Extra != null)
                {
                    z.err = z.writeBytes(z.Extra);
                    if (z.err != null)
                    {
                        return (0L, error.As(z.err)!);
                    }

                }

                if (z.Name != "")
                {
                    z.err = z.writeString(z.Name);
                    if (z.err != null)
                    {
                        return (0L, error.As(z.err)!);
                    }

                }

                if (z.Comment != "")
                {
                    z.err = z.writeString(z.Comment);
                    if (z.err != null)
                    {
                        return (0L, error.As(z.err)!);
                    }

                }

                if (z.compressor == null)
                {
                    z.compressor, _ = flate.NewWriter(z.w, z.level);
                }

            }

            z.size += uint32(len(p));
            z.digest = crc32.Update(z.digest, crc32.IEEETable, p);
            n, z.err = z.compressor.Write(p);
            return (n, error.As(z.err)!);

        }

        // Flush flushes any pending compressed data to the underlying writer.
        //
        // It is useful mainly in compressed network protocols, to ensure that
        // a remote reader has enough data to reconstruct a packet. Flush does
        // not return until the data has been written. If the underlying
        // writer returns an error, Flush returns that error.
        //
        // In the terminology of the zlib library, Flush is equivalent to Z_SYNC_FLUSH.
        private static error Flush(this ptr<Writer> _addr_z)
        {
            ref Writer z = ref _addr_z.val;

            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            if (z.closed)
            {
                return error.As(null!)!;
            }

            if (!z.wroteHeader)
            {
                z.Write(null);
                if (z.err != null)
                {
                    return error.As(z.err)!;
                }

            }

            z.err = z.compressor.Flush();
            return error.As(z.err)!;

        }

        // Close closes the Writer by flushing any unwritten data to the underlying
        // io.Writer and writing the GZIP footer.
        // It does not close the underlying io.Writer.
        private static error Close(this ptr<Writer> _addr_z)
        {
            ref Writer z = ref _addr_z.val;

            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            if (z.closed)
            {
                return error.As(null!)!;
            }

            z.closed = true;
            if (!z.wroteHeader)
            {
                z.Write(null);
                if (z.err != null)
                {
                    return error.As(z.err)!;
                }

            }

            z.err = z.compressor.Close();
            if (z.err != null)
            {
                return error.As(z.err)!;
            }

            le.PutUint32(z.buf[..4L], z.digest);
            le.PutUint32(z.buf[4L..8L], z.size);
            _, z.err = z.w.Write(z.buf[..8L]);
            return error.As(z.err)!;

        }
    }
}}
