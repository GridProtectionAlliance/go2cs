// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run generate_zipdata.go

// Package tzdata provides an embedded copy of the timezone database.
// If this package is imported anywhere in the program, then if
// the time package cannot find tzdata files on the system,
// it will use this embedded information.
//
// Importing this package will increase the size of a program by about
// 450 KB.
//
// This package should normally be imported by a program's main package,
// not by a library. Libraries normally shouldn't decide whether to
// include the timezone database in a program.
//
// This package will be automatically imported if you build with
// -tags timetzdata.
// package tzdata -- go2cs converted at 2022 March 06 22:08:06 UTC
// import "time/tzdata" ==> using tzdata = go.time.tzdata_package
// Original source: C:\Program Files\Go\src\time\tzdata\tzdata.go
// The test for this package is time/tzdata_test.go.

using errors = go.errors_package;
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using System;


namespace go.time;

public static partial class tzdata_package {

    // registerLoadFromEmbeddedTZData is defined in package time.
    //go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
private static (@string, error) registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> _p0);

private static void init() {
    registerLoadFromEmbeddedTZData(loadFromEmbeddedTZData);
}

// get4s returns the little-endian 32-bit value at the start of s.
private static nint get4s(@string s) {
    if (len(s) < 4) {>>MARKER:FUNCTION_registerLoadFromEmbeddedTZData_BLOCK_PREFIX<<
        return 0;
    }
    return int(s[0]) | int(s[1]) << 8 | int(s[2]) << 16 | int(s[3]) << 24;

}

// get2s returns the little-endian 16-bit value at the start of s.
private static nint get2s(@string s) {
    if (len(s) < 2) {
        return 0;
    }
    return int(s[0]) | int(s[1]) << 8;

}

// loadFromEmbeddedTZData returns the contents of the file with the given
// name in an uncompressed zip file, where the contents of the file can
// be found in embeddedTzdata.
// This is similar to time.loadTzinfoFromZip.
private static (@string, error) loadFromEmbeddedTZData(@string name) {
    @string _p0 = default;
    error _p0 = default!;

    const nuint zecheader = 0x06054b50;
    const nuint zcheader = 0x02014b50;
    const nint ztailsize = 22;

    const nint zheadersize = 30;
    const nuint zheader = 0x04034b50;

    var z = zipdata;

    var idx = len(z) - ztailsize;
    var n = get2s(z[(int)idx + 10..]);
    idx = get4s(z[(int)idx + 16..]);

    for (nint i = 0; i < n; i++) { 
        // See time.loadTzinfoFromZip for zip entry layout.
        if (get4s(z[(int)idx..]) != zcheader) {
            break;
        }
        var meth = get2s(z[(int)idx + 10..]);
        var size = get4s(z[(int)idx + 24..]);
        var namelen = get2s(z[(int)idx + 28..]);
        var xlen = get2s(z[(int)idx + 30..]);
        var fclen = get2s(z[(int)idx + 32..]);
        var off = get4s(z[(int)idx + 42..]);
        var zname = z[(int)idx + 46..(int)idx + 46 + namelen];
        idx += 46 + namelen + xlen + fclen;
        if (zname != name) {
            continue;
        }
        if (meth != 0) {
            return ("", error.As(errors.New("unsupported compression for " + name + " in embedded tzdata"))!);
        }
        idx = off;
        if (get4s(z[(int)idx..]) != zheader || get2s(z[(int)idx + 8..]) != meth || get2s(z[(int)idx + 26..]) != namelen || z[(int)idx + 30..(int)idx + 30 + namelen] != name) {
            return ("", error.As(errors.New("corrupt embedded tzdata"))!);
        }
        xlen = get2s(z[(int)idx + 28..]);
        idx += 30 + namelen + xlen;
        return (z[(int)idx..(int)idx + size], error.As(null!)!);

    }

    return ("", error.As(syscall.ENOENT)!);

}

} // end tzdata_package
