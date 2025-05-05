// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using io = io_package;

partial class slicewriter_package {

// WriteSeeker is a helper object that implements the io.WriteSeeker
// interface. Clients can create a WriteSeeker, make a series of Write
// calls to add data to it (and possibly Seek calls to update
// previously written portions), then finally invoke BytesWritten() to
// get a pointer to the constructed byte slice.
[GoType] partial struct WriteSeeker {
    internal slice<byte> payload;
    internal int64 off;
}

[GoRecv] public static (nint n, error err) Write(this ref WriteSeeker sws, slice<byte> p) {
    nint n = default!;
    error err = default!;

    nint amt = len(p);
    var towrite = sws.payload[(int)(sws.off)..];
    if (len(towrite) < amt) {
        sws.payload = append(sws.payload, new slice<byte>(amt - len(towrite)).ꓸꓸꓸ);
        towrite = sws.payload[(int)(sws.off)..];
    }
    copy(towrite, p);
    sws.off += ((int64)amt);
    return (amt, default!);
}

// Seek repositions the read/write position of the WriteSeeker within
// its internally maintained slice. Note that it is not possible to
// expand the size of the slice using SEEK_SET; trying to seek outside
// the slice will result in an error.
[GoRecv] public static (int64, error) Seek(this ref WriteSeeker sws, int64 offset, nint whence) {
    switch (whence) {
    case io.SeekStart: {
        if (sws.off != offset && (offset < 0 || offset > ((int64)len(sws.payload)))) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, offset, len(sws.payload)));
        }
        sws.off = offset;
        return (offset, default!);
    }
    case io.SeekCurrent: {
        var newoff = sws.off + offset;
        if (newoff != sws.off && (newoff < 0 || newoff > ((int64)len(sws.payload)))) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, newoff, len(sws.payload)));
        }
        sws.off += offset;
        return (sws.off, default!);
    }
    case io.SeekEnd: {
        var newoff = ((int64)len(sws.payload)) + offset;
        if (newoff != sws.off && (newoff < 0 || newoff > ((int64)len(sws.payload)))) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, newoff, len(sws.payload)));
        }
        sws.off = newoff;
        return (sws.off, default!);
    }}

    // other modes not supported
    return (0, fmt.Errorf("unsupported seek mode %d"u8, whence));
}

// BytesWritten returns the underlying byte slice for the WriteSeeker,
// containing the data written to it via Write/Seek calls.
[GoRecv] public static slice<byte> BytesWritten(this ref WriteSeeker sws) {
    return sws.payload;
}

[GoRecv] public static (nint n, error err) Read(this ref WriteSeeker sws, slice<byte> p) {
    nint n = default!;
    error err = default!;

    nint amt = len(p);
    var toread = sws.payload[(int)(sws.off)..];
    if (len(toread) < amt) {
        amt = len(toread);
    }
    copy(p, toread);
    sws.off += ((int64)amt);
    return (amt, default!);
}

} // end slicewriter_package
