// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bio -- go2cs converted at 2022 March 13 05:43:19 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Program Files\Go\src\cmd\internal\bio\must.go
namespace go.cmd.@internal;

using io = io_package;
using log = log_package;


// MustClose closes Closer c and calls log.Fatal if it returns a non-nil error.

public static partial class bio_package {

public static void MustClose(io.Closer c) {
    {
        var err = c.Close();

        if (err != null) {
            log.Fatal(err);
        }
    }
}

// MustWriter returns a Writer that wraps the provided Writer,
// except that it calls log.Fatal instead of returning a non-nil error.
public static io.Writer MustWriter(io.Writer w) {
    return new mustWriter(w);
}

private partial struct mustWriter {
    public io.Writer w;
}

private static (nint, error) Write(this mustWriter w, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;

    var (n, err) = w.w.Write(b);
    if (err != null) {
        log.Fatal(err);
    }
    return (n, error.As(null!)!);
}

private static (nint, error) WriteString(this mustWriter w, @string s) {
    nint _p0 = default;
    error _p0 = default!;

    var (n, err) = io.WriteString(w.w, s);
    if (err != null) {
        log.Fatal(err);
    }
    return (n, error.As(null!)!);
}

} // end bio_package
