// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2020 August 29 08:21:57 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\buffer.go
// Simple byte buffer for marshaling data.

using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class bytes_package
    {
        // A Buffer is a variable-sized buffer of bytes with Read and Write methods.
        // The zero value for Buffer is an empty buffer ready to use.
        public partial struct Buffer
        {
            public slice<byte> buf; // contents are the bytes buf[off : len(buf)]
            public long off; // read at &buf[off], write at &buf[len(buf)]
            public array<byte> bootstrap; // memory to hold first slice; helps small buffers avoid allocation.
            public readOp lastRead; // last read operation, so that Unread* can work correctly.

// FIXME: it would be advisable to align Buffer to cachelines to avoid false
// sharing.
        }

        // The readOp constants describe the last action performed on
        // the buffer, so that UnreadRune and UnreadByte can check for
        // invalid usage. opReadRuneX constants are chosen such that
        // converted to int they correspond to the rune size that was read.
        private partial struct readOp // : sbyte
        {
        }

        // Don't use iota for these, as the values need to correspond with the
        // names and comments, which is easier to see when being explicit.
        private static readonly readOp opRead = -1L; // Any other read operation.
        private static readonly readOp opInvalid = 0L; // Non-read operation.
        private static readonly readOp opReadRune1 = 1L; // Read rune of size 1.
        private static readonly readOp opReadRune2 = 2L; // Read rune of size 2.
        private static readonly readOp opReadRune3 = 3L; // Read rune of size 3.
        private static readonly readOp opReadRune4 = 4L; // Read rune of size 4.

        // ErrTooLarge is passed to panic if memory cannot be allocated to store data in a buffer.
        public static var ErrTooLarge = errors.New("bytes.Buffer: too large");
        private static var errNegativeRead = errors.New("bytes.Buffer: reader returned negative count from Read");

        private static readonly var maxInt = int(~uint(0L) >> (int)(1L));

        // Bytes returns a slice of length b.Len() holding the unread portion of the buffer.
        // The slice is valid for use only until the next buffer modification (that is,
        // only until the next call to a method like Read, Write, Reset, or Truncate).
        // The slice aliases the buffer content at least until the next buffer modification,
        // so immediate changes to the slice will affect the result of future reads.


        // Bytes returns a slice of length b.Len() holding the unread portion of the buffer.
        // The slice is valid for use only until the next buffer modification (that is,
        // only until the next call to a method like Read, Write, Reset, or Truncate).
        // The slice aliases the buffer content at least until the next buffer modification,
        // so immediate changes to the slice will affect the result of future reads.
        private static slice<byte> Bytes(this ref Buffer b)
        {
            return b.buf[b.off..];
        }

        // String returns the contents of the unread portion of the buffer
        // as a string. If the Buffer is a nil pointer, it returns "<nil>".
        //
        // To build strings more efficiently, see the strings.Builder type.
        private static @string String(this ref Buffer b)
        {
            if (b == null)
            { 
                // Special case, useful in debugging.
                return "<nil>";
            }
            return string(b.buf[b.off..]);
        }

        // empty returns whether the unread portion of the buffer is empty.
        private static bool empty(this ref Buffer b)
        {
            return len(b.buf) <= b.off;
        }

        // Len returns the number of bytes of the unread portion of the buffer;
        // b.Len() == len(b.Bytes()).
        private static long Len(this ref Buffer b)
        {
            return len(b.buf) - b.off;
        }

        // Cap returns the capacity of the buffer's underlying byte slice, that is, the
        // total space allocated for the buffer's data.
        private static long Cap(this ref Buffer b)
        {
            return cap(b.buf);
        }

        // Truncate discards all but the first n unread bytes from the buffer
        // but continues to use the same allocated storage.
        // It panics if n is negative or greater than the length of the buffer.
        private static void Truncate(this ref Buffer _b, long n) => func(_b, (ref Buffer b, Defer _, Panic panic, Recover __) =>
        {
            if (n == 0L)
            {
                b.Reset();
                return;
            }
            b.lastRead = opInvalid;
            if (n < 0L || n > b.Len())
            {
                panic("bytes.Buffer: truncation out of range");
            }
            b.buf = b.buf[..b.off + n];
        });

        // Reset resets the buffer to be empty,
        // but it retains the underlying storage for use by future writes.
        // Reset is the same as Truncate(0).
        private static void Reset(this ref Buffer b)
        {
            b.buf = b.buf[..0L];
            b.off = 0L;
            b.lastRead = opInvalid;
        }

        // tryGrowByReslice is a inlineable version of grow for the fast-case where the
        // internal buffer only needs to be resliced.
        // It returns the index where bytes should be written and whether it succeeded.
        private static (long, bool) tryGrowByReslice(this ref Buffer b, long n)
        {
            {
                var l = len(b.buf);

                if (n <= cap(b.buf) - l)
                {
                    b.buf = b.buf[..l + n];
                    return (l, true);
                }

            }
            return (0L, false);
        }

        // grow grows the buffer to guarantee space for n more bytes.
        // It returns the index where bytes should be written.
        // If the buffer can't grow it will panic with ErrTooLarge.
        private static long grow(this ref Buffer _b, long n) => func(_b, (ref Buffer b, Defer _, Panic panic, Recover __) =>
        {
            var m = b.Len(); 
            // If buffer is empty, reset to recover space.
            if (m == 0L && b.off != 0L)
            {
                b.Reset();
            } 
            // Try to grow by means of a reslice.
            {
                var (i, ok) = b.tryGrowByReslice(n);

                if (ok)
                {
                    return i;
                } 
                // Check if we can make use of bootstrap array.

            } 
            // Check if we can make use of bootstrap array.
            if (b.buf == null && n <= len(b.bootstrap))
            {
                b.buf = b.bootstrap[..n];
                return 0L;
            }
            var c = cap(b.buf);
            if (n <= c / 2L - m)
            { 
                // We can slide things down instead of allocating a new
                // slice. We only need m+n <= c to slide, but
                // we instead let capacity get twice as large so we
                // don't spend all our time copying.
                copy(b.buf, b.buf[b.off..]);
            }
            else if (c > maxInt - c - n)
            {
                panic(ErrTooLarge);
            }
            else
            { 
                // Not enough space anywhere, we need to allocate.
                var buf = makeSlice(2L * c + n);
                copy(buf, b.buf[b.off..]);
                b.buf = buf;
            } 
            // Restore b.off and len(b.buf).
            b.off = 0L;
            b.buf = b.buf[..m + n];
            return m;
        });

        // Grow grows the buffer's capacity, if necessary, to guarantee space for
        // another n bytes. After Grow(n), at least n bytes can be written to the
        // buffer without another allocation.
        // If n is negative, Grow will panic.
        // If the buffer can't grow it will panic with ErrTooLarge.
        private static void Grow(this ref Buffer _b, long n) => func(_b, (ref Buffer b, Defer _, Panic panic, Recover __) =>
        {
            if (n < 0L)
            {
                panic("bytes.Buffer.Grow: negative count");
            }
            var m = b.grow(n);
            b.buf = b.buf[..m];
        });

        // Write appends the contents of p to the buffer, growing the buffer as
        // needed. The return value n is the length of p; err is always nil. If the
        // buffer becomes too large, Write will panic with ErrTooLarge.
        private static (long, error) Write(this ref Buffer b, slice<byte> p)
        {
            b.lastRead = opInvalid;
            var (m, ok) = b.tryGrowByReslice(len(p));
            if (!ok)
            {
                m = b.grow(len(p));
            }
            return (copy(b.buf[m..], p), null);
        }

        // WriteString appends the contents of s to the buffer, growing the buffer as
        // needed. The return value n is the length of s; err is always nil. If the
        // buffer becomes too large, WriteString will panic with ErrTooLarge.
        private static (long, error) WriteString(this ref Buffer b, @string s)
        {
            b.lastRead = opInvalid;
            var (m, ok) = b.tryGrowByReslice(len(s));
            if (!ok)
            {
                m = b.grow(len(s));
            }
            return (copy(b.buf[m..], s), null);
        }

        // MinRead is the minimum slice size passed to a Read call by
        // Buffer.ReadFrom. As long as the Buffer has at least MinRead bytes beyond
        // what is required to hold the contents of r, ReadFrom will not grow the
        // underlying buffer.
        public static readonly long MinRead = 512L;

        // ReadFrom reads data from r until EOF and appends it to the buffer, growing
        // the buffer as needed. The return value n is the number of bytes read. Any
        // error except io.EOF encountered during the read is also returned. If the
        // buffer becomes too large, ReadFrom will panic with ErrTooLarge.


        // ReadFrom reads data from r until EOF and appends it to the buffer, growing
        // the buffer as needed. The return value n is the number of bytes read. Any
        // error except io.EOF encountered during the read is also returned. If the
        // buffer becomes too large, ReadFrom will panic with ErrTooLarge.
        private static (long, error) ReadFrom(this ref Buffer _b, io.Reader r) => func(_b, (ref Buffer b, Defer _, Panic panic, Recover __) =>
        {
            b.lastRead = opInvalid;
            while (true)
            {
                var i = b.grow(MinRead);
                var (m, e) = r.Read(b.buf[i..cap(b.buf)]);
                if (m < 0L)
                {
                    panic(errNegativeRead);
                }
                b.buf = b.buf[..i + m];
                n += int64(m);
                if (e == io.EOF)
                {
                    return (n, null); // e is EOF, so return nil explicitly
                }
                if (e != null)
                {
                    return (n, e);
                }
            }

        });

        // makeSlice allocates a slice of size n. If the allocation fails, it panics
        // with ErrTooLarge.
        private static slice<byte> makeSlice(long n) => func((defer, panic, _) =>
        { 
            // If the make fails, give a known error.
            defer(() =>
            {
                if (recover() != null)
                {
                    panic(ErrTooLarge);
                }
            }());
            return make_slice<byte>(n);
        });

        // WriteTo writes data to w until the buffer is drained or an error occurs.
        // The return value n is the number of bytes written; it always fits into an
        // int, but it is int64 to match the io.WriterTo interface. Any error
        // encountered during the write is also returned.
        private static (long, error) WriteTo(this ref Buffer _b, io.Writer w) => func(_b, (ref Buffer b, Defer _, Panic panic, Recover __) =>
        {
            b.lastRead = opInvalid;
            {
                var nBytes = b.Len();

                if (nBytes > 0L)
                {
                    var (m, e) = w.Write(b.buf[b.off..]);
                    if (m > nBytes)
                    {
                        panic("bytes.Buffer.WriteTo: invalid Write count");
                    }
                    b.off += m;
                    n = int64(m);
                    if (e != null)
                    {
                        return (n, e);
                    } 
                    // all bytes should have been written, by definition of
                    // Write method in io.Writer
                    if (m != nBytes)
                    {
                        return (n, io.ErrShortWrite);
                    }
                } 
                // Buffer is now empty; reset.

            } 
            // Buffer is now empty; reset.
            b.Reset();
            return (n, null);
        });

        // WriteByte appends the byte c to the buffer, growing the buffer as needed.
        // The returned error is always nil, but is included to match bufio.Writer's
        // WriteByte. If the buffer becomes too large, WriteByte will panic with
        // ErrTooLarge.
        private static error WriteByte(this ref Buffer b, byte c)
        {
            b.lastRead = opInvalid;
            var (m, ok) = b.tryGrowByReslice(1L);
            if (!ok)
            {
                m = b.grow(1L);
            }
            b.buf[m] = c;
            return error.As(null);
        }

        // WriteRune appends the UTF-8 encoding of Unicode code point r to the
        // buffer, returning its length and an error, which is always nil but is
        // included to match bufio.Writer's WriteRune. The buffer is grown as needed;
        // if it becomes too large, WriteRune will panic with ErrTooLarge.
        private static (long, error) WriteRune(this ref Buffer b, int r)
        {
            if (r < utf8.RuneSelf)
            {
                b.WriteByte(byte(r));
                return (1L, null);
            }
            b.lastRead = opInvalid;
            var (m, ok) = b.tryGrowByReslice(utf8.UTFMax);
            if (!ok)
            {
                m = b.grow(utf8.UTFMax);
            }
            n = utf8.EncodeRune(b.buf[m..m + utf8.UTFMax], r);
            b.buf = b.buf[..m + n];
            return (n, null);
        }

        // Read reads the next len(p) bytes from the buffer or until the buffer
        // is drained. The return value n is the number of bytes read. If the
        // buffer has no data to return, err is io.EOF (unless len(p) is zero);
        // otherwise it is nil.
        private static (long, error) Read(this ref Buffer b, slice<byte> p)
        {
            b.lastRead = opInvalid;
            if (b.empty())
            { 
                // Buffer is empty, reset to recover space.
                b.Reset();
                if (len(p) == 0L)
                {
                    return (0L, null);
                }
                return (0L, io.EOF);
            }
            n = copy(p, b.buf[b.off..]);
            b.off += n;
            if (n > 0L)
            {
                b.lastRead = opRead;
            }
            return (n, null);
        }

        // Next returns a slice containing the next n bytes from the buffer,
        // advancing the buffer as if the bytes had been returned by Read.
        // If there are fewer than n bytes in the buffer, Next returns the entire buffer.
        // The slice is only valid until the next call to a read or write method.
        private static slice<byte> Next(this ref Buffer b, long n)
        {
            b.lastRead = opInvalid;
            var m = b.Len();
            if (n > m)
            {
                n = m;
            }
            var data = b.buf[b.off..b.off + n];
            b.off += n;
            if (n > 0L)
            {
                b.lastRead = opRead;
            }
            return data;
        }

        // ReadByte reads and returns the next byte from the buffer.
        // If no byte is available, it returns error io.EOF.
        private static (byte, error) ReadByte(this ref Buffer b)
        {
            if (b.empty())
            { 
                // Buffer is empty, reset to recover space.
                b.Reset();
                return (0L, io.EOF);
            }
            var c = b.buf[b.off];
            b.off++;
            b.lastRead = opRead;
            return (c, null);
        }

        // ReadRune reads and returns the next UTF-8-encoded
        // Unicode code point from the buffer.
        // If no bytes are available, the error returned is io.EOF.
        // If the bytes are an erroneous UTF-8 encoding, it
        // consumes one byte and returns U+FFFD, 1.
        private static (int, long, error) ReadRune(this ref Buffer b)
        {
            if (b.empty())
            { 
                // Buffer is empty, reset to recover space.
                b.Reset();
                return (0L, 0L, io.EOF);
            }
            var c = b.buf[b.off];
            if (c < utf8.RuneSelf)
            {
                b.off++;
                b.lastRead = opReadRune1;
                return (rune(c), 1L, null);
            }
            var (r, n) = utf8.DecodeRune(b.buf[b.off..]);
            b.off += n;
            b.lastRead = readOp(n);
            return (r, n, null);
        }

        // UnreadRune unreads the last rune returned by ReadRune.
        // If the most recent read or write operation on the buffer was
        // not a successful ReadRune, UnreadRune returns an error.  (In this regard
        // it is stricter than UnreadByte, which will unread the last byte
        // from any read operation.)
        private static error UnreadRune(this ref Buffer b)
        {
            if (b.lastRead <= opInvalid)
            {
                return error.As(errors.New("bytes.Buffer: UnreadRune: previous operation was not a successful ReadRune"));
            }
            if (b.off >= int(b.lastRead))
            {
                b.off -= int(b.lastRead);
            }
            b.lastRead = opInvalid;
            return error.As(null);
        }

        // UnreadByte unreads the last byte returned by the most recent successful
        // read operation that read at least one byte. If a write has happened since
        // the last read, if the last read returned an error, or if the read read zero
        // bytes, UnreadByte returns an error.
        private static error UnreadByte(this ref Buffer b)
        {
            if (b.lastRead == opInvalid)
            {
                return error.As(errors.New("bytes.Buffer: UnreadByte: previous operation was not a successful read"));
            }
            b.lastRead = opInvalid;
            if (b.off > 0L)
            {
                b.off--;
            }
            return error.As(null);
        }

        // ReadBytes reads until the first occurrence of delim in the input,
        // returning a slice containing the data up to and including the delimiter.
        // If ReadBytes encounters an error before finding a delimiter,
        // it returns the data read before the error and the error itself (often io.EOF).
        // ReadBytes returns err != nil if and only if the returned data does not end in
        // delim.
        private static (slice<byte>, error) ReadBytes(this ref Buffer b, byte delim)
        {
            var (slice, err) = b.readSlice(delim); 
            // return a copy of slice. The buffer's backing array may
            // be overwritten by later calls.
            line = append(line, slice);
            return (line, err);
        }

        // readSlice is like ReadBytes but returns a reference to internal buffer data.
        private static (slice<byte>, error) readSlice(this ref Buffer b, byte delim)
        {
            var i = IndexByte(b.buf[b.off..], delim);
            var end = b.off + i + 1L;
            if (i < 0L)
            {
                end = len(b.buf);
                err = io.EOF;
            }
            line = b.buf[b.off..end];
            b.off = end;
            b.lastRead = opRead;
            return (line, err);
        }

        // ReadString reads until the first occurrence of delim in the input,
        // returning a string containing the data up to and including the delimiter.
        // If ReadString encounters an error before finding a delimiter,
        // it returns the data read before the error and the error itself (often io.EOF).
        // ReadString returns err != nil if and only if the returned data does not end
        // in delim.
        private static (@string, error) ReadString(this ref Buffer b, byte delim)
        {
            var (slice, err) = b.readSlice(delim);
            return (string(slice), err);
        }

        // NewBuffer creates and initializes a new Buffer using buf as its
        // initial contents. The new Buffer takes ownership of buf, and the
        // caller should not use buf after this call. NewBuffer is intended to
        // prepare a Buffer to read existing data. It can also be used to size
        // the internal buffer for writing. To do that, buf should have the
        // desired capacity but a length of zero.
        //
        // In most cases, new(Buffer) (or just declaring a Buffer variable) is
        // sufficient to initialize a Buffer.
        public static ref Buffer NewBuffer(slice<byte> buf)
        {
            return ref new Buffer(buf:buf);
        }

        // NewBufferString creates and initializes a new Buffer using string s as its
        // initial contents. It is intended to prepare a buffer to read an existing
        // string.
        //
        // In most cases, new(Buffer) (or just declaring a Buffer variable) is
        // sufficient to initialize a Buffer.
        public static ref Buffer NewBufferString(@string s)
        {
            return ref new Buffer(buf:[]byte(s));
        }
    }
}
