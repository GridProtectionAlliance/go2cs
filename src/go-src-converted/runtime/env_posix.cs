// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris windows plan9

// package runtime -- go2cs converted at 2020 October 09 04:45:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\env_posix.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static @string gogetenv(@string key)
        {
            var env = environ();
            if (env == null)
            {
                throw("getenv before env init");
            }
            foreach (var (_, s) in env)
            {
                if (len(s) > len(key) && s[len(key)] == '=' && envKeyEqual(s[..len(key)], key))
                {
                    return s[len(key) + 1L..];
                }
            }            return "";

        }

        // envKeyEqual reports whether a == b, with ASCII-only case insensitivity
        // on Windows. The two strings must have the same length.
        private static bool envKeyEqual(@string a, @string b)
        {
            if (GOOS == "windows")
            { // case insensitive
                for (long i = 0L; i < len(a); i++)
                {
                    var ca = a[i];
                    var cb = b[i];
                    if (ca == cb || lowerASCII(ca) == lowerASCII(cb))
                    {
                        continue;
                    }

                    return false;

                }

                return true;

            }

            return a == b;

        }

        private static byte lowerASCII(byte c)
        {
            if ('A' <= c && c <= 'Z')
            {
                return c + ('a' - 'A');
            }

            return c;

        }

        private static unsafe.Pointer _cgo_setenv = default; // pointer to C function
        private static unsafe.Pointer _cgo_unsetenv = default; // pointer to C function

        // Update the C environment if cgo is loaded.
        // Called from syscall.Setenv.
        //go:linkname syscall_setenv_c syscall.setenv_c
        private static void syscall_setenv_c(@string k, @string v)
        {
            if (_cgo_setenv == null)
            {
                return ;
            }

            ref array<unsafe.Pointer> arg = ref heap(new array<unsafe.Pointer>(new unsafe.Pointer[] { cstring(k), cstring(v) }), out ptr<array<unsafe.Pointer>> _addr_arg);
            asmcgocall(_cgo_setenv, @unsafe.Pointer(_addr_arg));

        }

        // Update the C environment if cgo is loaded.
        // Called from syscall.unsetenv.
        //go:linkname syscall_unsetenv_c syscall.unsetenv_c
        private static void syscall_unsetenv_c(@string k)
        {
            if (_cgo_unsetenv == null)
            {
                return ;
            }

            ref array<unsafe.Pointer> arg = ref heap(new array<unsafe.Pointer>(new unsafe.Pointer[] { cstring(k) }), out ptr<array<unsafe.Pointer>> _addr_arg);
            asmcgocall(_cgo_unsetenv, @unsafe.Pointer(_addr_arg));

        }

        private static unsafe.Pointer cstring(@string s)
        {
            var p = make_slice<byte>(len(s) + 1L);
            copy(p, s);
            return @unsafe.Pointer(_addr_p[0L]);
        }
    }
}
