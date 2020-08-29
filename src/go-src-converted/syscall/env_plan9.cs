// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 environment variables.

// package syscall -- go2cs converted at 2020 August 29 08:36:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\env_plan9.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static var errZeroLengthKey = errors.New("zero length key");        private static var errShortWrite = errors.New("i/o count too small");

        private static (@string, error) readenv(@string key) => func((defer, _, __) =>
        {
            var (fd, err) = open("/env/" + key, O_RDONLY);
            if (err != null)
            {
                return ("", err);
            }
            defer(Close(fd));
            var (l, _) = Seek(fd, 0L, 2L);
            Seek(fd, 0L, 0L);
            var buf = make_slice<byte>(l);
            var (n, err) = Read(fd, buf);
            if (err != null)
            {
                return ("", err);
            }
            if (n > 0L && buf[n - 1L] == 0L)
            {
                buf = buf[..n - 1L];
            }
            return (string(buf), null);
        });

        private static error writeenv(@string key, @string value) => func((defer, _, __) =>
        {
            var (fd, err) = create("/env/" + key, O_RDWR, 0666L);
            if (err != null)
            {
                return error.As(err);
            }
            defer(Close(fd));
            slice<byte> b = (slice<byte>)value;
            var (n, err) = Write(fd, b);
            if (err != null)
            {
                return error.As(err);
            }
            if (n != len(b))
            {
                return error.As(errShortWrite);
            }
            return error.As(null);
        });

        public static (@string, bool) Getenv(@string key)
        {
            if (len(key) == 0L)
            {
                return ("", false);
            }
            var (v, err) = readenv(key);
            if (err != null)
            {
                return ("", false);
            }
            return (v, true);
        }

        public static error Setenv(@string key, @string value)
        {
            if (len(key) == 0L)
            {
                return error.As(errZeroLengthKey);
            }
            var err = writeenv(key, value);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(null);
        }

        public static void Clearenv()
        {
            RawSyscall(SYS_RFORK, RFCENVG, 0L, 0L);
        }

        public static error Unsetenv(@string key)
        {
            if (len(key) == 0L)
            {
                return error.As(errZeroLengthKey);
            }
            Remove("/env/" + key);
            return error.As(null);
        }

        public static slice<@string> Environ() => func((defer, _, __) =>
        {
            var (fd, err) = open("/env", O_RDONLY);
            if (err != null)
            {
                return null;
            }
            defer(Close(fd));
            var (files, err) = readdirnames(fd);
            if (err != null)
            {
                return null;
            }
            var ret = make_slice<@string>(0L, len(files));

            foreach (var (_, key) in files)
            {
                var (v, err) = readenv(key);
                if (err != null)
                {
                    continue;
                }
                ret = append(ret, key + "=" + v);
            }
            return ret;
        });
    }
}
