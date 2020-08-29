// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\env_plan9.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static array<byte> tracebackbuf = new array<byte>(128L);

        private static @string gogetenv(@string key)
        {
            array<byte> file = new array<byte>(128L);
            if (len(key) > len(file) - 6L)
            {
                return "";
            }
            copy(file[..], "/env/");
            copy(file[5L..], key);

            var fd = open(ref file[0L], _OREAD, 0L);
            if (fd < 0L)
            {
                return "";
            }
            var n = seek(fd, 0L, 2L);
            if (n <= 0L)
            {
                closefd(fd);
                return "";
            }
            var p = make_slice<byte>(n);

            var r = pread(fd, @unsafe.Pointer(ref p[0L]), int32(n), 0L);
            closefd(fd);
            if (r < 0L)
            {
                return "";
            }
            if (p[r - 1L] == 0L)
            {
                r--;
            }
            @string s = default;
            var sp = stringStructOf(ref s);
            sp.str = @unsafe.Pointer(ref p[0L]);
            sp.len = int(r);
            return s;
        }

        private static unsafe.Pointer _cgo_setenv = default; // pointer to C function
        private static unsafe.Pointer _cgo_unsetenv = default; // pointer to C function
    }
}
