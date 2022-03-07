// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:48 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dir_windows.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private static (slice<@string>, slice<DirEntry>, slice<FileInfo>, error) readdir(this ptr<File> _addr_file, nint n, readdirMode mode) {
    slice<@string> names = default;
    slice<DirEntry> dirents = default;
    slice<FileInfo> infos = default;
    error err = default!;
    ref File file = ref _addr_file.val;

    if (!file.isdir()) {
        return (null, null, null, error.As(addr(new PathError(Op:"readdir",Path:file.name,Err:syscall.ENOTDIR))!)!);
    }
    var wantAll = n <= 0;
    if (wantAll) {
        n = -1;
    }
    var d = _addr_file.dirinfo.data;
    while (n != 0 && !file.dirinfo.isempty) {
        if (file.dirinfo.needdata) {
            var e = file.pfd.FindNextFile(d);
            runtime.KeepAlive(file);
            if (e != null) {
                if (e == syscall.ERROR_NO_MORE_FILES) {
                    break;
                }
                else
 {
                    err = addr(new PathError(Op:"FindNextFile",Path:file.name,Err:e));
                    return ;
                }
            }
        }
        file.dirinfo.needdata = true;
        var name = syscall.UTF16ToString(d.FileName[(int)0..]);
        if (name == "." || name == "..") { // Useless names
            continue;

        }
        if (mode == readdirName) {
            names = append(names, name);
        }
        else
 {
            var f = newFileStatFromWin32finddata(d);
            f.name = name;
            f.path = file.dirinfo.path;
            f.appendNameToPath = true;
            if (mode == readdirDirEntry) {
                dirents = append(dirents, new dirEntry(f));
            }
            else
 {
                infos = append(infos, f);
            }
        }
        n--;

    }
    if (!wantAll && len(names) + len(dirents) + len(infos) == 0) {
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
