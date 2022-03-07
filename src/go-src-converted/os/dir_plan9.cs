// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:46 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dir_plan9.go
using io = go.io_package;
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static (slice<@string>, slice<DirEntry>, slice<FileInfo>, error) readdir(this ptr<File> _addr_file, nint n, readdirMode mode) {
    slice<@string> names = default;
    slice<DirEntry> dirents = default;
    slice<FileInfo> infos = default;
    error err = default!;
    ref File file = ref _addr_file.val;
 
    // If this file has no dirinfo, create one.
    if (file.dirinfo == null) {
        file.dirinfo = @new<dirInfo>();
    }
    var d = file.dirinfo;
    var size = n;
    if (size <= 0) {
        size = 100;
        n = -1;
    }
    while (n != 0) { 
        // Refill the buffer if necessary.
        if (d.bufp >= d.nbuf) {
            var (nb, err) = file.Read(d.buf[..]); 

            // Update the buffer state before checking for errors.
            (d.bufp, d.nbuf) = (0, nb);            if (err != null) {
                if (err == io.EOF) {
                    break;
                }
                return (names, dirents, infos, error.As(addr(new PathError(Op:"readdir",Path:file.name,Err:err))!)!);

            }
            if (nb < syscall.STATFIXLEN) {
                return (names, dirents, infos, error.As(addr(new PathError(Op:"readdir",Path:file.name,Err:syscall.ErrShortStat))!)!);
            }
        }
        var b = d.buf[(int)d.bufp..];
        var m = int(uint16(b[0]) | uint16(b[1]) << 8) + 2;
        if (m < syscall.STATFIXLEN) {
            return (names, dirents, infos, error.As(addr(new PathError(Op:"readdir",Path:file.name,Err:syscall.ErrShortStat))!)!);
        }
        var (dir, err) = syscall.UnmarshalDir(b[..(int)m]);
        if (err != null) {
            return (names, dirents, infos, error.As(addr(new PathError(Op:"readdir",Path:file.name,Err:err))!)!);
        }
        if (mode == readdirName) {
            names = append(names, dir.Name);
        }
        else
 {
            var f = fileInfoFromStat(dir);
            if (mode == readdirDirEntry) {
                dirents = append(dirents, new dirEntry(f));
            }
            else
 {
                infos = append(infos, f);
            }
        }
        d.bufp += m;
        n--;

    }

    if (n > 0 && len(names) + len(dirents) + len(infos) == 0) {
        return (null, null, null, error.As(io.EOF)!);
    }
    return (names, dirents, infos, error.As(null!)!);

}

private partial struct dirEntry {
    public ptr<fileStat> fs;
}

private static @string Name(this dirEntry de) {
    return de.fs.Name();
}
private static bool IsDir(this dirEntry de) {
    return de.fs.IsDir();
}
private static FileMode Type(this dirEntry de) {
    return de.fs.Mode().Type();
}
private static (FileInfo, error) Info(this dirEntry de) {
    FileInfo _p0 = default;
    error _p0 = default!;

    return (de.fs, error.As(null!)!);
}

} // end os_package
