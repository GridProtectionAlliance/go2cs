// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bio implements common I/O abstractions used within the Go toolchain.
// package bio -- go2cs converted at 2022 March 06 22:32:21 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Program Files\Go\src\cmd\internal\bio\buf.go
using bufio = go.bufio_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;

namespace go.cmd.@internal;

public static partial class bio_package {

    // Reader implements a seekable buffered io.Reader.
public partial struct Reader {
    public ptr<os.File> f;
    public ref ptr<bufio.Reader> Reader> => ref Reader>_ptr;
}

// Writer implements a seekable buffered io.Writer.
public partial struct Writer {
    public ptr<os.File> f;
    public ref ptr<bufio.Writer> Writer> => ref Writer>_ptr;
}

// Create creates the file named name and returns a Writer
// for that file.
public static (ptr<Writer>, error) Create(@string name) {
    ptr<Writer> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Create(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new Writer(f:f,Writer:bufio.NewWriter(f))), error.As(null!)!);

}

// Open returns a Reader for the file named name.
public static (ptr<Reader>, error) Open(@string name) {
    ptr<Reader> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_NewReader(_addr_f)!, error.As(null!)!);

}

// NewReader returns a Reader from an open file.
public static ptr<Reader> NewReader(ptr<os.File> _addr_f) {
    ref os.File f = ref _addr_f.val;

    return addr(new Reader(f:f,Reader:bufio.NewReader(f)));
}

private static long MustSeek(this ptr<Reader> _addr_r, long offset, nint whence) {
    ref Reader r = ref _addr_r.val;

    if (whence == 1) {
        offset -= int64(r.Buffered());
    }
    var (off, err) = r.f.Seek(offset, whence);
    if (err != null) {
        log.Fatalf("seeking in output: %v", err);
    }
    r.Reset(r.f);
    return off;

}

private static long MustSeek(this ptr<Writer> _addr_w, long offset, nint whence) {
    ref Writer w = ref _addr_w.val;

    {
        var err = w.Flush();

        if (err != null) {
            log.Fatalf("writing output: %v", err);
        }
    }

    var (off, err) = w.f.Seek(offset, whence);
    if (err != null) {
        log.Fatalf("seeking in output: %v", err);
    }
    return off;

}

private static long Offset(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    var (off, err) = r.f.Seek(0, 1);
    if (err != null) {
        log.Fatalf("seeking in output [0, 1]: %v", err);
    }
    off -= int64(r.Buffered());
    return off;

}

private static long Offset(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    {
        var err = w.Flush();

        if (err != null) {
            log.Fatalf("writing output: %v", err);
        }
    }

    var (off, err) = w.f.Seek(0, 1);
    if (err != null) {
        log.Fatalf("seeking in output [0, 1]: %v", err);
    }
    return off;

}

private static error Close(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return error.As(r.f.Close())!;
}

private static error Close(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    var err = w.Flush();
    var err1 = w.f.Close();
    if (err == null) {
        err = err1;
    }
    return error.As(err)!;

}

private static ptr<os.File> File(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    return _addr_r.f!;
}

private static ptr<os.File> File(this ptr<Writer> _addr_w) {
    ref Writer w = ref _addr_w.val;

    return _addr_w.f!;
}

// Slice reads the next length bytes of r into a slice.
//
// This slice may be backed by mmap'ed memory. Currently, this memory
// will never be unmapped. The second result reports whether the
// backing memory is read-only.
private static (slice<byte>, bool, error) Slice(this ptr<Reader> _addr_r, ulong length) {
    slice<byte> _p0 = default;
    bool _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    if (length == 0) {
        return (new slice<byte>(new byte[] {  }), false, error.As(null!)!);
    }
    var (data, ok) = r.sliceOS(length);
    if (ok) {
        return (data, true, error.As(null!)!);
    }
    data = make_slice<byte>(length);
    var (_, err) = io.ReadFull(r, data);
    if (err != null) {
        return (null, false, error.As(err)!);
    }
    return (data, false, error.As(null!)!);

}

// SliceRO returns a slice containing the next length bytes of r
// backed by a read-only mmap'd data. If the mmap cannot be
// established (limit exceeded, region too small, etc) a nil slice
// will be returned. If mmap succeeds, it will never be unmapped.
private static slice<byte> SliceRO(this ptr<Reader> _addr_r, ulong length) {
    ref Reader r = ref _addr_r.val;

    var (data, ok) = r.sliceOS(length);
    if (ok) {
        return data;
    }
    return null;

}

} // end bio_package
