// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package iotest implements Readers and Writers useful mainly for testing.
// package iotest -- go2cs converted at 2022 March 06 23:19:30 UTC
// import "testing/iotest" ==> using iotest = go.testing.iotest_package
// Original source: C:\Program Files\Go\src\testing\iotest\reader.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;

namespace go.testing;

public static partial class iotest_package {

    // OneByteReader returns a Reader that implements
    // each non-empty Read by reading one byte from r.
public static io.Reader OneByteReader(io.Reader r) {
    return addr(new oneByteReader(r));
}

private partial struct oneByteReader {
    public io.Reader r;
}

private static (nint, error) Read(this ptr<oneByteReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref oneByteReader r = ref _addr_r.val;

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    return r.r.Read(p[(int)0..(int)1]);

}

// HalfReader returns a Reader that implements Read
// by reading half as many requested bytes from r.
public static io.Reader HalfReader(io.Reader r) {
    return addr(new halfReader(r));
}

private partial struct halfReader {
    public io.Reader r;
}

private static (nint, error) Read(this ptr<halfReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref halfReader r = ref _addr_r.val;

    return r.r.Read(p[(int)0..(int)(len(p) + 1) / 2]);
}

// DataErrReader changes the way errors are handled by a Reader. Normally, a
// Reader returns an error (typically EOF) from the first Read call after the
// last piece of data is read. DataErrReader wraps a Reader and changes its
// behavior so the final error is returned along with the final data, instead
// of in the first call after the final data.
public static io.Reader DataErrReader(io.Reader r) {
    return addr(new dataErrReader(r,nil,make([]byte,1024)));
}

private partial struct dataErrReader {
    public io.Reader r;
    public slice<byte> unread;
    public slice<byte> data;
}

private static (nint, error) Read(this ptr<dataErrReader> _addr_r, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref dataErrReader r = ref _addr_r.val;
 
    // loop because first call needs two reads:
    // one to get data and a second to look for an error.
    while (true) {
        if (len(r.unread) == 0) {
            var (n1, err1) = r.r.Read(r.data);
            r.unread = r.data[(int)0..(int)n1];
            err = err1;
        }
        if (n > 0 || err != null) {
            break;
        }
        n = copy(p, r.unread);
        r.unread = r.unread[(int)n..];

    }
    return ;

}

// ErrTimeout is a fake timeout error.
public static var ErrTimeout = errors.New("timeout");

// TimeoutReader returns ErrTimeout on the second read
// with no data. Subsequent calls to read succeed.
public static io.Reader TimeoutReader(io.Reader r) {
    return addr(new timeoutReader(r,0));
}

private partial struct timeoutReader {
    public io.Reader r;
    public nint count;
}

private static (nint, error) Read(this ptr<timeoutReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref timeoutReader r = ref _addr_r.val;

    r.count++;
    if (r.count == 2) {
        return (0, error.As(ErrTimeout)!);
    }
    return r.r.Read(p);

}

// ErrReader returns an io.Reader that returns 0, err from all Read calls.
public static io.Reader ErrReader(error err) {
    return addr(new errReader(err:err));
}

private partial struct errReader {
    public error err;
}

private static (nint, error) Read(this ptr<errReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref errReader r = ref _addr_r.val;

    return (0, error.As(r.err)!);
}

private partial struct smallByteReader {
    public io.Reader r;
    public nint off;
    public nint n;
}

private static (nint, error) Read(this ptr<smallByteReader> _addr_r, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref smallByteReader r = ref _addr_r.val;

    if (len(p) == 0) {
        return (0, error.As(null!)!);
    }
    r.n = r.n % 3 + 1;
    var n = r.n;
    if (n > len(p)) {
        n = len(p);
    }
    var (n, err) = r.r.Read(p[(int)0..(int)n]);
    if (err != null && err != io.EOF) {
        err = fmt.Errorf("Read(%d bytes at offset %d): %v", n, r.off, err);
    }
    r.off += n;
    return (n, error.As(err)!);

}

// TestReader tests that reading from r returns the expected file content.
// It does reads of different sizes, until EOF.
// If r implements io.ReaderAt or io.Seeker, TestReader also checks
// that those operations behave as they should.
//
// If TestReader finds any misbehaviors, it returns an error reporting them.
// The error text may span multiple lines.
public static error TestReader(io.Reader r, slice<byte> content) {
    if (len(content) > 0) {
        var (n, err) = r.Read(null);
        if (n != 0 || err != null) {
            return error.As(fmt.Errorf("Read(0) = %d, %v, want 0, nil", n, err))!;
        }
    }
    var (data, err) = io.ReadAll(addr(new smallByteReader(r:r)));
    if (err != null) {
        return error.As(err)!;
    }
    if (!bytes.Equal(data, content)) {
        return error.As(fmt.Errorf("ReadAll(small amounts) = %q\n\twant %q", data, content))!;
    }
    (n, err) = r.Read(make_slice<byte>(10));
    if (n != 0 || err != io.EOF) {
        return error.As(fmt.Errorf("Read(10) at EOF = %v, %v, want 0, EOF", n, err))!;
    }
    {
        io.ReadSeeker r__prev1 = r;

        io.ReadSeeker (r, ok) = r._<io.ReadSeeker>();

        if (ok) { 
            // Seek(0, 1) should report the current file position (EOF).
            {
                var off__prev2 = off;

                var (off, err) = r.Seek(0, 1);

                if (off != int64(len(content)) || err != null) {
                    return error.As(fmt.Errorf("Seek(0, 1) from EOF = %d, %v, want %d, nil", off, err, len(content)))!;
                } 

                // Seek backward partway through file, in two steps.
                // If middle == 0, len(content) == 0, can't use the -1 and +1 seeks.

                off = off__prev2;

            } 

            // Seek backward partway through file, in two steps.
            // If middle == 0, len(content) == 0, can't use the -1 and +1 seeks.
            var middle = len(content) - len(content) / 3;
            if (middle > 0) {
                {
                    var off__prev3 = off;

                    (off, err) = r.Seek(-1, 1);

                    if (off != int64(len(content) - 1) || err != null) {
                        return error.As(fmt.Errorf("Seek(-1, 1) from EOF = %d, %v, want %d, nil", -off, err, len(content) - 1))!;
                    }

                    off = off__prev3;

                }

                {
                    var off__prev3 = off;

                    (off, err) = r.Seek(int64(-len(content) / 3), 1);

                    if (off != int64(middle - 1) || err != null) {
                        return error.As(fmt.Errorf("Seek(%d, 1) from %d = %d, %v, want %d, nil", -len(content) / 3, len(content) - 1, off, err, middle - 1))!;
                    }

                    off = off__prev3;

                }

                {
                    var off__prev3 = off;

                    (off, err) = r.Seek(+1, 1);

                    if (off != int64(middle) || err != null) {
                        return error.As(fmt.Errorf("Seek(+1, 1) from %d = %d, %v, want %d, nil", middle - 1, off, err, middle))!;
                    }

                    off = off__prev3;

                }

            } 

            // Seek(0, 1) should report the current file position (middle).
            {
                var off__prev2 = off;

                (off, err) = r.Seek(0, 1);

                if (off != int64(middle) || err != null) {
                    return error.As(fmt.Errorf("Seek(0, 1) from %d = %d, %v, want %d, nil", middle, off, err, middle))!;
                } 

                // Reading forward should return the last part of the file.

                off = off__prev2;

            } 

            // Reading forward should return the last part of the file.
            (data, err) = io.ReadAll(addr(new smallByteReader(r:r)));
            if (err != null) {
                return error.As(fmt.Errorf("ReadAll from offset %d: %v", middle, err))!;
            }

            if (!bytes.Equal(data, content[(int)middle..])) {
                return error.As(fmt.Errorf("ReadAll from offset %d = %q\n\twant %q", middle, data, content[(int)middle..]))!;
            } 

            // Seek relative to end of file, but start elsewhere.
            {
                var off__prev2 = off;

                (off, err) = r.Seek(int64(middle / 2), 0);

                if (off != int64(middle / 2) || err != null) {
                    return error.As(fmt.Errorf("Seek(%d, 0) from EOF = %d, %v, want %d, nil", middle / 2, off, err, middle / 2))!;
                }

                off = off__prev2;

            }

            {
                var off__prev2 = off;

                (off, err) = r.Seek(int64(-len(content) / 3), 2);

                if (off != int64(middle) || err != null) {
                    return error.As(fmt.Errorf("Seek(%d, 2) from %d = %d, %v, want %d, nil", -len(content) / 3, middle / 2, off, err, middle))!;
                } 

                // Reading forward should return the last part of the file (again).

                off = off__prev2;

            } 

            // Reading forward should return the last part of the file (again).
            data, err = io.ReadAll(addr(new smallByteReader(r:r)));
            if (err != null) {
                return error.As(fmt.Errorf("ReadAll from offset %d: %v", middle, err))!;
            }

            if (!bytes.Equal(data, content[(int)middle..])) {
                return error.As(fmt.Errorf("ReadAll from offset %d = %q\n\twant %q", middle, data, content[(int)middle..]))!;
            } 

            // Absolute seek & read forward.
            {
                var off__prev2 = off;

                (off, err) = r.Seek(int64(middle / 2), 0);

                if (off != int64(middle / 2) || err != null) {
                    return error.As(fmt.Errorf("Seek(%d, 0) from EOF = %d, %v, want %d, nil", middle / 2, off, err, middle / 2))!;
                }

                off = off__prev2;

            }

            data, err = io.ReadAll(r);
            if (err != null) {
                return error.As(fmt.Errorf("ReadAll from offset %d: %v", middle / 2, err))!;
            }

            if (!bytes.Equal(data, content[(int)middle / 2..])) {
                return error.As(fmt.Errorf("ReadAll from offset %d = %q\n\twant %q", middle / 2, data, content[(int)middle / 2..]))!;
            }

        }
        r = r__prev1;

    }


    {
        io.ReadSeeker r__prev1 = r;

        (r, ok) = r._<io.ReaderAt>();

        if (ok) {
            var data = make_slice<byte>(len(content), len(content) + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in data) {
                    i = __i;
                    data[i] = 0xfe;
                }

                i = i__prev1;
            }

            (n, err) = r.ReadAt(data, 0);
            if (n != len(data) || err != null && err != io.EOF) {
                return error.As(fmt.Errorf("ReadAt(%d, 0) = %v, %v, want %d, nil or EOF", len(data), n, err, len(data)))!;
            }

            if (!bytes.Equal(data, content)) {
                return error.As(fmt.Errorf("ReadAt(%d, 0) = %q\n\twant %q", len(data), data, content))!;
            }

            n, err = r.ReadAt(data[..(int)1], int64(len(data)));
            if (n != 0 || err != io.EOF) {
                return error.As(fmt.Errorf("ReadAt(1, %d) = %v, %v, want 0, EOF", len(data), n, err))!;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in data) {
                    i = __i;
                    data[i] = 0xfe;
                }

                i = i__prev1;
            }

            n, err = r.ReadAt(data[..(int)cap(data)], 0);
            if (n != len(data) || err != io.EOF) {
                return error.As(fmt.Errorf("ReadAt(%d, 0) = %v, %v, want %d, EOF", cap(data), n, err, len(data)))!;
            }

            if (!bytes.Equal(data, content)) {
                return error.As(fmt.Errorf("ReadAt(%d, 0) = %q\n\twant %q", len(data), data, content))!;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in data) {
                    i = __i;
                    data[i] = 0xfe;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in data) {
                    i = __i;
                    n, err = r.ReadAt(data[(int)i..(int)i + 1], int64(i));
                    if (n != 1 || err != null && (i != len(data) - 1 || err != io.EOF)) {
                        @string want = "nil";
                        if (i == len(data) - 1) {
                            want = "nil or EOF";
                        }
                        return error.As(fmt.Errorf("ReadAt(1, %d) = %v, %v, want 1, %s", i, n, err, want))!;
                    }
                    if (data[i] != content[i]) {
                        return error.As(fmt.Errorf("ReadAt(1, %d) = %q want %q", i, data[(int)i..(int)i + 1], content[(int)i..(int)i + 1]))!;
                    }
                }

                i = i__prev1;
            }
        }
        r = r__prev1;

    }

    return error.As(null!)!;

}

} // end iotest_package
