// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows environment variables.

// package syscall -- go2cs converted at 2020 August 29 08:36:49 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\env_windows.go
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static (@string, bool) Getenv(@string key)
        {
            var (keyp, err) = UTF16PtrFromString(key);
            if (err != null)
            {
                return ("", false);
            }
            var n = uint32(100L);
            while (true)
            {
                var b = make_slice<ushort>(n);
                n, err = GetEnvironmentVariable(keyp, ref b[0L], uint32(len(b)));
                if (n == 0L && err == ERROR_ENVVAR_NOT_FOUND)
                {
                    return ("", false);
                }
                if (n <= uint32(len(b)))
                {
                    return (string(utf16.Decode(b[..n])), true);
                }
            }
        }

        public static error Setenv(@string key, @string value)
        {
            var (v, err) = UTF16PtrFromString(value);
            if (err != null)
            {
                return error.As(err);
            }
            var (keyp, err) = UTF16PtrFromString(key);
            if (err != null)
            {
                return error.As(err);
            }
            var e = SetEnvironmentVariable(keyp, v);
            if (e != null)
            {
                return error.As(e);
            }
            return error.As(null);
        }

        public static error Unsetenv(@string key)
        {
            var (keyp, err) = UTF16PtrFromString(key);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(SetEnvironmentVariable(keyp, null));
        }

        public static void Clearenv()
        {
            foreach (var (_, s) in Environ())
            { 
                // Environment variables can begin with =
                // so start looking for the separator = at j=1.
                // http://blogs.msdn.com/b/oldnewthing/archive/2010/05/06/10008132.aspx
                for (long j = 1L; j < len(s); j++)
                {
                    if (s[j] == '=')
                    {
                        Unsetenv(s[0L..j]);
                        break;
                    }
                }

            }
        }

        public static slice<@string> Environ() => func((defer, _, __) =>
        {
            var (s, e) = GetEnvironmentStrings();
            if (e != null)
            {
                return null;
            }
            defer(FreeEnvironmentStrings(s));
            var r = make_slice<@string>(0L, 50L); // Empty with room to grow.
            for (long from = 0L;
            long i = 0L;
            ref array<ushort> p = new ptr<ref array<ushort>>(@unsafe.Pointer(s)); true; i++)
            {
                if (p[i] == 0L)
                { 
                    // empty string marks the end
                    if (i <= from)
                    {
                        break;
                    }
                    r = append(r, string(utf16.Decode(p[from..i])));
                    from = i + 1L;
                }
            }

            return r;
        });
    }
}
