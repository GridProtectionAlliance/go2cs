// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:38 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\env_plan9.go
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

 
// Plan 9 environment device
private static readonly @string envDir = "/env/"; 
// size of buffer to read from a directory
private static readonly nint dirBufSize = 4096; 
// size of buffer to read an environment variable (may grow)
private static readonly nint envBufSize = 128; 
// offset of the name field in a 9P directory entry - see syscall.UnmarshalDir()
private static readonly nint nameOffset = 39;


// Goenvs caches the Plan 9 environment variables at start of execution into
// string array envs, to supply the initial contents for os.Environ.
// Subsequent calls to os.Setenv will change this cache, without writing back
// to the (possibly shared) Plan 9 environment, so that Setenv and Getenv
// conform to the same Posix semantics as on other operating systems.
// For Plan 9 shared environment semantics, instead of Getenv(key) and
// Setenv(key, value), one can use os.ReadFile("/env/" + key) and
// os.WriteFile("/env/" + key, value, 0666) respectively.
//go:nosplit
private static void goenvs() => func((defer, _, _) => {
    var buf = make_slice<byte>(envBufSize);
    copy(buf, envDir);
    var dirfd = open(_addr_buf[0], _OREAD, 0);
    if (dirfd < 0) {
        return ;
    }
    defer(closefd(dirfd));
    dofiles(dirfd, name => {
        name = append(name, 0);
        buf = buf[..(int)len(envDir)];
        copy(buf, envDir);
        buf = append(buf, name);
        var fd = open(_addr_buf[0], _OREAD, 0);
        if (fd < 0) {
            return ;
        }
        defer(closefd(fd));
        var n = len(buf);
        nint r = 0;
        while (true) {
            r = int(pread(fd, @unsafe.Pointer(_addr_buf[0]), int32(n), 0));
            if (r < n) {
                break;
            }
            n = int(seek(fd, 0, 2)) + 1;
            if (len(buf) < n) {
                buf = make_slice<byte>(n);
            }
        }
        if (r <= 0) {
            r = 0;
        }
        else if (buf[r - 1] == 0) {
            r--;
        }
        name[len(name) - 1] = '=';
        var env = make_slice<byte>(len(name) + r);
        copy(env, name);
        copy(env[(int)len(name)..], buf[..(int)r]);
        envs = append(envs, string(env));

    });

});

// Dofiles reads the directory opened with file descriptor fd, applying function f
// to each filename in it.
//go:nosplit
private static void dofiles(int dirfd, Action<slice<byte>> f) {
    ptr<var> dirbuf = @new<[dirBufSize]byte>();

    long off = 0;
    while (true) {
        var n = pread(dirfd, @unsafe.Pointer(_addr_dirbuf[0]), int32(dirBufSize), off);
        if (n <= 0) {
            return ;
        }
        {
            var b = dirbuf[..(int)n];

            while (len(b) > 0) {
                slice<byte> name = default;
                name, b = gdirname(b);
                if (name == null) {
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
private static (slice<byte>, slice<byte>) gdirname(slice<byte> buf) {
    slice<byte> name = default;
    slice<byte> rest = default;

    if (2 + nameOffset + 2 > len(buf)) {
        return ;
    }
    var (entryLen, buf) = gbit16(buf);
    if (entryLen > len(buf)) {
        return ;
    }
    var (n, b) = gbit16(buf[(int)nameOffset..]);
    if (n > len(b)) {
        return ;
    }
    name = b[..(int)n];
    rest = buf[(int)entryLen..];
    return ;

}

// Gbit16 reads a 16-bit little-endian binary number from b and returns it
// with the remaining slice of b.
//go:nosplit
private static (nint, slice<byte>) gbit16(slice<byte> b) {
    nint _p0 = default;
    slice<byte> _p0 = default;

    return (int(b[0]) | int(b[1]) << 8, b[(int)2..]);
}

} // end runtime_package
