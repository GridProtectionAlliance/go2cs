// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package iotest -- go2cs converted at 2022 March 06 23:19:29 UTC
// import "testing/iotest" ==> using iotest = go.testing.iotest_package
// Original source: C:\Program Files\Go\src\testing\iotest\logger.go
using io = go.io_package;
using log = go.log_package;

namespace go.testing;

public static partial class iotest_package {

private partial struct writeLogger {
    public @string prefix;
    public io.Writer w;
}

private static (nint, error) Write(this ptr<writeLogger> _addr_l, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref writeLogger l = ref _addr_l.val;

    n, err = l.w.Write(p);
    if (err != null) {
        log.Printf("%s %x: %v", l.prefix, p[(int)0..(int)n], err);
    }
    else
 {
        log.Printf("%s %x", l.prefix, p[(int)0..(int)n]);
    }
    return ;

}

// NewWriteLogger returns a writer that behaves like w except
// that it logs (using log.Printf) each write to standard error,
// printing the prefix and the hexadecimal data written.
public static io.Writer NewWriteLogger(@string prefix, io.Writer w) {
    return addr(new writeLogger(prefix,w));
}

private partial struct readLogger {
    public @string prefix;
    public io.Reader r;
}

private static (nint, error) Read(this ptr<readLogger> _addr_l, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref readLogger l = ref _addr_l.val;

    n, err = l.r.Read(p);
    if (err != null) {
        log.Printf("%s %x: %v", l.prefix, p[(int)0..(int)n], err);
    }
    else
 {
        log.Printf("%s %x", l.prefix, p[(int)0..(int)n]);
    }
    return ;

}

// NewReadLogger returns a reader that behaves like r except
// that it logs (using log.Printf) each read to standard error,
// printing the prefix and the hexadecimal data read.
public static io.Reader NewReadLogger(@string prefix, io.Reader r) {
    return addr(new readLogger(prefix,r));
}

} // end iotest_package
