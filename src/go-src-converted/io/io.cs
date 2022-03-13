// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package io provides basic interfaces to I/O primitives.
// Its primary job is to wrap existing implementations of such primitives,
// such as those in package os, into shared public interfaces that
// abstract the functionality, plus some other related primitives.
//
// Because these interfaces and primitives wrap lower-level operations with
// various implementations, unless otherwise informed clients should not
// assume they are safe for parallel execution.

// package io -- go2cs converted at 2022 March 13 05:27:45 UTC
// import "io" ==> using io = go.io_package
// Original source: C:\Program Files\Go\src\io\io.go
namespace go;

using errors = errors_package;
using sync = sync_package;


// Seek whence values.

using System;
public static partial class io_package {

public static readonly nint SeekStart = 0; // seek relative to the origin of the file
public static readonly nint SeekCurrent = 1; // seek relative to the current offset
public static readonly nint SeekEnd = 2; // seek relative to the end

// ErrShortWrite means that a write accepted fewer bytes than requested
// but failed to return an explicit error.
public static var ErrShortWrite = errors.New("short write");

// errInvalidWrite means that a write returned an impossible count.
private static var errInvalidWrite = errors.New("invalid write result");

// ErrShortBuffer means that a read required a longer buffer than was provided.
public static var ErrShortBuffer = errors.New("short buffer");

// EOF is the error returned by Read when no more input is available.
// (Read must return EOF itself, not an error wrapping EOF,
// because callers will test for EOF using ==.)
// Functions should return EOF only to signal a graceful end of input.
// If the EOF occurs unexpectedly in a structured data stream,
// the appropriate error is either ErrUnexpectedEOF or some other error
// giving more detail.
public static var EOF = errors.New("EOF");

// ErrUnexpectedEOF means that EOF was encountered in the
// middle of reading a fixed-size block or data structure.
public static var ErrUnexpectedEOF = errors.New("unexpected EOF");

// ErrNoProgress is returned by some clients of an Reader when
// many calls to Read have failed to return any data or error,
// usually the sign of a broken Reader implementation.
public static var ErrNoProgress = errors.New("multiple Read calls return no data or error");

// Reader is the interface that wraps the basic Read method.
//
// Read reads up to len(p) bytes into p. It returns the number of bytes
// read (0 <= n <= len(p)) and any error encountered. Even if Read
// returns n < len(p), it may use all of p as scratch space during the call.
// If some data is available but not len(p) bytes, Read conventionally
// returns what is available instead of waiting for more.
//
// When Read encounters an error or end-of-file condition after
// successfully reading n > 0 bytes, it returns the number of
// bytes read. It may return the (non-nil) error from the same call
// or return the error (and n == 0) from a subsequent call.
// An instance of this general case is that a Reader returning
// a non-zero number of bytes at the end of the input stream may
// return either err == EOF or err == nil. The next Read should
// return 0, EOF.
//
// Callers should always process the n > 0 bytes returned before
// considering the error err. Doing so correctly handles I/O errors
// that happen after reading some bytes and also both of the
// allowed EOF behaviors.
//
// Implementations of Read are discouraged from returning a
// zero byte count with a nil error, except when len(p) == 0.
// Callers should treat a return of 0 and nil as indicating that
// nothing happened; in particular it does not indicate EOF.
//
// Implementations must not retain p.
public partial interface Reader {
    (nint, error) Read(slice<byte> p);
}

// Writer is the interface that wraps the basic Write method.
//
// Write writes len(p) bytes from p to the underlying data stream.
// It returns the number of bytes written from p (0 <= n <= len(p))
// and any error encountered that caused the write to stop early.
// Write must return a non-nil error if it returns n < len(p).
// Write must not modify the slice data, even temporarily.
//
// Implementations must not retain p.
public partial interface Writer {
    (nint, error) Write(slice<byte> p);
}

// Closer is the interface that wraps the basic Close method.
//
// The behavior of Close after the first call is undefined.
// Specific implementations may document their own behavior.
public partial interface Closer {
    error Close();
}

// Seeker is the interface that wraps the basic Seek method.
//
// Seek sets the offset for the next Read or Write to offset,
// interpreted according to whence:
// SeekStart means relative to the start of the file,
// SeekCurrent means relative to the current offset, and
// SeekEnd means relative to the end.
// Seek returns the new offset relative to the start of the
// file and an error, if any.
//
// Seeking to an offset before the start of the file is an error.
// Seeking to any positive offset is legal, but the behavior of subsequent
// I/O operations on the underlying object is implementation-dependent.
public partial interface Seeker {
    (long, error) Seek(long offset, nint whence);
}

// ReadWriter is the interface that groups the basic Read and Write methods.
public partial interface ReadWriter {
}

// ReadCloser is the interface that groups the basic Read and Close methods.
public partial interface ReadCloser {
}

// WriteCloser is the interface that groups the basic Write and Close methods.
public partial interface WriteCloser {
}

// ReadWriteCloser is the interface that groups the basic Read, Write and Close methods.
public partial interface ReadWriteCloser {
}

// ReadSeeker is the interface that groups the basic Read and Seek methods.
public partial interface ReadSeeker {
}

// ReadSeekCloser is the interface that groups the basic Read, Seek and Close
// methods.
public partial interface ReadSeekCloser {
}

// WriteSeeker is the interface that groups the basic Write and Seek methods.
public partial interface WriteSeeker {
}

// ReadWriteSeeker is the interface that groups the basic Read, Write and Seek methods.
public partial interface ReadWriteSeeker {
}

// ReaderFrom is the interface that wraps the ReadFrom method.
//
// ReadFrom reads data from r until EOF or error.
// The return value n is the number of bytes read.
// Any error except EOF encountered during the read is also returned.
//
// The Copy function uses ReaderFrom if available.
public partial interface ReaderFrom {
    (long, error) ReadFrom(Reader r);
}

// WriterTo is the interface that wraps the WriteTo method.
//
// WriteTo writes data to w until there's no more data to write or
// when an error occurs. The return value n is the number of bytes
// written. Any error encountered during the write is also returned.
//
// The Copy function uses WriterTo if available.
public partial interface WriterTo {
    (long, error) WriteTo(Writer w);
}

// ReaderAt is the interface that wraps the basic ReadAt method.
//
// ReadAt reads len(p) bytes into p starting at offset off in the
// underlying input source. It returns the number of bytes
// read (0 <= n <= len(p)) and any error encountered.
//
// When ReadAt returns n < len(p), it returns a non-nil error
// explaining why more bytes were not returned. In this respect,
// ReadAt is stricter than Read.
//
// Even if ReadAt returns n < len(p), it may use all of p as scratch
// space during the call. If some data is available but not len(p) bytes,
// ReadAt blocks until either all the data is available or an error occurs.
// In this respect ReadAt is different from Read.
//
// If the n = len(p) bytes returned by ReadAt are at the end of the
// input source, ReadAt may return either err == EOF or err == nil.
//
// If ReadAt is reading from an input source with a seek offset,
// ReadAt should not affect nor be affected by the underlying
// seek offset.
//
// Clients of ReadAt can execute parallel ReadAt calls on the
// same input source.
//
// Implementations must not retain p.
public partial interface ReaderAt {
    (nint, error) ReadAt(slice<byte> p, long off);
}

// WriterAt is the interface that wraps the basic WriteAt method.
//
// WriteAt writes len(p) bytes from p to the underlying data stream
// at offset off. It returns the number of bytes written from p (0 <= n <= len(p))
// and any error encountered that caused the write to stop early.
// WriteAt must return a non-nil error if it returns n < len(p).
//
// If WriteAt is writing to a destination with a seek offset,
// WriteAt should not affect nor be affected by the underlying
// seek offset.
//
// Clients of WriteAt can execute parallel WriteAt calls on the same
// destination if the ranges do not overlap.
//
// Implementations must not retain p.
public partial interface WriterAt {
    (nint, error) WriteAt(slice<byte> p, long off);
}

// ByteReader is the interface that wraps the ReadByte method.
//
// ReadByte reads and returns the next byte from the input or
// any error encountered. If ReadByte returns an error, no input
// byte was consumed, and the returned byte value is undefined.
//
// ReadByte provides an efficient interface for byte-at-time
// processing. A Reader that does not implement  ByteReader
// can be wrapped using bufio.NewReader to add this method.
public partial interface ByteReader {
    (byte, error) ReadByte();
}

// ByteScanner is the interface that adds the UnreadByte method to the
// basic ReadByte method.
//
// UnreadByte causes the next call to ReadByte to return the same byte
// as the previous call to ReadByte.
// It may be an error to call UnreadByte twice without an intervening
// call to ReadByte.
public partial interface ByteScanner {
    error UnreadByte();
}

// ByteWriter is the interface that wraps the WriteByte method.
public partial interface ByteWriter {
    error WriteByte(byte c);
}

// RuneReader is the interface that wraps the ReadRune method.
//
// ReadRune reads a single UTF-8 encoded Unicode character
// and returns the rune and its size in bytes. If no character is
// available, err will be set.
public partial interface RuneReader {
    (int, nint, error) ReadRune();
}

// RuneScanner is the interface that adds the UnreadRune method to the
// basic ReadRune method.
//
// UnreadRune causes the next call to ReadRune to return the same rune
// as the previous call to ReadRune.
// It may be an error to call UnreadRune twice without an intervening
// call to ReadRune.
public partial interface RuneScanner {
    error UnreadRune();
}

// StringWriter is the interface that wraps the WriteString method.
public partial interface StringWriter {
    (nint, error) WriteString(@string s);
}

// WriteString writes the contents of the string s to w, which accepts a slice of bytes.
// If w implements StringWriter, its WriteString method is invoked directly.
// Otherwise, w.Write is called exactly once.
public static (nint, error) WriteString(Writer w, @string s) {
    nint n = default;
    error err = default!;

    {
        StringWriter (sw, ok) = StringWriter.As(w._<StringWriter>())!;

        if (ok) {
            return sw.WriteString(s);
        }
    }
    return w.Write((slice<byte>)s);
}

// ReadAtLeast reads from r into buf until it has read at least min bytes.
// It returns the number of bytes copied and an error if fewer bytes were read.
// The error is EOF only if no bytes were read.
// If an EOF happens after reading fewer than min bytes,
// ReadAtLeast returns ErrUnexpectedEOF.
// If min is greater than the length of buf, ReadAtLeast returns ErrShortBuffer.
// On return, n >= min if and only if err == nil.
// If r returns an error having read at least min bytes, the error is dropped.
public static (nint, error) ReadAtLeast(Reader r, slice<byte> buf, nint min) {
    nint n = default;
    error err = default!;

    if (len(buf) < min) {
        return (0, error.As(ErrShortBuffer)!);
    }
    while (n < min && err == null) {
        nint nn = default;
        nn, err = r.Read(buf[(int)n..]);
        n += nn;
    }
    if (n >= min) {
        err = null;
    }
    else if (n > 0 && err == EOF) {
        err = ErrUnexpectedEOF;
    }
    return ;
}

// ReadFull reads exactly len(buf) bytes from r into buf.
// It returns the number of bytes copied and an error if fewer bytes were read.
// The error is EOF only if no bytes were read.
// If an EOF happens after reading some but not all the bytes,
// ReadFull returns ErrUnexpectedEOF.
// On return, n == len(buf) if and only if err == nil.
// If r returns an error having read at least len(buf) bytes, the error is dropped.
public static (nint, error) ReadFull(Reader r, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return ReadAtLeast(r, buf, len(buf));
}

// CopyN copies n bytes (or until an error) from src to dst.
// It returns the number of bytes copied and the earliest
// error encountered while copying.
// On return, written == n if and only if err == nil.
//
// If dst implements the ReaderFrom interface,
// the copy is implemented using it.
public static (long, error) CopyN(Writer dst, Reader src, long n) {
    long written = default;
    error err = default!;

    written, err = Copy(dst, LimitReader(src, n));
    if (written == n) {
        return (n, error.As(null!)!);
    }
    if (written < n && err == null) { 
        // src stopped early; must have been EOF.
        err = EOF;
    }
    return ;
}

// Copy copies from src to dst until either EOF is reached
// on src or an error occurs. It returns the number of bytes
// copied and the first error encountered while copying, if any.
//
// A successful Copy returns err == nil, not err == EOF.
// Because Copy is defined to read from src until EOF, it does
// not treat an EOF from Read as an error to be reported.
//
// If src implements the WriterTo interface,
// the copy is implemented by calling src.WriteTo(dst).
// Otherwise, if dst implements the ReaderFrom interface,
// the copy is implemented by calling dst.ReadFrom(src).
public static (long, error) Copy(Writer dst, Reader src) {
    long written = default;
    error err = default!;

    return copyBuffer(dst, src, null);
}

// CopyBuffer is identical to Copy except that it stages through the
// provided buffer (if one is required) rather than allocating a
// temporary one. If buf is nil, one is allocated; otherwise if it has
// zero length, CopyBuffer panics.
//
// If either src implements WriterTo or dst implements ReaderFrom,
// buf will not be used to perform the copy.
public static (long, error) CopyBuffer(Writer dst, Reader src, slice<byte> buf) => func((_, panic, _) => {
    long written = default;
    error err = default!;

    if (buf != null && len(buf) == 0) {
        panic("empty buffer in CopyBuffer");
    }
    return copyBuffer(dst, src, buf);
});

// copyBuffer is the actual implementation of Copy and CopyBuffer.
// if buf is nil, one is allocated.
private static (long, error) copyBuffer(Writer dst, Reader src, slice<byte> buf) {
    long written = default;
    error err = default!;
 
    // If the reader has a WriteTo method, use it to do the copy.
    // Avoids an allocation and a copy.
    {
        WriterTo (wt, ok) = WriterTo.As(src._<WriterTo>())!;

        if (ok) {
            return wt.WriteTo(dst);
        }
    } 
    // Similarly, if the writer has a ReadFrom method, use it to do the copy.
    {
        ReaderFrom (rt, ok) = ReaderFrom.As(dst._<ReaderFrom>())!;

        if (ok) {
            return rt.ReadFrom(src);
        }
    }
    if (buf == null) {
        nint size = 32 * 1024;
        {
            ptr<LimitedReader> (l, ok) = src._<ptr<LimitedReader>>();

            if (ok && int64(size) > l.N) {
                if (l.N < 1) {
                    size = 1;
                }
                else
 {
                    size = int(l.N);
                }
            }

        }
        buf = make_slice<byte>(size);
    }
    while (true) {
        var (nr, er) = src.Read(buf);
        if (nr > 0) {
            var (nw, ew) = dst.Write(buf[(int)0..(int)nr]);
            if (nw < 0 || nr < nw) {
                nw = 0;
                if (ew == null) {
                    ew = errInvalidWrite;
                }
            }
            written += int64(nw);
            if (ew != null) {
                err = ew;
                break;
            }
            if (nr != nw) {
                err = ErrShortWrite;
                break;
            }
        }
        if (er != null) {
            if (er != EOF) {
                err = er;
            }
            break;
        }
    }
    return (written, error.As(err)!);
}

// LimitReader returns a Reader that reads from r
// but stops with EOF after n bytes.
// The underlying implementation is a *LimitedReader.
public static Reader LimitReader(Reader r, long n) {
    return addr(new LimitedReader(r,n));
}

// A LimitedReader reads from R but limits the amount of
// data returned to just N bytes. Each call to Read
// updates N to reflect the new amount remaining.
// Read returns EOF when N <= 0 or when the underlying R returns EOF.
public partial struct LimitedReader {
    public Reader R; // underlying reader
    public long N; // max bytes remaining
}

private static (nint, error) Read(this ptr<LimitedReader> _addr_l, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref LimitedReader l = ref _addr_l.val;

    if (l.N <= 0) {
        return (0, error.As(EOF)!);
    }
    if (int64(len(p)) > l.N) {
        p = p[(int)0..(int)l.N];
    }
    n, err = l.R.Read(p);
    l.N -= int64(n);
    return ;
}

// NewSectionReader returns a SectionReader that reads from r
// starting at offset off and stops with EOF after n bytes.
public static ptr<SectionReader> NewSectionReader(ReaderAt r, long off, long n) {
    return addr(new SectionReader(r,off,off,off+n));
}

// SectionReader implements Read, Seek, and ReadAt on a section
// of an underlying ReaderAt.
public partial struct SectionReader {
    public ReaderAt r;
    public long @base;
    public long off;
    public long limit;
}

private static (nint, error) Read(this ptr<SectionReader> _addr_s, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref SectionReader s = ref _addr_s.val;

    if (s.off >= s.limit) {
        return (0, error.As(EOF)!);
    }
    {
        var max = s.limit - s.off;

        if (int64(len(p)) > max) {
            p = p[(int)0..(int)max];
        }
    }
    n, err = s.r.ReadAt(p, s.off);
    s.off += int64(n);
    return ;
}

private static var errWhence = errors.New("Seek: invalid whence");
private static var errOffset = errors.New("Seek: invalid offset");

private static (long, error) Seek(this ptr<SectionReader> _addr_s, long offset, nint whence) {
    long _p0 = default;
    error _p0 = default!;
    ref SectionReader s = ref _addr_s.val;


    if (whence == SeekStart) 
        offset += s.@base;
    else if (whence == SeekCurrent) 
        offset += s.off;
    else if (whence == SeekEnd) 
        offset += s.limit;
    else 
        return (0, error.As(errWhence)!);
        if (offset < s.@base) {
        return (0, error.As(errOffset)!);
    }
    s.off = offset;
    return (offset - s.@base, error.As(null!)!);
}

private static (nint, error) ReadAt(this ptr<SectionReader> _addr_s, slice<byte> p, long off) {
    nint n = default;
    error err = default!;
    ref SectionReader s = ref _addr_s.val;

    if (off < 0 || off >= s.limit - s.@base) {
        return (0, error.As(EOF)!);
    }
    off += s.@base;
    {
        var max = s.limit - off;

        if (int64(len(p)) > max) {
            p = p[(int)0..(int)max];
            n, err = s.r.ReadAt(p, off);
            if (err == null) {
                err = EOF;
            }
            return (n, error.As(err)!);
        }
    }
    return s.r.ReadAt(p, off);
}

// Size returns the size of the section in bytes.
private static long Size(this ptr<SectionReader> _addr_s) {
    ref SectionReader s = ref _addr_s.val;

    return s.limit - s.@base;
}

// TeeReader returns a Reader that writes to w what it reads from r.
// All reads from r performed through it are matched with
// corresponding writes to w. There is no internal buffering -
// the write must complete before the read completes.
// Any error encountered while writing is reported as a read error.
public static Reader TeeReader(Reader r, Writer w) {
    return addr(new teeReader(r,w));
}

private partial struct teeReader {
    public Reader r;
    public Writer w;
}

private static (nint, error) Read(this ptr<teeReader> _addr_t, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref teeReader t = ref _addr_t.val;

    n, err = t.r.Read(p);
    if (n > 0) {
        {
            var (n, err) = t.w.Write(p[..(int)n]);

            if (err != null) {
                return (n, error.As(err)!);
            }

        }
    }
    return ;
}

// Discard is a Writer on which all Write calls succeed
// without doing anything.
public static Writer Discard = Writer.As(new discard())!;

private partial struct discard {
}

// discard implements ReaderFrom as an optimization so Copy to
// io.Discard can avoid doing unnecessary work.
private static ReaderFrom _ = ReaderFrom.As(new discard())!;

private static (nint, error) Write(this discard _p0, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;

    return (len(p), error.As(null!)!);
}

private static (nint, error) WriteString(this discard _p0, @string s) {
    nint _p0 = default;
    error _p0 = default!;

    return (len(s), error.As(null!)!);
}

private static sync.Pool blackHolePool = new sync.Pool(New:func()interface{}{b:=make([]byte,8192)return&b},);

private static (long, error) ReadFrom(this discard _p0, Reader r) {
    long n = default;
    error err = default!;

    ptr<slice<byte>> bufp = blackHolePool.Get()._<ptr<slice<byte>>>();
    nint readSize = 0;
    while (true) {
        readSize, err = r.Read(bufp.val);
        n += int64(readSize);
        if (err != null) {
            blackHolePool.Put(bufp);
            if (err == EOF) {
                return (n, error.As(null!)!);
            }
            return ;
        }
    }
}

// NopCloser returns a ReadCloser with a no-op Close method wrapping
// the provided Reader r.
public static ReadCloser NopCloser(Reader r) {
    return new nopCloser(r);
}

private partial struct nopCloser : Reader {
    public Reader Reader;
}

private static error Close(this nopCloser _p0) {
    return error.As(null!)!;
}

// ReadAll reads from r until an error or EOF and returns the data it read.
// A successful call returns err == nil, not err == EOF. Because ReadAll is
// defined to read from src until EOF, it does not treat an EOF from Read
// as an error to be reported.
public static (slice<byte>, error) ReadAll(Reader r) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var b = make_slice<byte>(0, 512);
    while (true) {
        if (len(b) == cap(b)) { 
            // Add more capacity (let append pick how much).
            b = append(b, 0)[..(int)len(b)];
        }
        var (n, err) = r.Read(b[(int)len(b)..(int)cap(b)]);
        b = b[..(int)len(b) + n];
        if (err != null) {
            if (err == EOF) {
                err = null;
            }
            return (b, error.As(err)!);
        }
    }
}

} // end io_package
