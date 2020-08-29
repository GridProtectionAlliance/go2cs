// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris windows

// package runtime -- go2cs converted at 2020 August 29 08:16:54 UTC
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
                if (len(s) > len(key) && s[len(key)] == '=' && s[..len(key)] == key)
                {
                    return s[len(key) + 1L..];
                }
            }            return "";
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
                return;
            }
            array<unsafe.Pointer> arg = new array<unsafe.Pointer>(new unsafe.Pointer[] { cstring(k), cstring(v) });
            asmcgocall(_cgo_setenv, @unsafe.Pointer(ref arg));
        }

        // Update the C environment if cgo is loaded.
        // Called from syscall.unsetenv.
        //go:linkname syscall_unsetenv_c syscall.unsetenv_c
        private static void syscall_unsetenv_c(@string k)
        {
            if (_cgo_unsetenv == null)
            {
                return;
            }
            array<unsafe.Pointer> arg = new array<unsafe.Pointer>(new unsafe.Pointer[] { cstring(k) });
            asmcgocall(_cgo_unsetenv, @unsafe.Pointer(ref arg));
        }

        private static unsafe.Pointer cstring(@string s)
        {
            var p = make_slice<byte>(len(s) + 1L);
            copy(p, s);
            return @unsafe.Pointer(ref p[0L]);
        }
    }
}
