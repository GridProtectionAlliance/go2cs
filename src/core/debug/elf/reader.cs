// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using io = io_package;
using os = os_package;

partial class elf_package {

// errorReader returns error from all operations.
[GoType] partial struct errorReader {
    internal error error;
}

internal static (nint n, error err) Read(this errorReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    return (0, r.error);
}

internal static (nint n, error err) ReadAt(this errorReader r, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    return (0, r.error);
}

internal static (int64, error) Seek(this errorReader r, int64 offset, nint whence) {
    return (0, r.error);
}

internal static error Close(this errorReader r) {
    return r.error;
}

// readSeekerFromReader converts an io.Reader into an io.ReadSeeker.
// In general Seek may not be efficient, but it is optimized for
// common cases such as seeking to the end to find the length of the
// data.
[GoType] partial struct readSeekerFromReader {
    internal Func<(io.Reader, error)> reset;
    internal io_package.Reader r;
    internal int64 size;
    internal int64 offset;
}

[GoRecv] internal static void start(this ref readSeekerFromReader r) {
    (x, err) = r.reset();
    if (err != default!){
        r.r = new errorReader(err);
    } else {
        r.r = x;
    }
    r.offset = 0;
}

[GoRecv] internal static (nint n, error err) Read(this ref readSeekerFromReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (r.r == default!) {
        r.start();
    }
    (n, err) = r.r.Read(p);
    r.offset += ((int64)n);
    return (n, err);
}

[GoRecv] internal static (int64, error) Seek(this ref readSeekerFromReader r, int64 offset, nint whence) {
    int64 newOffset = default!;
    switch (whence) {
    case io.SeekStart: {
        newOffset = offset;
        break;
    }
    case io.SeekCurrent: {
        newOffset = r.offset + offset;
        break;
    }
    case io.SeekEnd: {
        newOffset = r.size + offset;
        break;
    }
    default: {
        return (0, os.ErrInvalid);
    }}

    switch (ᐧ) {
    case {} when newOffset is r.offset: {
        return (newOffset, default!);
    }
    case {} when newOffset is < 0 or > r.size: {
        return (0, os.ErrInvalid);
    }
    case {} when newOffset is 0: {
        r.r = default!;
        break;
    }
    case {} when newOffset is r.size: {
        r.r = new errorReader(io.EOF);
        break;
    }
    default: {
        if (newOffset < r.offset) {
            // Restart at the beginning.
            r.start();
        }
        // Read until we reach offset.
        array<byte> buf = new(512);
        while (r.offset < newOffset) {
            var b = buf[..];
            if (newOffset - r.offset < ((int64)len(buf))) {
                b = buf[..(int)(newOffset - r.offset)];
            }
            {
                var (_, err) = r.Read(b); if (err != default!) {
                    return (0, err);
                }
            }
        }
        break;
    }}

    r.offset = newOffset;
    return (r.offset, default!);
}

} // end elf_package
