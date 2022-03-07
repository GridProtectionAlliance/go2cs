// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2022 March 06 22:14:01 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Program Files\Go\src\bytes\buffer.go
// Simple byte buffer for marshaling data.

using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go;

public static partial class bytes_package {

    // smallBufferSize is an initial allocation minimal capacity.
private static readonly nint smallBufferSize = 64;

// A Buffer is a variable-sized buffer of bytes with Read and Write methods.
// The zero value for Buffer is an empty buffer ready to use.


// A Buffer is a variable-sized buffer of bytes with Read and Write methods.
// The zero value for Buffer is an empty buffer ready to use.
public partial struct Buffer {
    public slice<byte> buf; // contents are the bytes buf[off : len(buf)]
    public nint off; // read at &buf[off], write at &buf[len(buf)]
    public readOp lastRead; // last read operation, so that Unread* can work correctly.
}

// The readOp constants describe the last action performed on
// the buffer, so that UnreadRune and UnreadByte can check for
// invalid usage. opReadRuneX constants are chosen such that
// converted to int they correspond to the rune size that was read.
private partial struct readOp { // : sbyte
}

// Don't use iota for these, as the values need to correspond with the
// names and comments, which is easier to see when being explicit.
private static readonly readOp opRead = -1; // Any other read operation.
private static readonly readOp opInvalid = 0; // Non-read operation.
private static readonly readOp opReadRune1 = 1; // Read rune of size 1.
private static readonly readOp opReadRune2 = 2; // Read rune of size 2.
private static readonly readOp opReadRune3 = 3; // Read rune of size 3.
private static readonly readOp opReadRune4 = 4; // Read rune of size 4.

// ErrTooLarge is passed to panic if memory cannot be allocated to store data in a buffer.
public static var ErrTooLarge = errors.New("bytes.Buffer: too large");
private static var errNegativeRead = errors.New("bytes.Buffer: reader returned negative count from Read");

private static readonly var maxInt = int(~uint(0) >> 1);

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
private static slice<byte> Bytes(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    return b.buf[(int)b.off..];
}

// String returns the contents of the unread portion of the buffer
// as a string. If the Buffer is a nil pointer, it returns "<nil>".
//
// To build strings more efficiently, see the strings.Builder type.
private static @string String(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    if (b == null) { 
        // Special case, useful in debugging.
        return "<nil>";

    }
    return string(b.buf[(int)b.off..]);

}

// empty reports whether the unread portion of the buffer is empty.
private static bool empty(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    return len(b.buf) <= b.off;
}

// Len returns the number of bytes of the unread portion of the buffer;
// b.Len() == len(b.Bytes()).
private static nint Len(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    return len(b.buf) - b.off;
}

// Cap returns the capacity of the buffer's underlying byte slice, that is, the
// total space allocated for the buffer's data.
private static nint Cap(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    return cap(b.buf);
}

// Truncate discards all but the first n unread bytes from the buffer
// but continues to use the same allocated storage.
// It panics if n is negative or greater than the length of the buffer.
private static void Truncate(this ptr<Buffer> _addr_b, nint n) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    if (n == 0) {
        b.Reset();
        return ;
    }
    b.lastRead = opInvalid;
    if (n < 0 || n > b.Len()) {
        panic("bytes.Buffer: truncation out of range");
    }
    b.buf = b.buf[..(int)b.off + n];

});

// Reset resets the buffer to be empty,
// but it retains the underlying storage for use by future writes.
// Reset is the same as Truncate(0).
private static void Reset(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    b.buf = b.buf[..(int)0];
    b.off = 0;
    b.lastRead = opInvalid;
}

// tryGrowByReslice is a inlineable version of grow for the fast-case where the
// internal buffer only needs to be resliced.
// It returns the index where bytes should be written and whether it succeeded.
private static (nint, bool) tryGrowByReslice(this ptr<Buffer> _addr_b, nint n) {
    nint _p0 = default;
    bool _p0 = default;
    ref Buffer b = ref _addr_b.val;

    {
        var l = len(b.buf);

        if (n <= cap(b.buf) - l) {
            b.buf = b.buf[..(int)l + n];
            return (l, true);
        }
    }

    return (0, false);

}

// grow grows the buffer to guarantee space for n more bytes.
// It returns the index where bytes should be written.
// If the buffer can't grow it will panic with ErrTooLarge.
private static nint grow(this ptr<Buffer> _addr_b, nint n) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    var m = b.Len(); 
    // If buffer is empty, reset to recover space.
    if (m == 0 && b.off != 0) {
        b.Reset();
    }
    {
        var (i, ok) = b.tryGrowByReslice(n);

        if (ok) {
            return i;
        }
    }

    if (b.buf == null && n <= smallBufferSize) {
        b.buf = make_slice<byte>(n, smallBufferSize);
        return 0;
    }
    var c = cap(b.buf);
    if (n <= c / 2 - m) { 
        // We can slide things down instead of allocating a new
        // slice. We only need m+n <= c to slide, but
        // we instead let capacity get twice as large so we
        // don't spend all our time copying.
        copy(b.buf, b.buf[(int)b.off..]);

    }
    else if (c > maxInt - c - n) {
        panic(ErrTooLarge);
    }
    else
 { 
        // Not enough space anywhere, we need to allocate.
        var buf = makeSlice(2 * c + n);
        copy(buf, b.buf[(int)b.off..]);
        b.buf = buf;

    }
    b.off = 0;
    b.buf = b.buf[..(int)m + n];
    return m;

});

// Grow grows the buffer's capacity, if necessary, to guarantee space for
// another n bytes. After Grow(n), at least n bytes can be written to the
// buffer without another allocation.
// If n is negative, Grow will panic.
// If the buffer can't grow it will panic with ErrTooLarge.
private static void Grow(this ptr<Buffer> _addr_b, nint n) => func((_, panic, _) => {
    ref Buffer b = ref _addr_b.val;

    if (n < 0) {
        panic("bytes.Buffer.Grow: negative count");
    }
    var m = b.grow(n);
    b.buf = b.buf[..(int)m];

});

// Write appends the contents of p to the buffer, growing the buffer as
// needed. The return value n is the length of p; err is always nil. If the
// buffer becomes too large, Write will panic with ErrTooLarge.
private static (nint, error) Write(this ptr<Buffer> _addr_b, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(len(p));
    if (!ok) {
        m = b.grow(len(p));
    }
    return (copy(b.buf[(int)m..], p), error.As(null!)!);

}

// WriteString appends the contents of s to the buffer, growing the buffer as
// needed. The return value n is the length of s; err is always nil. If the
// buffer becomes too large, WriteString will panic with ErrTooLarge.
private static (nint, error) WriteString(this ptr<Buffer> _addr_b, @string s) {
    nint n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(len(s));
    if (!ok) {
        m = b.grow(len(s));
    }
    return (copy(b.buf[(int)m..], s), error.As(null!)!);

}

// MinRead is the minimum slice size passed to a Read call by
// Buffer.ReadFrom. As long as the Buffer has at least MinRead bytes beyond
// what is required to hold the contents of r, ReadFrom will not grow the
// underlying buffer.
public static readonly nint MinRead = 512;

// ReadFrom reads data from r until EOF and appends it to the buffer, growing
// the buffer as needed. The return value n is the number of bytes read. Any
// error except io.EOF encountered during the read is also returned. If the
// buffer becomes too large, ReadFrom will panic with ErrTooLarge.


// ReadFrom reads data from r until EOF and appends it to the buffer, growing
// the buffer as needed. The return value n is the number of bytes read. Any
// error except io.EOF encountered during the read is also returned. If the
// buffer becomes too large, ReadFrom will panic with ErrTooLarge.
private static (long, error) ReadFrom(this ptr<Buffer> _addr_b, io.Reader r) => func((_, panic, _) => {
    long n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    while (true) {
        var i = b.grow(MinRead);
        b.buf = b.buf[..(int)i];
        var (m, e) = r.Read(b.buf[(int)i..(int)cap(b.buf)]);
        if (m < 0) {
            panic(errNegativeRead);
        }
        b.buf = b.buf[..(int)i + m];
        n += int64(m);
        if (e == io.EOF) {
            return (n, error.As(null!)!); // e is EOF, so return nil explicitly
        }
        if (e != null) {
            return (n, error.As(e)!);
        }
    }

});

// makeSlice allocates a slice of size n. If the allocation fails, it panics
// with ErrTooLarge.
private static slice<byte> makeSlice(nint n) => func((defer, panic, _) => { 
    // If the make fails, give a known error.
    defer(() => {
        if (recover() != null) {
            panic(ErrTooLarge);
        }
    }());
    return make_slice<byte>(n);

});

// WriteTo writes data to w until the buffer is drained or an error occurs.
// The return value n is the number of bytes written; it always fits into an
// int, but it is int64 to match the io.WriterTo interface. Any error
// encountered during the write is also returned.
private static (long, error) WriteTo(this ptr<Buffer> _addr_b, io.Writer w) => func((_, panic, _) => {
    long n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    {
        var nBytes = b.Len();

        if (nBytes > 0) {
            var (m, e) = w.Write(b.buf[(int)b.off..]);
            if (m > nBytes) {
                panic("bytes.Buffer.WriteTo: invalid Write count");
            }
            b.off += m;
            n = int64(m);
            if (e != null) {
                return (n, error.As(e)!);
            } 
            // all bytes should have been written, by definition of
            // Write method in io.Writer
            if (m != nBytes) {
                return (n, error.As(io.ErrShortWrite)!);
            }

        }
    } 
    // Buffer is now empty; reset.
    b.Reset();
    return (n, error.As(null!)!);

});

// WriteByte appends the byte c to the buffer, growing the buffer as needed.
// The returned error is always nil, but is included to match bufio.Writer's
// WriteByte. If the buffer becomes too large, WriteByte will panic with
// ErrTooLarge.
private static error WriteByte(this ptr<Buffer> _addr_b, byte c) {
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(1);
    if (!ok) {
        m = b.grow(1);
    }
    b.buf[m] = c;
    return error.As(null!)!;

}

// WriteRune appends the UTF-8 encoding of Unicode code point r to the
// buffer, returning its length and an error, which is always nil but is
// included to match bufio.Writer's WriteRune. The buffer is grown as needed;
// if it becomes too large, WriteRune will panic with ErrTooLarge.
private static (nint, error) WriteRune(this ptr<Buffer> _addr_b, int r) {
    nint n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;
 
    // Compare as uint32 to correctly handle negative runes.
    if (uint32(r) < utf8.RuneSelf) {
        b.WriteByte(byte(r));
        return (1, error.As(null!)!);
    }
    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(utf8.UTFMax);
    if (!ok) {
        m = b.grow(utf8.UTFMax);
    }
    n = utf8.EncodeRune(b.buf[(int)m..(int)m + utf8.UTFMax], r);
    b.buf = b.buf[..(int)m + n];
    return (n, error.As(null!)!);

}

// Read reads the next len(p) bytes from the buffer or until the buffer
// is drained. The return value n is the number of bytes read. If the
// buffer has no data to return, err is io.EOF (unless len(p) is zero);
// otherwise it is nil.
private static (nint, error) Read(this ptr<Buffer> _addr_b, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    if (b.empty()) { 
        // Buffer is empty, reset to recover space.
        b.Reset();
        if (len(p) == 0) {
            return (0, error.As(null!)!);
        }
        return (0, error.As(io.EOF)!);

    }
    n = copy(p, b.buf[(int)b.off..]);
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    return (n, error.As(null!)!);

}

// Next returns a slice containing the next n bytes from the buffer,
// advancing the buffer as if the bytes had been returned by Read.
// If there are fewer than n bytes in the buffer, Next returns the entire buffer.
// The slice is only valid until the next call to a read or write method.
private static slice<byte> Next(this ptr<Buffer> _addr_b, nint n) {
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    var m = b.Len();
    if (n > m) {
        n = m;
    }
    var data = b.buf[(int)b.off..(int)b.off + n];
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    return data;

}

// ReadByte reads and returns the next byte from the buffer.
// If no byte is available, it returns error io.EOF.
private static (byte, error) ReadByte(this ptr<Buffer> _addr_b) {
    byte _p0 = default;
    error _p0 = default!;
    ref Buffer b = ref _addr_b.val;

    if (b.empty()) { 
        // Buffer is empty, reset to recover space.
        b.Reset();
        return (0, error.As(io.EOF)!);

    }
    var c = b.buf[b.off];
    b.off++;
    b.lastRead = opRead;
    return (c, error.As(null!)!);

}

// ReadRune reads and returns the next UTF-8-encoded
// Unicode code point from the buffer.
// If no bytes are available, the error returned is io.EOF.
// If the bytes are an erroneous UTF-8 encoding, it
// consumes one byte and returns U+FFFD, 1.
private static (int, nint, error) ReadRune(this ptr<Buffer> _addr_b) {
    int r = default;
    nint size = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    if (b.empty()) { 
        // Buffer is empty, reset to recover space.
        b.Reset();
        return (0, 0, error.As(io.EOF)!);

    }
    var c = b.buf[b.off];
    if (c < utf8.RuneSelf) {
        b.off++;
        b.lastRead = opReadRune1;
        return (rune(c), 1, error.As(null!)!);
    }
    var (r, n) = utf8.DecodeRune(b.buf[(int)b.off..]);
    b.off += n;
    b.lastRead = readOp(n);
    return (r, n, error.As(null!)!);

}

// UnreadRune unreads the last rune returned by ReadRune.
// If the most recent read or write operation on the buffer was
// not a successful ReadRune, UnreadRune returns an error.  (In this regard
// it is stricter than UnreadByte, which will unread the last byte
// from any read operation.)
private static error UnreadRune(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    if (b.lastRead <= opInvalid) {
        return error.As(errors.New("bytes.Buffer: UnreadRune: previous operation was not a successful ReadRune"))!;
    }
    if (b.off >= int(b.lastRead)) {
        b.off -= int(b.lastRead);
    }
    b.lastRead = opInvalid;
    return error.As(null!)!;

}

private static var errUnreadByte = errors.New("bytes.Buffer: UnreadByte: previous operation was not a successful read");

// UnreadByte unreads the last byte returned by the most recent successful
// read operation that read at least one byte. If a write has happened since
// the last read, if the last read returned an error, or if the read read zero
// bytes, UnreadByte returns an error.
private static error UnreadByte(this ptr<Buffer> _addr_b) {
    ref Buffer b = ref _addr_b.val;

    if (b.lastRead == opInvalid) {
        return error.As(errUnreadByte)!;
    }
    b.lastRead = opInvalid;
    if (b.off > 0) {
        b.off--;
    }
    return error.As(null!)!;

}

// ReadBytes reads until the first occurrence of delim in the input,
// returning a slice containing the data up to and including the delimiter.
// If ReadBytes encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often io.EOF).
// ReadBytes returns err != nil if and only if the returned data does not end in
// delim.
private static (slice<byte>, error) ReadBytes(this ptr<Buffer> _addr_b, byte delim) {
    slice<byte> line = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    var (slice, err) = b.readSlice(delim); 
    // return a copy of slice. The buffer's backing array may
    // be overwritten by later calls.
    line = append(line, slice);
    return (line, error.As(err)!);

}

// readSlice is like ReadBytes but returns a reference to internal buffer data.
private static (slice<byte>, error) readSlice(this ptr<Buffer> _addr_b, byte delim) {
    slice<byte> line = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    var i = IndexByte(b.buf[(int)b.off..], delim);
    var end = b.off + i + 1;
    if (i < 0) {
        end = len(b.buf);
        err = io.EOF;
    }
    line = b.buf[(int)b.off..(int)end];
    b.off = end;
    b.lastRead = opRead;
    return (line, error.As(err)!);

}

// ReadString reads until the first occurrence of delim in the input,
// returning a string containing the data up to and including the delimiter.
// If ReadString encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often io.EOF).
// ReadString returns err != nil if and only if the returned data does not end
// in delim.
private static (@string, error) ReadString(this ptr<Buffer> _addr_b, byte delim) {
    @string line = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    var (slice, err) = b.readSlice(delim);
    return (string(slice), error.As(err)!);
}

// NewBuffer creates and initializes a new Buffer using buf as its
// initial contents. The new Buffer takes ownership of buf, and the
// caller should not use buf after this call. NewBuffer is intended to
// prepare a Buffer to read existing data. It can also be used to set
// the initial size of the internal buffer for writing. To do that,
// buf should have the desired capacity but a length of zero.
//
// In most cases, new(Buffer) (or just declaring a Buffer variable) is
// sufficient to initialize a Buffer.
public static ptr<Buffer> NewBuffer(slice<byte> buf) {
    return addr(new Buffer(buf:buf));
}

// NewBufferString creates and initializes a new Buffer using string s as its
// initial contents. It is intended to prepare a buffer to read an existing
// string.
//
// In most cases, new(Buffer) (or just declaring a Buffer variable) is
// sufficient to initialize a Buffer.
public static ptr<Buffer> NewBufferString(@string s) {
    return addr(new Buffer(buf:[]byte(s)));
}

} // end bytes_package
