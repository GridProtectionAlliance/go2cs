// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

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
namespace go.time;

// The test for this package is time/tzdata_test.go.
using errors = errors_package;
using syscall = syscall_package;
using _ = unsafe_package; // for go:linkname

partial class tzdata_package {

// registerLoadFromEmbeddedTZData is defined in package time.
//
//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
internal static partial void registerLoadFromEmbeddedTZData(Func<@string, (string, error)> _);

[GoInit] internal static void init() {
    registerLoadFromEmbeddedTZData(loadFromEmbeddedTZData);
}

// get4s returns the little-endian 32-bit value at the start of s.
internal static nint get4s(@string s) {
    if (len(s) < 4) {
        return 0;
    }
    return (nint)((nint)((nint)(((nint)s[0]) | ((nint)s[1]) << (int)(8)) | ((nint)s[2]) << (int)(16)) | ((nint)s[3]) << (int)(24));
}

// get2s returns the little-endian 16-bit value at the start of s.
internal static nint get2s(@string s) {
    if (len(s) < 2) {
        return 0;
    }
    return (nint)(((nint)s[0]) | ((nint)s[1]) << (int)(8));
}

// loadFromEmbeddedTZData returns the contents of the file with the given
// name in an uncompressed zip file, where the contents of the file can
// be found in embeddedTzdata.
// This is similar to time.loadTzinfoFromZip.
internal static (@string, error) loadFromEmbeddedTZData(@string name) {
    static readonly UntypedInt zecheader = /* 0x06054b50 */ 101010256;
    static readonly UntypedInt zcheader = /* 0x02014b50 */ 33639248;
    static readonly UntypedInt ztailsize = 22;
    static readonly UntypedInt zheadersize = 30;
    static readonly UntypedInt zheader = /* 0x04034b50 */ 67324752;
    // zipdata is provided by zzipdata.go,
    // which is generated by cmd/dist during make.bash.
    @string z = zipdata;
    nint idx = len(z) - ztailsize;
    nint n = get2s(z[(int)(idx + 10)..]);
    idx = get4s(z[(int)(idx + 16)..]);
    for (nint i = 0; i < n; i++) {
        // See time.loadTzinfoFromZip for zip entry layout.
        if (get4s(z[(int)(idx)..]) != zcheader) {
            break;
        }
        nint meth = get2s(z[(int)(idx + 10)..]);
        nint size = get4s(z[(int)(idx + 24)..]);
        nint namelen = get2s(z[(int)(idx + 28)..]);
        nint xlen = get2s(z[(int)(idx + 30)..]);
        nint fclen = get2s(z[(int)(idx + 32)..]);
        nint off = get4s(z[(int)(idx + 42)..]);
        @string zname = z[(int)(idx + 46)..(int)(idx + 46 + namelen)];
        idx += 46 + namelen + xlen + fclen;
        if (zname != name) {
            continue;
        }
        if (meth != 0) {
            return ("", errors.New("unsupported compression for "u8 + name + " in embedded tzdata"u8));
        }
        // See time.loadTzinfoFromZip for zip per-file header layout.
        idx = off;
        if (get4s(z[(int)(idx)..]) != zheader || get2s(z[(int)(idx + 8)..]) != meth || get2s(z[(int)(idx + 26)..]) != namelen || z[(int)(idx + 30)..(int)(idx + 30 + namelen)] != name) {
            return ("", errors.New("corrupt embedded tzdata"u8));
        }
        xlen = get2s(z[(int)(idx + 28)..]);
        idx += 30 + namelen + xlen;
        return (z[(int)(idx)..(int)(idx + size)], default!);
    }
    return ("", syscall.ENOENT);
}

} // end tzdata_package
