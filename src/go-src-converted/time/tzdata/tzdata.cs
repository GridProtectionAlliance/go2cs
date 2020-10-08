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
// 800 KB.
//
// This package should normally be imported by a program's main package,
// not by a library. Libraries normally shouldn't decide whether to
// include the timezone database in a program.
//
// This package will be automatically imported if you build with
// -tags timetzdata.
// package tzdata -- go2cs converted at 2020 October 08 00:33:52 UTC
// import "time/tzdata" ==> using tzdata = go.time.tzdata_package
// Original source: C:\Go\src\time\tzdata\tzdata.go
// The test for this package is time/tzdata_test.go.

using errors = go.errors_package;
using syscall = go.syscall_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace time
{
    public static partial class tzdata_package
    {
        // registerLoadFromEmbeddedTZData is defined in package time.
        //go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData
        private static (@string, error) registerLoadFromEmbeddedTZData(Func<@string, (@string, error)> _p0)
;

        private static void init()
        {
            registerLoadFromEmbeddedTZData(loadFromEmbeddedTZData);
        }

        // get4s returns the little-endian 32-bit value at the start of s.
        private static long get4s(@string s)
        {
            if (len(s) < 4L)
            {>>MARKER:FUNCTION_registerLoadFromEmbeddedTZData_BLOCK_PREFIX<<
                return 0L;
            }

            return int(s[0L]) | int(s[1L]) << (int)(8L) | int(s[2L]) << (int)(16L) | int(s[3L]) << (int)(24L);

        }

        // get2s returns the little-endian 16-bit value at the start of s.
        private static long get2s(@string s)
        {
            if (len(s) < 2L)
            {
                return 0L;
            }

            return int(s[0L]) | int(s[1L]) << (int)(8L);

        }

        // loadFromEmbeddedTZData returns the contents of the file with the given
        // name in an uncompressed zip file, where the contents of the file can
        // be found in embeddedTzdata.
        // This is similar to time.loadTzinfoFromZip.
        private static (@string, error) loadFromEmbeddedTZData(@string name)
        {
            @string _p0 = default;
            error _p0 = default!;

            const ulong zecheader = (ulong)0x06054b50UL;
            const ulong zcheader = (ulong)0x02014b50UL;
            const long ztailsize = (long)22L;

            const long zheadersize = (long)30L;
            const ulong zheader = (ulong)0x04034b50UL;

            var z = zipdata;

            var idx = len(z) - ztailsize;
            var n = get2s(z[idx + 10L..]);
            idx = get4s(z[idx + 16L..]);

            for (long i = 0L; i < n; i++)
            { 
                // See time.loadTzinfoFromZip for zip entry layout.
                if (get4s(z[idx..]) != zcheader)
                {
                    break;
                }

                var meth = get2s(z[idx + 10L..]);
                var size = get4s(z[idx + 24L..]);
                var namelen = get2s(z[idx + 28L..]);
                var xlen = get2s(z[idx + 30L..]);
                var fclen = get2s(z[idx + 32L..]);
                var off = get4s(z[idx + 42L..]);
                var zname = z[idx + 46L..idx + 46L + namelen];
                idx += 46L + namelen + xlen + fclen;
                if (zname != name)
                {
                    continue;
                }

                if (meth != 0L)
                {
                    return ("", error.As(errors.New("unsupported compression for " + name + " in embedded tzdata"))!);
                } 

                // See time.loadTzinfoFromZip for zip per-file header layout.
                idx = off;
                if (get4s(z[idx..]) != zheader || get2s(z[idx + 8L..]) != meth || get2s(z[idx + 26L..]) != namelen || z[idx + 30L..idx + 30L + namelen] != name)
                {
                    return ("", error.As(errors.New("corrupt embedded tzdata"))!);
                }

                xlen = get2s(z[idx + 28L..]);
                idx += 30L + namelen + xlen;
                return (z[idx..idx + size], error.As(null!)!);

            }


            return ("", error.As(syscall.ENOENT)!);

        }
    }
}}
