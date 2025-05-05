// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using io = io_package;

partial class iotest_package {

// TruncateWriter returns a Writer that writes to w
// but stops silently after n bytes.
public static io.Writer TruncateWriter(io.Writer w, int64 n) {
    return new truncateWriter(w, n);
}

[GoType] partial struct truncateWriter {
    internal io_package.Writer w;
    internal int64 n;
}

[GoRecv] internal static (nint n, error err) Write(this ref truncateWriter t, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (t.n <= 0) {
        return (len(p), default!);
    }
    // real write
    n = len(p);
    if (((int64)n) > t.n) {
        n = ((nint)t.n);
    }
    (n, err) = t.w.Write(p[0..(int)(n)]);
    t.n -= ((int64)n);
    if (err == default!) {
        n = len(p);
    }
    return (n, err);
}

} // end iotest_package
