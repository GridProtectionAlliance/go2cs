// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2020 October 09 04:49:44 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\reader.go
using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class bytes_package
    {
        // A Reader implements the io.Reader, io.ReaderAt, io.WriterTo, io.Seeker,
        // io.ByteScanner, and io.RuneScanner interfaces by reading from
        // a byte slice.
        // Unlike a Buffer, a Reader is read-only and supports seeking.
        // The zero value for Reader operates like a Reader of an empty slice.
        public partial struct Reader
        {
            public slice<byte> s;
            public long i; // current reading index
            public long prevRune; // index of previous rune; or < 0
        }

        // Len returns the number of bytes of the unread portion of the
        // slice.
        private static long Len(this ptr<Reader> _addr_r)
        {
            ref Reader r = ref _addr_r.val;

            if (r.i >= int64(len(r.s)))
            {
                return 0L;
            }

            return int(int64(len(r.s)) - r.i);

        }

        // Size returns the original length of the underlying byte slice.
        // Size is the number of bytes available for reading via ReadAt.
        // The returned value is always the same and is not affected by calls
        // to any other method.
        private static long Size(this ptr<Reader> _addr_r)
        {
            ref Reader r = ref _addr_r.val;

            return int64(len(r.s));
        }

        // Read implements the io.Reader interface.
        private static (long, error) Read(this ptr<Reader> _addr_r, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;

            if (r.i >= int64(len(r.s)))
            {
                return (0L, error.As(io.EOF)!);
            }

            r.prevRune = -1L;
            n = copy(b, r.s[r.i..]);
            r.i += int64(n);
            return ;

        }

        // ReadAt implements the io.ReaderAt interface.
        private static (long, error) ReadAt(this ptr<Reader> _addr_r, slice<byte> b, long off)
        {
            long n = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;
 
            // cannot modify state - see io.ReaderAt
            if (off < 0L)
            {
                return (0L, error.As(errors.New("bytes.Reader.ReadAt: negative offset"))!);
            }

            if (off >= int64(len(r.s)))
            {
                return (0L, error.As(io.EOF)!);
            }

            n = copy(b, r.s[off..]);
            if (n < len(b))
            {
                err = io.EOF;
            }

            return ;

        }

        // ReadByte implements the io.ByteReader interface.
        private static (byte, error) ReadByte(this ptr<Reader> _addr_r)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref Reader r = ref _addr_r.val;

            r.prevRune = -1L;
            if (r.i >= int64(len(r.s)))
            {
                return (0L, error.As(io.EOF)!);
            }

            var b = r.s[r.i];
            r.i++;
            return (b, error.As(null!)!);

        }

        // UnreadByte complements ReadByte in implementing the io.ByteScanner interface.
        private static error UnreadByte(this ptr<Reader> _addr_r)
        {
            ref Reader r = ref _addr_r.val;

            if (r.i <= 0L)
            {
                return error.As(errors.New("bytes.Reader.UnreadByte: at beginning of slice"))!;
            }

            r.prevRune = -1L;
            r.i--;
            return error.As(null!)!;

        }

        // ReadRune implements the io.RuneReader interface.
        private static (int, long, error) ReadRune(this ptr<Reader> _addr_r)
        {
            int ch = default;
            long size = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;

            if (r.i >= int64(len(r.s)))
            {
                r.prevRune = -1L;
                return (0L, 0L, error.As(io.EOF)!);
            }

            r.prevRune = int(r.i);
            {
                var c = r.s[r.i];

                if (c < utf8.RuneSelf)
                {
                    r.i++;
                    return (rune(c), 1L, error.As(null!)!);
                }

            }

            ch, size = utf8.DecodeRune(r.s[r.i..]);
            r.i += int64(size);
            return ;

        }

        // UnreadRune complements ReadRune in implementing the io.RuneScanner interface.
        private static error UnreadRune(this ptr<Reader> _addr_r)
        {
            ref Reader r = ref _addr_r.val;

            if (r.i <= 0L)
            {
                return error.As(errors.New("bytes.Reader.UnreadRune: at beginning of slice"))!;
            }

            if (r.prevRune < 0L)
            {
                return error.As(errors.New("bytes.Reader.UnreadRune: previous operation was not ReadRune"))!;
            }

            r.i = int64(r.prevRune);
            r.prevRune = -1L;
            return error.As(null!)!;

        }

        // Seek implements the io.Seeker interface.
        private static (long, error) Seek(this ptr<Reader> _addr_r, long offset, long whence)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Reader r = ref _addr_r.val;

            r.prevRune = -1L;
            long abs = default;

            if (whence == io.SeekStart) 
                abs = offset;
            else if (whence == io.SeekCurrent) 
                abs = r.i + offset;
            else if (whence == io.SeekEnd) 
                abs = int64(len(r.s)) + offset;
            else 
                return (0L, error.As(errors.New("bytes.Reader.Seek: invalid whence"))!);
                        if (abs < 0L)
            {
                return (0L, error.As(errors.New("bytes.Reader.Seek: negative position"))!);
            }

            r.i = abs;
            return (abs, error.As(null!)!);

        }

        // WriteTo implements the io.WriterTo interface.
        private static (long, error) WriteTo(this ptr<Reader> _addr_r, io.Writer w) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            ref Reader r = ref _addr_r.val;

            r.prevRune = -1L;
            if (r.i >= int64(len(r.s)))
            {
                return (0L, error.As(null!)!);
            }

            var b = r.s[r.i..];
            var (m, err) = w.Write(b);
            if (m > len(b))
            {
                panic("bytes.Reader.WriteTo: invalid Write count");
            }

            r.i += int64(m);
            n = int64(m);
            if (m != len(b) && err == null)
            {
                err = io.ErrShortWrite;
            }

            return ;

        });

        // Reset resets the Reader to be reading from b.
        private static void Reset(this ptr<Reader> _addr_r, slice<byte> b)
        {
            ref Reader r = ref _addr_r.val;

            r.val = new Reader(b,0,-1);
        }

        // NewReader returns a new Reader reading from b.
        public static ptr<Reader> NewReader(slice<byte> b)
        {
            return addr(new Reader(b,0,-1));
        }
    }
}
