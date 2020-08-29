// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2020 August 29 08:42:39 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\reader.go
using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class strings_package
    {
        // A Reader implements the io.Reader, io.ReaderAt, io.Seeker, io.WriterTo,
        // io.ByteScanner, and io.RuneScanner interfaces by reading
        // from a string.
        public partial struct Reader
        {
            public @string s;
            public long i; // current reading index
            public long prevRune; // index of previous rune; or < 0
        }

        // Len returns the number of bytes of the unread portion of the
        // string.
        private static long Len(this ref Reader r)
        {
            if (r.i >= int64(len(r.s)))
            {
                return 0L;
            }
            return int(int64(len(r.s)) - r.i);
        }

        // Size returns the original length of the underlying string.
        // Size is the number of bytes available for reading via ReadAt.
        // The returned value is always the same and is not affected by calls
        // to any other method.
        private static long Size(this ref Reader r)
        {
            return int64(len(r.s));
        }

        private static (long, error) Read(this ref Reader r, slice<byte> b)
        {
            if (r.i >= int64(len(r.s)))
            {
                return (0L, io.EOF);
            }
            r.prevRune = -1L;
            n = copy(b, r.s[r.i..]);
            r.i += int64(n);
            return;
        }

        private static (long, error) ReadAt(this ref Reader r, slice<byte> b, long off)
        { 
            // cannot modify state - see io.ReaderAt
            if (off < 0L)
            {
                return (0L, errors.New("strings.Reader.ReadAt: negative offset"));
            }
            if (off >= int64(len(r.s)))
            {
                return (0L, io.EOF);
            }
            n = copy(b, r.s[off..]);
            if (n < len(b))
            {
                err = io.EOF;
            }
            return;
        }

        private static (byte, error) ReadByte(this ref Reader r)
        {
            r.prevRune = -1L;
            if (r.i >= int64(len(r.s)))
            {
                return (0L, io.EOF);
            }
            var b = r.s[r.i];
            r.i++;
            return (b, null);
        }

        private static error UnreadByte(this ref Reader r)
        {
            r.prevRune = -1L;
            if (r.i <= 0L)
            {
                return error.As(errors.New("strings.Reader.UnreadByte: at beginning of string"));
            }
            r.i--;
            return error.As(null);
        }

        private static (int, long, error) ReadRune(this ref Reader r)
        {
            if (r.i >= int64(len(r.s)))
            {
                r.prevRune = -1L;
                return (0L, 0L, io.EOF);
            }
            r.prevRune = int(r.i);
            {
                var c = r.s[r.i];

                if (c < utf8.RuneSelf)
                {
                    r.i++;
                    return (rune(c), 1L, null);
                }

            }
            ch, size = utf8.DecodeRuneInString(r.s[r.i..]);
            r.i += int64(size);
            return;
        }

        private static error UnreadRune(this ref Reader r)
        {
            if (r.prevRune < 0L)
            {
                return error.As(errors.New("strings.Reader.UnreadRune: previous operation was not ReadRune"));
            }
            r.i = int64(r.prevRune);
            r.prevRune = -1L;
            return error.As(null);
        }

        // Seek implements the io.Seeker interface.
        private static (long, error) Seek(this ref Reader r, long offset, long whence)
        {
            r.prevRune = -1L;
            long abs = default;

            if (whence == io.SeekStart) 
                abs = offset;
            else if (whence == io.SeekCurrent) 
                abs = r.i + offset;
            else if (whence == io.SeekEnd) 
                abs = int64(len(r.s)) + offset;
            else 
                return (0L, errors.New("strings.Reader.Seek: invalid whence"));
                        if (abs < 0L)
            {
                return (0L, errors.New("strings.Reader.Seek: negative position"));
            }
            r.i = abs;
            return (abs, null);
        }

        // WriteTo implements the io.WriterTo interface.
        private static (long, error) WriteTo(this ref Reader _r, io.Writer w) => func(_r, (ref Reader r, Defer _, Panic panic, Recover __) =>
        {
            r.prevRune = -1L;
            if (r.i >= int64(len(r.s)))
            {
                return (0L, null);
            }
            var s = r.s[r.i..];
            var (m, err) = io.WriteString(w, s);
            if (m > len(s))
            {
                panic("strings.Reader.WriteTo: invalid WriteString count");
            }
            r.i += int64(m);
            n = int64(m);
            if (m != len(s) && err == null)
            {
                err = io.ErrShortWrite;
            }
            return;
        });

        // Reset resets the Reader to be reading from s.
        private static void Reset(this ref Reader r, @string s)
        {
            r.Value = new Reader(s,0,-1);

        }

        // NewReader returns a new Reader reading from s.
        // It is similar to bytes.NewBufferString but more efficient and read-only.
        public static ref Reader NewReader(@string s)
        {
            return ref new Reader(s,0,-1);
        }
    }
}
