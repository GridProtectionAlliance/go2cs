// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:54 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\env_plan9.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
 
        // Plan 9 environment device
        private static readonly @string envDir = (@string)"/env/"; 
        // size of buffer to read from a directory
        private static readonly long dirBufSize = (long)4096L; 
        // size of buffer to read an environment variable (may grow)
        private static readonly long envBufSize = (long)128L; 
        // offset of the name field in a 9P directory entry - see syscall.UnmarshalDir()
        private static readonly long nameOffset = (long)39L;


        // Goenvs caches the Plan 9 environment variables at start of execution into
        // string array envs, to supply the initial contents for os.Environ.
        // Subsequent calls to os.Setenv will change this cache, without writing back
        // to the (possibly shared) Plan 9 environment, so that Setenv and Getenv
        // conform to the same Posix semantics as on other operating systems.
        // For Plan 9 shared environment semantics, instead of Getenv(key) and
        // Setenv(key, value), one can use ioutil.ReadFile("/env/" + key) and
        // ioutil.WriteFile("/env/" + key, value, 0666) respectively.
        //go:nosplit
        private static void goenvs() => func((defer, _, __) =>
        {
            var buf = make_slice<byte>(envBufSize);
            copy(buf, envDir);
            var dirfd = open(_addr_buf[0L], _OREAD, 0L);
            if (dirfd < 0L)
            {
                return ;
            }

            defer(closefd(dirfd));
            dofiles(dirfd, name =>
            {
                name = append(name, 0L);
                buf = buf[..len(envDir)];
                copy(buf, envDir);
                buf = append(buf, name);
                var fd = open(_addr_buf[0L], _OREAD, 0L);
                if (fd < 0L)
                {
                    return ;
                }

                defer(closefd(fd));
                var n = len(buf);
                long r = 0L;
                while (true)
                {
                    r = int(pread(fd, @unsafe.Pointer(_addr_buf[0L]), int32(n), 0L));
                    if (r < n)
                    {
                        break;
                    }

                    n = int(seek(fd, 0L, 2L)) + 1L;
                    if (len(buf) < n)
                    {
                        buf = make_slice<byte>(n);
                    }

                }

                if (r <= 0L)
                {
                    r = 0L;
                }
                else if (buf[r - 1L] == 0L)
                {
                    r--;
                }

                name[len(name) - 1L] = '=';
                var env = make_slice<byte>(len(name) + r);
                copy(env, name);
                copy(env[len(name)..], buf[..r]);
                envs = append(envs, string(env));

            });

        });

        // Dofiles reads the directory opened with file descriptor fd, applying function f
        // to each filename in it.
        //go:nosplit
        private static void dofiles(int dirfd, Action<slice<byte>> f)
        {
            ptr<var> dirbuf = @new<[dirBufSize]byte>();

            long off = 0L;
            while (true)
            {
                var n = pread(dirfd, @unsafe.Pointer(_addr_dirbuf[0L]), int32(dirBufSize), off);
                if (n <= 0L)
                {
                    return ;
                }

                {
                    var b = dirbuf[..n];

                    while (len(b) > 0L)
                    {
                        slice<byte> name = default;
                        name, b = gdirname(b);
                        if (name == null)
                        {
                            return ;
                        }

                        f(name);

                    }

                }
                off += int64(n);

            }


        }

        // Gdirname returns the first filename from a buffer of directory entries,
        // and a slice containing the remaining directory entries.
        // If the buffer doesn't start with a valid directory entry, the returned name is nil.
        //go:nosplit
        private static (slice<byte>, slice<byte>) gdirname(slice<byte> buf)
        {
            slice<byte> name = default;
            slice<byte> rest = default;

            if (2L + nameOffset + 2L > len(buf))
            {
                return ;
            }

            var (entryLen, buf) = gbit16(buf);
            if (entryLen > len(buf))
            {
                return ;
            }

            var (n, b) = gbit16(buf[nameOffset..]);
            if (n > len(b))
            {
                return ;
            }

            name = b[..n];
            rest = buf[entryLen..];
            return ;

        }

        // Gbit16 reads a 16-bit little-endian binary number from b and returns it
        // with the remaining slice of b.
        //go:nosplit
        private static (long, slice<byte>) gbit16(slice<byte> b)
        {
            long _p0 = default;
            slice<byte> _p0 = default;

            return (int(b[0L]) | int(b[1L]) << (int)(8L), b[2L..]);
        }
    }
}
