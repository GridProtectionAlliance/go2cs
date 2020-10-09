// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2020 October 09 05:56:21 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\str.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static @string itoa(long val)
        { // do it here rather than with fmt to avoid dependency
            if (val < 0L)
            {
                return "-" + uitoa(uint(-val));
            }
            return uitoa(uint(val));

        }

        private static @string uitoa(ulong val)
        {
            array<byte> buf = new array<byte>(32L); // big enough for int64
            var i = len(buf) - 1L;
            while (val >= 10L)
            {
                buf[i] = byte(val % 10L + '0');
                i--;
                val /= 10L;
            }

            buf[i] = byte(val + '0');
            return string(buf[i..]);

        }
    }
}}}}}}
