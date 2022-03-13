// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2022 March 13 05:41:08 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Program Files\Go\src\strings\reader.go
namespace go;

using errors = errors_package;
using io = io_package;
using utf8 = unicode.utf8_package;


// A Reader implements the io.Reader, io.ReaderAt, io.ByteReader, io.ByteScanner,
// io.RuneReader, io.RuneScanner, io.Seeker, and io.WriterTo interfaces by reading
// from a string.
// The zero value for Reader operates like a Reader of an empty string.

public static partial class strings_package {

public partial struct Reader {
    public @string s;
    public long i; // current reading index
    public nint prevRune; // index of previous rune; or < 0
}

// Len returns the number of bytes of the unread portion of the
// string.
private static nint Len(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    if (r.i >= int64(len(r.s))) {
        return 0;
    }
    return int(int64(len(r.s)) - r.i);
}

// Size returns the original length of the underlying string.
// Size is the number of bytes available for reading via ReadAt.
// The returned value is always the same and is not affected by calls
// to any other method.
private static long Size(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return int64(len(r.s));
}

// Read implements the io.Reader interface.
private static (nint, error) Read(this ptr<Reader> _addr_r, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    if (r.i >= int64(len(r.s))) {
        return (0, error.As(io.EOF)!);
    }
    r.prevRune = -1;
    n = copy(b, r.s[(int)r.i..]);
    r.i += int64(n);
    return ;
}

// ReadAt implements the io.ReaderAt interface.
private static (nint, error) ReadAt(this ptr<Reader> _addr_r, slice<byte> b, long off) {
    nint n = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;
 
    // cannot modify state - see io.ReaderAt
    if (off < 0) {
        return (0, error.As(errors.New("strings.Reader.ReadAt: negative offset"))!);
    }
    if (off >= int64(len(r.s))) {
        return (0, error.As(io.EOF)!);
    }
    n = copy(b, r.s[(int)off..]);
    if (n < len(b)) {
        err = io.EOF;
    }
    return ;
}

// ReadByte implements the io.ByteReader interface.
private static (byte, error) ReadByte(this ptr<Reader> _addr_r) {
    byte _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    r.prevRune = -1;
    if (r.i >= int64(len(r.s))) {
        return (0, error.As(io.EOF)!);
    }
    var b = r.s[r.i];
    r.i++;
    return (b, error.As(null!)!);
}

// UnreadByte implements the io.ByteScanner interface.
private static error UnreadByte(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    if (r.i <= 0) {
        return error.As(errors.New("strings.Reader.UnreadByte: at beginning of string"))!;
    }
    r.prevRune = -1;
    r.i--;
    return error.As(null!)!;
}

// ReadRune implements the io.RuneReader interface.
private static (int, nint, error) ReadRune(this ptr<Reader> _addr_r) {
    int ch = default;
    nint size = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    if (r.i >= int64(len(r.s))) {
        r.prevRune = -1;
        return (0, 0, error.As(io.EOF)!);
    }
    r.prevRune = int(r.i);
    {
        var c = r.s[r.i];

        if (c < utf8.RuneSelf) {
            r.i++;
            return (rune(c), 1, error.As(null!)!);
        }
    }
    ch, size = utf8.DecodeRuneInString(r.s[(int)r.i..]);
    r.i += int64(size);
    return ;
}

// UnreadRune implements the io.RuneScanner interface.
private static error UnreadRune(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    if (r.i <= 0) {
        return error.As(errors.New("strings.Reader.UnreadRune: at beginning of string"))!;
    }
    if (r.prevRune < 0) {
        return error.As(errors.New("strings.Reader.UnreadRune: previous operation was not ReadRune"))!;
    }
    r.i = int64(r.prevRune);
    r.prevRune = -1;
    return error.As(null!)!;
}

// Seek implements the io.Seeker interface.
private static (long, error) Seek(this ptr<Reader> _addr_r, long offset, nint whence) {
    long _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    r.prevRune = -1;
    long abs = default;

    if (whence == io.SeekStart) 
        abs = offset;
    else if (whence == io.SeekCurrent) 
        abs = r.i + offset;
    else if (whence == io.SeekEnd) 
        abs = int64(len(r.s)) + offset;
    else 
        return (0, error.As(errors.New("strings.Reader.Seek: invalid whence"))!);
        if (abs < 0) {
        return (0, error.As(errors.New("strings.Reader.Seek: negative position"))!);
    }
    r.i = abs;
    return (abs, error.As(null!)!);
}

// WriteTo implements the io.WriterTo interface.
private static (long, error) WriteTo(this ptr<Reader> _addr_r, io.Writer w) => func((_, panic, _) => {
    long n = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    r.prevRune = -1;
    if (r.i >= int64(len(r.s))) {
        return (0, error.As(null!)!);
    }
    var s = r.s[(int)r.i..];
    var (m, err) = io.WriteString(w, s);
    if (m > len(s)) {
        panic("strings.Reader.WriteTo: invalid WriteString count");
    }
    r.i += int64(m);
    n = int64(m);
    if (m != len(s) && err == null) {
        err = io.ErrShortWrite;
    }
    return ;
});

// Reset resets the Reader to be reading from s.
private static void Reset(this ptr<Reader> _addr_r, @string s) {
    ref Reader r = ref _addr_r.val;

    r.val = new Reader(s,0,-1);
}

// NewReader returns a new Reader reading from s.
// It is similar to bytes.NewBufferString but more efficient and read-only.
public static ptr<Reader> NewReader(@string s) {
    return addr(new Reader(s,0,-1));
}

} // end strings_package
