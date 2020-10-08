// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bufio implements buffered I/O. It wraps an io.Reader or io.Writer
// object, creating another object (Reader or Writer) that also implements
// the interface but provides buffering and some help for textual I/O.
// package bufio -- go2cs converted at 2020 October 08 03:26:17 UTC
// import "bufio" ==> using bufio = go.bufio_package
// Original source: C:\Go\src\bufio\bufio.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class bufio_package
    {
        private static readonly long defaultBufSize = (long)4096L;


        public static var ErrInvalidUnreadByte = errors.New("bufio: invalid use of UnreadByte");        public static var ErrInvalidUnreadRune = errors.New("bufio: invalid use of UnreadRune");        public static var ErrBufferFull = errors.New("bufio: buffer full");        public static var ErrNegativeCount = errors.New("bufio: negative count");

        // Buffered input.

        // Reader implements buffering for an io.Reader object.
        public partial struct Reader
        {
            public slice<byte> buf;
            public io.Reader rd; // reader provided by the client
            public long r; // buf read and write positions
            public long w; // buf read and write positions
            public error err;
            public long lastByte; // last byte read for UnreadByte; -1 means invalid
            public long lastRuneSize; // size of last rune read for UnreadRune; -1 means invalid
        }

        private static readonly long minReadBufferSize = (long)16L;

        private static readonly long maxConsecutiveEmptyReads = (long)100L;

        // NewReaderSize returns a new Reader whose buffer has at least the specified
        // size. If the argument io.Reader is already a Reader with large enough
        // size, it returns the underlying Reader.


        // NewReaderSize returns a new Reader whose buffer has at least the specified
        // size. If the argument io.Reader is already a Reader with large enough
        // size, it returns the underlying Reader.
        public static ptr<Reader> NewReaderSize(io.Reader rd, long size)
        { 
            // Is it already a Reader?
            ptr<Reader> (b, ok) = rd._<ptr<Reader>>();
            if (ok && len(b.buf) >= size)
            {
                return _addr_b!;
            }

            if (size < minReadBufferSize)
            {
                size = minReadBufferSize;
            }

            ptr<Reader> r = @new<Reader>();
            r.reset(make_slice<byte>(size), rd);
            return _addr_r!;

        }

        // NewReader returns a new Reader whose buffer has the default size.
        public static ptr<Reader> NewReader(io.Reader rd)
        {
            return _addr_NewReaderSize(rd, defaultBufSize)!;
        }

        // Size returns the size of the underlying buffer in bytes.
        private static long Size(this ptr<Reader> _addr_b)
        {
            ref Reader b = ref _addr_b.val;

            return len(b.buf);
        }

        // Reset discards any buffered data, resets all state, and switches
        // the buffered reader to read from r.
        private static void Reset(this ptr<Reader> _addr_b, io.Reader r)
        {
            ref Reader b = ref _addr_b.val;

            b.reset(b.buf, r);
        }

        private static void reset(this ptr<Reader> _addr_b, slice<byte> buf, io.Reader r)
        {
            ref Reader b = ref _addr_b.val;

            b.val = new Reader(buf:buf,rd:r,lastByte:-1,lastRuneSize:-1,);
        }

        private static var errNegativeRead = errors.New("bufio: reader returned negative count from Read");

        // fill reads a new chunk into the buffer.
        private static void fill(this ptr<Reader> _addr_b) => func((_, panic, __) =>
        {
            ref Reader b = ref _addr_b.val;
 
            // Slide existing data to beginning.
            if (b.r > 0L)
            {
                copy(b.buf, b.buf[b.r..b.w]);
                b.w -= b.r;
                b.r = 0L;
            }

            if (b.w >= len(b.buf))
            {
                panic("bufio: tried to fill full buffer");
            } 

            // Read new data: try a limited number of times.
            for (var i = maxConsecutiveEmptyReads; i > 0L; i--)
            {
                var (n, err) = b.rd.Read(b.buf[b.w..]);
                if (n < 0L)
                {
                    panic(errNegativeRead);
                }

                b.w += n;
                if (err != null)
                {
                    b.err = err;
                    return ;
                }

                if (n > 0L)
                {
                    return ;
                }

            }

            b.err = io.ErrNoProgress;

        });

        private static error readErr(this ptr<Reader> _addr_b)
        {
            ref Reader b = ref _addr_b.val;

            var err = b.err;
            b.err = null;
            return error.As(err)!;
        }

        // Peek returns the next n bytes without advancing the reader. The bytes stop
        // being valid at the next read call. If Peek returns fewer than n bytes, it
        // also returns an error explaining why the read is short. The error is
        // ErrBufferFull if n is larger than b's buffer size.
        //
        // Calling Peek prevents a UnreadByte or UnreadRune call from succeeding
        // until the next read operation.
        private static (slice<byte>, error) Peek(this ptr<Reader> _addr_b, long n)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Reader b = ref _addr_b.val;

            if (n < 0L)
            {
                return (null, error.As(ErrNegativeCount)!);
            }

            b.lastByte = -1L;
            b.lastRuneSize = -1L;

            while (b.w - b.r < n && b.w - b.r < len(b.buf) && b.err == null)
            {
                b.fill(); // b.w-b.r < len(b.buf) => buffer is not full
            }


            if (n > len(b.buf))
            {
                return (b.buf[b.r..b.w], error.As(ErrBufferFull)!);
            } 

            // 0 <= n <= len(b.buf)
            error err = default!;
            {
                var avail = b.w - b.r;

                if (avail < n)
                { 
                    // not enough data in buffer
                    n = avail;
                    err = error.As(b.readErr())!;
                    if (err == null)
                    {
                        err = error.As(ErrBufferFull)!;
                    }

                }

            }

            return (b.buf[b.r..b.r + n], error.As(err)!);

        }

        // Discard skips the next n bytes, returning the number of bytes discarded.
        //
        // If Discard skips fewer than n bytes, it also returns an error.
        // If 0 <= n <= b.Buffered(), Discard is guaranteed to succeed without
        // reading from the underlying io.Reader.
        private static (long, error) Discard(this ptr<Reader> _addr_b, long n)
        {
            long discarded = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            if (n < 0L)
            {
                return (0L, error.As(ErrNegativeCount)!);
            }

            if (n == 0L)
            {
                return ;
            }

            var remain = n;
            while (true)
            {
                var skip = b.Buffered();
                if (skip == 0L)
                {
                    b.fill();
                    skip = b.Buffered();
                }

                if (skip > remain)
                {
                    skip = remain;
                }

                b.r += skip;
                remain -= skip;
                if (remain == 0L)
                {
                    return (n, error.As(null!)!);
                }

                if (b.err != null)
                {
                    return (n - remain, error.As(b.readErr())!);
                }

            }


        }

        // Read reads data into p.
        // It returns the number of bytes read into p.
        // The bytes are taken from at most one Read on the underlying Reader,
        // hence n may be less than len(p).
        // To read exactly len(p) bytes, use io.ReadFull(b, p).
        // At EOF, the count will be zero and err will be io.EOF.
        private static (long, error) Read(this ptr<Reader> _addr_b, slice<byte> p) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            n = len(p);
            if (n == 0L)
            {
                if (b.Buffered() > 0L)
                {
                    return (0L, error.As(null!)!);
                }

                return (0L, error.As(b.readErr())!);

            }

            if (b.r == b.w)
            {
                if (b.err != null)
                {
                    return (0L, error.As(b.readErr())!);
                }

                if (len(p) >= len(b.buf))
                { 
                    // Large read, empty buffer.
                    // Read directly into p to avoid copy.
                    n, b.err = b.rd.Read(p);
                    if (n < 0L)
                    {
                        panic(errNegativeRead);
                    }

                    if (n > 0L)
                    {
                        b.lastByte = int(p[n - 1L]);
                        b.lastRuneSize = -1L;
                    }

                    return (n, error.As(b.readErr())!);

                } 
                // One read.
                // Do not use b.fill, which will loop.
                b.r = 0L;
                b.w = 0L;
                n, b.err = b.rd.Read(b.buf);
                if (n < 0L)
                {
                    panic(errNegativeRead);
                }

                if (n == 0L)
                {
                    return (0L, error.As(b.readErr())!);
                }

                b.w += n;

            } 

            // copy as much as we can
            n = copy(p, b.buf[b.r..b.w]);
            b.r += n;
            b.lastByte = int(b.buf[b.r - 1L]);
            b.lastRuneSize = -1L;
            return (n, error.As(null!)!);

        });

        // ReadByte reads and returns a single byte.
        // If no byte is available, returns an error.
        private static (byte, error) ReadByte(this ptr<Reader> _addr_b)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref Reader b = ref _addr_b.val;

            b.lastRuneSize = -1L;
            while (b.r == b.w)
            {
                if (b.err != null)
                {
                    return (0L, error.As(b.readErr())!);
                }

                b.fill(); // buffer is empty
            }

            var c = b.buf[b.r];
            b.r++;
            b.lastByte = int(c);
            return (c, error.As(null!)!);

        }

        // UnreadByte unreads the last byte. Only the most recently read byte can be unread.
        //
        // UnreadByte returns an error if the most recent method called on the
        // Reader was not a read operation. Notably, Peek is not considered a
        // read operation.
        private static error UnreadByte(this ptr<Reader> _addr_b)
        {
            ref Reader b = ref _addr_b.val;

            if (b.lastByte < 0L || b.r == 0L && b.w > 0L)
            {
                return error.As(ErrInvalidUnreadByte)!;
            } 
            // b.r > 0 || b.w == 0
            if (b.r > 0L)
            {
                b.r--;
            }
            else
            { 
                // b.r == 0 && b.w == 0
                b.w = 1L;

            }

            b.buf[b.r] = byte(b.lastByte);
            b.lastByte = -1L;
            b.lastRuneSize = -1L;
            return error.As(null!)!;

        }

        // ReadRune reads a single UTF-8 encoded Unicode character and returns the
        // rune and its size in bytes. If the encoded rune is invalid, it consumes one byte
        // and returns unicode.ReplacementChar (U+FFFD) with a size of 1.
        private static (int, long, error) ReadRune(this ptr<Reader> _addr_b)
        {
            int r = default;
            long size = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            while (b.r + utf8.UTFMax > b.w && !utf8.FullRune(b.buf[b.r..b.w]) && b.err == null && b.w - b.r < len(b.buf))
            {
                b.fill(); // b.w-b.r < len(buf) => buffer is not full
            }

            b.lastRuneSize = -1L;
            if (b.r == b.w)
            {
                return (0L, 0L, error.As(b.readErr())!);
            }

            r = rune(b.buf[b.r]);
            size = 1L;
            if (r >= utf8.RuneSelf)
            {
                r, size = utf8.DecodeRune(b.buf[b.r..b.w]);
            }

            b.r += size;
            b.lastByte = int(b.buf[b.r - 1L]);
            b.lastRuneSize = size;
            return (r, size, error.As(null!)!);

        }

        // UnreadRune unreads the last rune. If the most recent method called on
        // the Reader was not a ReadRune, UnreadRune returns an error. (In this
        // regard it is stricter than UnreadByte, which will unread the last byte
        // from any read operation.)
        private static error UnreadRune(this ptr<Reader> _addr_b)
        {
            ref Reader b = ref _addr_b.val;

            if (b.lastRuneSize < 0L || b.r < b.lastRuneSize)
            {
                return error.As(ErrInvalidUnreadRune)!;
            }

            b.r -= b.lastRuneSize;
            b.lastByte = -1L;
            b.lastRuneSize = -1L;
            return error.As(null!)!;

        }

        // Buffered returns the number of bytes that can be read from the current buffer.
        private static long Buffered(this ptr<Reader> _addr_b)
        {
            ref Reader b = ref _addr_b.val;

            return b.w - b.r;
        }

        // ReadSlice reads until the first occurrence of delim in the input,
        // returning a slice pointing at the bytes in the buffer.
        // The bytes stop being valid at the next read.
        // If ReadSlice encounters an error before finding a delimiter,
        // it returns all the data in the buffer and the error itself (often io.EOF).
        // ReadSlice fails with error ErrBufferFull if the buffer fills without a delim.
        // Because the data returned from ReadSlice will be overwritten
        // by the next I/O operation, most clients should use
        // ReadBytes or ReadString instead.
        // ReadSlice returns err != nil if and only if line does not end in delim.
        private static (slice<byte>, error) ReadSlice(this ptr<Reader> _addr_b, byte delim)
        {
            slice<byte> line = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            long s = 0L; // search start index
            while (true)
            { 
                // Search buffer.
                {
                    var i__prev1 = i;

                    var i = bytes.IndexByte(b.buf[b.r + s..b.w], delim);

                    if (i >= 0L)
                    {
                        i += s;
                        line = b.buf[b.r..b.r + i + 1L];
                        b.r += i + 1L;
                        break;
                    } 

                    // Pending error?

                    i = i__prev1;

                } 

                // Pending error?
                if (b.err != null)
                {
                    line = b.buf[b.r..b.w];
                    b.r = b.w;
                    err = b.readErr();
                    break;
                } 

                // Buffer full?
                if (b.Buffered() >= len(b.buf))
                {
                    b.r = b.w;
                    line = b.buf;
                    err = ErrBufferFull;
                    break;
                }

                s = b.w - b.r; // do not rescan area we scanned before

                b.fill(); // buffer is not full
            } 

            // Handle last byte, if any.
 

            // Handle last byte, if any.
            {
                var i__prev1 = i;

                i = len(line) - 1L;

                if (i >= 0L)
                {
                    b.lastByte = int(line[i]);
                    b.lastRuneSize = -1L;
                }

                i = i__prev1;

            }


            return ;

        }

        // ReadLine is a low-level line-reading primitive. Most callers should use
        // ReadBytes('\n') or ReadString('\n') instead or use a Scanner.
        //
        // ReadLine tries to return a single line, not including the end-of-line bytes.
        // If the line was too long for the buffer then isPrefix is set and the
        // beginning of the line is returned. The rest of the line will be returned
        // from future calls. isPrefix will be false when returning the last fragment
        // of the line. The returned buffer is only valid until the next call to
        // ReadLine. ReadLine either returns a non-nil line or it returns an error,
        // never both.
        //
        // The text returned from ReadLine does not include the line end ("\r\n" or "\n").
        // No indication or error is given if the input ends without a final line end.
        // Calling UnreadByte after ReadLine will always unread the last byte read
        // (possibly a character belonging to the line end) even if that byte is not
        // part of the line returned by ReadLine.
        private static (slice<byte>, bool, error) ReadLine(this ptr<Reader> _addr_b) => func((_, panic, __) =>
        {
            slice<byte> line = default;
            bool isPrefix = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            line, err = b.ReadSlice('\n');
            if (err == ErrBufferFull)
            { 
                // Handle the case where "\r\n" straddles the buffer.
                if (len(line) > 0L && line[len(line) - 1L] == '\r')
                { 
                    // Put the '\r' back on buf and drop it from line.
                    // Let the next call to ReadLine check for "\r\n".
                    if (b.r == 0L)
                    { 
                        // should be unreachable
                        panic("bufio: tried to rewind past start of buffer");

                    }

                    b.r--;
                    line = line[..len(line) - 1L];

                }

                return (line, true, error.As(null!)!);

            }

            if (len(line) == 0L)
            {
                if (err != null)
                {
                    line = null;
                }

                return ;

            }

            err = null;

            if (line[len(line) - 1L] == '\n')
            {
                long drop = 1L;
                if (len(line) > 1L && line[len(line) - 2L] == '\r')
                {
                    drop = 2L;
                }

                line = line[..len(line) - drop];

            }

            return ;

        });

        // collectFragments reads until the first occurrence of delim in the input. It
        // returns (slice of full buffers, remaining bytes before delim, total number
        // of bytes in the combined first two elements, error).
        // The complete result is equal to
        // `bytes.Join(append(fullBuffers, finalFragment), nil)`, which has a
        // length of `totalLen`. The result is strucured in this way to allow callers
        // to minimize allocations and copies.
        private static (slice<slice<byte>>, slice<byte>, long, error) collectFragments(this ptr<Reader> _addr_b, byte delim)
        {
            slice<slice<byte>> fullBuffers = default;
            slice<byte> finalFragment = default;
            long totalLen = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            slice<byte> frag = default; 
            // Use ReadSlice to look for delim, accumulating full buffers.
            while (true)
            {
                error e = default!;
                frag, e = b.ReadSlice(delim);
                if (e == null)
                { // got final fragment
                    break;

                }

                if (e != ErrBufferFull)
                { // unexpected error
                    err = e;
                    break;

                } 

                // Make a copy of the buffer.
                var buf = make_slice<byte>(len(frag));
                copy(buf, frag);
                fullBuffers = append(fullBuffers, buf);
                totalLen += len(buf);

            }


            totalLen += len(frag);
            return (fullBuffers, frag, totalLen, error.As(err)!);

        }

        // ReadBytes reads until the first occurrence of delim in the input,
        // returning a slice containing the data up to and including the delimiter.
        // If ReadBytes encounters an error before finding a delimiter,
        // it returns the data read before the error and the error itself (often io.EOF).
        // ReadBytes returns err != nil if and only if the returned data does not end in
        // delim.
        // For simple uses, a Scanner may be more convenient.
        private static (slice<byte>, error) ReadBytes(this ptr<Reader> _addr_b, byte delim)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Reader b = ref _addr_b.val;

            var (full, frag, n, err) = b.collectFragments(delim); 
            // Allocate new buffer to hold the full pieces and the fragment.
            var buf = make_slice<byte>(n);
            n = 0L; 
            // Copy full pieces and fragment in.
            foreach (var (i) in full)
            {
                n += copy(buf[n..], full[i]);
            }
            copy(buf[n..], frag);
            return (buf, error.As(err)!);

        }

        // ReadString reads until the first occurrence of delim in the input,
        // returning a string containing the data up to and including the delimiter.
        // If ReadString encounters an error before finding a delimiter,
        // it returns the data read before the error and the error itself (often io.EOF).
        // ReadString returns err != nil if and only if the returned data does not end in
        // delim.
        // For simple uses, a Scanner may be more convenient.
        private static (@string, error) ReadString(this ptr<Reader> _addr_b, byte delim)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref Reader b = ref _addr_b.val;

            var (full, frag, n, err) = b.collectFragments(delim); 
            // Allocate new buffer to hold the full pieces and the fragment.
            strings.Builder buf = default;
            buf.Grow(n); 
            // Copy full pieces and fragment in.
            foreach (var (_, fb) in full)
            {
                buf.Write(fb);
            }
            buf.Write(frag);
            return (buf.String(), error.As(err)!);

        }

        // WriteTo implements io.WriterTo.
        // This may make multiple calls to the Read method of the underlying Reader.
        // If the underlying reader supports the WriteTo method,
        // this calls the underlying WriteTo without buffering.
        private static (long, error) WriteTo(this ptr<Reader> _addr_b, io.Writer w)
        {
            long n = default;
            error err = default!;
            ref Reader b = ref _addr_b.val;

            n, err = b.writeBuf(w);
            if (err != null)
            {
                return ;
            }

            {
                io.WriterTo (r, ok) = b.rd._<io.WriterTo>();

                if (ok)
                {
                    var (m, err) = r.WriteTo(w);
                    n += m;
                    return (n, error.As(err)!);
                }

            }


            {
                io.ReaderFrom (w, ok) = w._<io.ReaderFrom>();

                if (ok)
                {
                    (m, err) = w.ReadFrom(b.rd);
                    n += m;
                    return (n, error.As(err)!);
                }

            }


            if (b.w - b.r < len(b.buf))
            {
                b.fill(); // buffer not full
            }

            while (b.r < b.w)
            { 
                // b.r < b.w => buffer is not empty
                (m, err) = b.writeBuf(w);
                n += m;
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                b.fill(); // buffer is empty
            }


            if (b.err == io.EOF)
            {
                b.err = null;
            }

            return (n, error.As(b.readErr())!);

        }

        private static var errNegativeWrite = errors.New("bufio: writer returned negative count from Write");

        // writeBuf writes the Reader's buffer to the writer.
        private static (long, error) writeBuf(this ptr<Reader> _addr_b, io.Writer w) => func((_, panic, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref Reader b = ref _addr_b.val;

            var (n, err) = w.Write(b.buf[b.r..b.w]);
            if (n < 0L)
            {
                panic(errNegativeWrite);
            }

            b.r += n;
            return (int64(n), error.As(err)!);

        });

        // buffered output

        // Writer implements buffering for an io.Writer object.
        // If an error occurs writing to a Writer, no more data will be
        // accepted and all subsequent writes, and Flush, will return the error.
        // After all data has been written, the client should call the
        // Flush method to guarantee all data has been forwarded to
        // the underlying io.Writer.
        public partial struct Writer
        {
            public error err;
            public slice<byte> buf;
            public long n;
            public io.Writer wr;
        }

        // NewWriterSize returns a new Writer whose buffer has at least the specified
        // size. If the argument io.Writer is already a Writer with large enough
        // size, it returns the underlying Writer.
        public static ptr<Writer> NewWriterSize(io.Writer w, long size)
        { 
            // Is it already a Writer?
            ptr<Writer> (b, ok) = w._<ptr<Writer>>();
            if (ok && len(b.buf) >= size)
            {
                return _addr_b!;
            }

            if (size <= 0L)
            {
                size = defaultBufSize;
            }

            return addr(new Writer(buf:make([]byte,size),wr:w,));

        }

        // NewWriter returns a new Writer whose buffer has the default size.
        public static ptr<Writer> NewWriter(io.Writer w)
        {
            return _addr_NewWriterSize(w, defaultBufSize)!;
        }

        // Size returns the size of the underlying buffer in bytes.
        private static long Size(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            return len(b.buf);
        }

        // Reset discards any unflushed buffered data, clears any error, and
        // resets b to write its output to w.
        private static void Reset(this ptr<Writer> _addr_b, io.Writer w)
        {
            ref Writer b = ref _addr_b.val;

            b.err = null;
            b.n = 0L;
            b.wr = w;
        }

        // Flush writes any buffered data to the underlying io.Writer.
        private static error Flush(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            if (b.err != null)
            {
                return error.As(b.err)!;
            }

            if (b.n == 0L)
            {
                return error.As(null!)!;
            }

            var (n, err) = b.wr.Write(b.buf[0L..b.n]);
            if (n < b.n && err == null)
            {
                err = io.ErrShortWrite;
            }

            if (err != null)
            {
                if (n > 0L && n < b.n)
                {
                    copy(b.buf[0L..b.n - n], b.buf[n..b.n]);
                }

                b.n -= n;
                b.err = err;
                return error.As(err)!;

            }

            b.n = 0L;
            return error.As(null!)!;

        }

        // Available returns how many bytes are unused in the buffer.
        private static long Available(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            return len(b.buf) - b.n;
        }

        // Buffered returns the number of bytes that have been written into the current buffer.
        private static long Buffered(this ptr<Writer> _addr_b)
        {
            ref Writer b = ref _addr_b.val;

            return b.n;
        }

        // Write writes the contents of p into the buffer.
        // It returns the number of bytes written.
        // If nn < len(p), it also returns an error explaining
        // why the write is short.
        private static (long, error) Write(this ptr<Writer> _addr_b, slice<byte> p)
        {
            long nn = default;
            error err = default!;
            ref Writer b = ref _addr_b.val;

            while (len(p) > b.Available() && b.err == null)
            {
                long n = default;
                if (b.Buffered() == 0L)
                { 
                    // Large write, empty buffer.
                    // Write directly from p to avoid copy.
                    n, b.err = b.wr.Write(p);

                }
                else
                {
                    n = copy(b.buf[b.n..], p);
                    b.n += n;
                    b.Flush();
                }

                nn += n;
                p = p[n..];

            }

            if (b.err != null)
            {
                return (nn, error.As(b.err)!);
            }

            n = copy(b.buf[b.n..], p);
            b.n += n;
            nn += n;
            return (nn, error.As(null!)!);

        }

        // WriteByte writes a single byte.
        private static error WriteByte(this ptr<Writer> _addr_b, byte c)
        {
            ref Writer b = ref _addr_b.val;

            if (b.err != null)
            {
                return error.As(b.err)!;
            }

            if (b.Available() <= 0L && b.Flush() != null)
            {
                return error.As(b.err)!;
            }

            b.buf[b.n] = c;
            b.n++;
            return error.As(null!)!;

        }

        // WriteRune writes a single Unicode code point, returning
        // the number of bytes written and any error.
        private static (long, error) WriteRune(this ptr<Writer> _addr_b, int r)
        {
            long size = default;
            error err = default!;
            ref Writer b = ref _addr_b.val;

            if (r < utf8.RuneSelf)
            {
                err = b.WriteByte(byte(r));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                return (1L, error.As(null!)!);

            }

            if (b.err != null)
            {
                return (0L, error.As(b.err)!);
            }

            var n = b.Available();
            if (n < utf8.UTFMax)
            {
                b.Flush();

                if (b.err != null)
                {
                    return (0L, error.As(b.err)!);
                }

                n = b.Available();
                if (n < utf8.UTFMax)
                { 
                    // Can only happen if buffer is silly small.
                    return b.WriteString(string(r));

                }

            }

            size = utf8.EncodeRune(b.buf[b.n..], r);
            b.n += size;
            return (size, error.As(null!)!);

        }

        // WriteString writes a string.
        // It returns the number of bytes written.
        // If the count is less than len(s), it also returns an error explaining
        // why the write is short.
        private static (long, error) WriteString(this ptr<Writer> _addr_b, @string s)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Writer b = ref _addr_b.val;

            long nn = 0L;
            while (len(s) > b.Available() && b.err == null)
            {
                var n = copy(b.buf[b.n..], s);
                b.n += n;
                nn += n;
                s = s[n..];
                b.Flush();
            }

            if (b.err != null)
            {
                return (nn, error.As(b.err)!);
            }

            n = copy(b.buf[b.n..], s);
            b.n += n;
            nn += n;
            return (nn, error.As(null!)!);

        }

        // ReadFrom implements io.ReaderFrom. If the underlying writer
        // supports the ReadFrom method, and b has no buffered data yet,
        // this calls the underlying ReadFrom without buffering.
        private static (long, error) ReadFrom(this ptr<Writer> _addr_b, io.Reader r)
        {
            long n = default;
            error err = default!;
            ref Writer b = ref _addr_b.val;

            if (b.err != null)
            {
                return (0L, error.As(b.err)!);
            }

            if (b.Buffered() == 0L)
            {
                {
                    io.ReaderFrom (w, ok) = b.wr._<io.ReaderFrom>();

                    if (ok)
                    {
                        n, err = w.ReadFrom(r);
                        b.err = err;
                        return (n, error.As(err)!);
                    }

                }

            }

            long m = default;
            while (true)
            {
                if (b.Available() == 0L)
                {
                    {
                        var err1 = b.Flush();

                        if (err1 != null)
                        {
                            return (n, error.As(err1)!);
                        }

                    }

                }

                long nr = 0L;
                while (nr < maxConsecutiveEmptyReads)
                {
                    m, err = r.Read(b.buf[b.n..]);
                    if (m != 0L || err != null)
                    {
                        break;
                    }

                    nr++;

                }

                if (nr == maxConsecutiveEmptyReads)
                {
                    return (n, error.As(io.ErrNoProgress)!);
                }

                b.n += m;
                n += int64(m);
                if (err != null)
                {
                    break;
                }

            }

            if (err == io.EOF)
            { 
                // If we filled the buffer exactly, flush preemptively.
                if (b.Available() == 0L)
                {
                    err = b.Flush();
                }
                else
                {
                    err = null;
                }

            }

            return (n, error.As(err)!);

        }

        // buffered input and output

        // ReadWriter stores pointers to a Reader and a Writer.
        // It implements io.ReadWriter.
        public partial struct ReadWriter
        {
            public ref ptr<Reader> ptr<Reader> => ref ptr<Reader>_ptr;
            public ref ptr<Writer> ptr<Writer> => ref ptr<Writer>_ptr;
        }

        // NewReadWriter allocates a new ReadWriter that dispatches to r and w.
        public static ptr<ReadWriter> NewReadWriter(ptr<Reader> _addr_r, ptr<Writer> _addr_w)
        {
            ref Reader r = ref _addr_r.val;
            ref Writer w = ref _addr_w.val;

            return addr(new ReadWriter(r,w));
        }
    }
}
