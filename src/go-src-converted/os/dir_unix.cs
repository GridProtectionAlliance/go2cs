// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2022 March 06 22:12:47 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dir_unix.go
using io = go.io_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class os_package {

    // Auxiliary information if the File describes a directory
private partial struct dirInfo {
    public ptr<slice<byte>> buf; // buffer for directory I/O
    public nint nbuf; // length of buf; return value from Getdirentries
    public nint bufp; // location of next record in buf.
}

 
// More than 5760 to work around https://golang.org/issue/24015.
private static readonly nint blockSize = 8192;


private static sync.Pool dirBufPool = new sync.Pool(New:func()interface{}{buf:=make([]byte,blockSize)return&buf},);

private static void close(this ptr<dirInfo> _addr_d) {
    ref dirInfo d = ref _addr_d.val;

    if (d.buf != null) {
        dirBufPool.Put(d.buf);
        d.buf = null;
    }
}

private static (slice<@string>, slice<DirEntry>, slice<FileInfo>, error) readdir(this ptr<File> _addr_f, nint n, readdirMode mode) {
    slice<@string> names = default;
    slice<DirEntry> dirents = default;
    slice<FileInfo> infos = default;
    error err = default!;
    ref File f = ref _addr_f.val;
 
    // If this file has no dirinfo, create one.
    if (f.dirinfo == null) {
        f.dirinfo = @new<dirInfo>();
        f.dirinfo.buf = dirBufPool.Get()._<ptr<slice<byte>>>();
    }
    var d = f.dirinfo; 

    // Change the meaning of n for the implementation below.
    //
    // The n above was for the public interface of "if n <= 0,
    // Readdir returns all the FileInfo from the directory in a
    // single slice".
    //
    // But below, we use only negative to mean looping until the
    // end and positive to mean bounded, with positive
    // terminating at 0.
    if (n == 0) {
        n = -1;
    }
    while (n != 0) { 
        // Refill the buffer if necessary
        if (d.bufp >= d.nbuf) {
            d.bufp = 0;
            error errno = default!;
            d.nbuf, errno = f.pfd.ReadDirent(d.buf.val);
            runtime.KeepAlive(f);
            if (errno != null) {
                return (names, dirents, infos, error.As(addr(new PathError(Op:"readdirent",Path:f.name,Err:errno))!)!);
            }
            if (d.nbuf <= 0) {
                break; // EOF
            }

        }
        var buf = (d.buf.val)[(int)d.bufp..(int)d.nbuf];
        var (reclen, ok) = direntReclen(buf);
        if (!ok || reclen > uint64(len(buf))) {
            break;
        }
        var rec = buf[..(int)reclen];
        d.bufp += int(reclen);
        var (ino, ok) = direntIno(rec);
        if (!ok) {
            break;
        }
        if (ino == 0) {
            continue;
        }
        const var namoff = uint64(@unsafe.Offsetof(new syscall.Dirent().Name));

        var (namlen, ok) = direntNamlen(rec);
        if (!ok || namoff + namlen > uint64(len(rec))) {
            break;
        }
        var name = rec[(int)namoff..(int)namoff + namlen];
        foreach (var (i, c) in name) {
            if (c == 0) {
                name = name[..(int)i];
                break;
            }
        }        if (string(name) == "." || string(name) == "..") {
            continue;
        }
        if (n > 0) { // see 'n == 0' comment above
            n--;

        }
        if (mode == readdirName) {
            names = append(names, string(name));
        }
        else if (mode == readdirDirEntry) {
            var (de, err) = newUnixDirent(f.name, string(name), direntType(rec));
            if (IsNotExist(err)) { 
                // File disappeared between readdir and stat.
                // Treat as if it didn't exist.
                continue;

            }

            if (err != null) {
                return (null, dirents, null, error.As(err)!);
            }

            dirents = append(dirents, de);

        }
        else
 {
            var (info, err) = lstat(f.name + "/" + string(name));
            if (IsNotExist(err)) { 
                // File disappeared between readdir + stat.
                // Treat as if it didn't exist.
                continue;

            }

            if (err != null) {
                return (null, null, infos, error.As(err)!);
            }

            infos = append(infos, info);

        }
    }

    if (n > 0 && len(names) + len(dirents) + len(infos) == 0) {
        return (null, null, null, error.As(io.EOF)!);
    }
    return (names, dirents, infos, error.As(null!)!);

}

// readInt returns the size-bytes unsigned integer in native byte order at offset off.
private static (ulong, bool) readInt(slice<byte> b, System.UIntPtr off, System.UIntPtr size) {
    ulong u = default;
    bool ok = default;

    if (len(b) < int(off + size)) {
        return (0, false);
    }
    if (isBigEndian) {
        return (readIntBE(b[(int)off..], size), true);
    }
    return (readIntLE(b[(int)off..], size), true);

}

private static ulong readIntBE(slice<byte> b, System.UIntPtr size) => func((_, panic, _) => {
    switch (size) {
        case 1: 
            return uint64(b[0]);
            break;
        case 2: 
            _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[1]) | uint64(b[0]) << 8;

            break;
        case 4: 
            _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[3]) | uint64(b[2]) << 8 | uint64(b[1]) << 16 | uint64(b[0]) << 24;

            break;
        case 8: 
            _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;

            break;
        default: 
            panic("syscall: readInt with unsupported size");
            break;
    }

});

private static ulong readIntLE(slice<byte> b, System.UIntPtr size) => func((_, panic, _) => {
    switch (size) {
        case 1: 
            return uint64(b[0]);
            break;
        case 2: 
            _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8;

            break;
        case 4: 
            _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24;

            break;
        case 8: 
            _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56;

            break;
        default: 
            panic("syscall: readInt with unsupported size");
            break;
    }

});

} // end os_package
