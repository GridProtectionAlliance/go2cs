// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:42 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dir_darwin.go
using io = go.io_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class os_package {

    // Auxiliary information if the File describes a directory
private partial struct dirInfo {
    public System.UIntPtr dir; // Pointer to DIR structure from dirent.h
}

private static void close(this ptr<dirInfo> _addr_d) {
    ref dirInfo d = ref _addr_d.val;

    if (d.dir == 0) {
        return ;
    }
    closedir(d.dir);
    d.dir = 0;

}

private static (slice<@string>, slice<DirEntry>, slice<FileInfo>, error) readdir(this ptr<File> _addr_f, nint n, readdirMode mode) {
    slice<@string> names = default;
    slice<DirEntry> dirents = default;
    slice<FileInfo> infos = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    if (f.dirinfo == null) {
        var (dir, call, errno) = f.pfd.OpenDir();
        if (errno != null) {
            return (null, null, null, error.As(addr(new PathError(Op:call,Path:f.name,Err:errno))!)!);
        }
        f.dirinfo = addr(new dirInfo(dir:dir,));

    }
    var d = f.dirinfo;

    var size = n;
    if (size <= 0) {
        size = 100;
        n = -1;
    }
    ref syscall.Dirent dirent = ref heap(out ptr<syscall.Dirent> _addr_dirent);
    ptr<syscall.Dirent> entptr;
    while (len(names) + len(dirents) + len(infos) < size || n == -1) {
        {
            var errno = readdir_r(d.dir, _addr_dirent, _addr_entptr);

            if (errno != 0) {
                if (errno == syscall.EINTR) {
                    continue;
                }
                return (names, dirents, infos, error.As(addr(new PathError(Op:"readdir",Path:f.name,Err:errno))!)!);
            }

        }

        if (entptr == null) { // EOF
            break;

        }
        if (dirent.Ino == 0) {
            continue;
        }
        ptr<array<byte>> name = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_dirent.Name))[..];
        foreach (var (i, c) in name) {
            if (c == 0) {
                name = name[..(int)i];
                break;
            }
        }        if (string(name) == "." || string(name) == "..") {
            continue;
        }
        if (mode == readdirName) {
            names = append(names, string(name));
        }
        else if (mode == readdirDirEntry) {
            var (de, err) = newUnixDirent(f.name, string(name), dtToType(dirent.Type));
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
        runtime.KeepAlive(f);

    }

    if (n > 0 && len(names) + len(dirents) + len(infos) == 0) {
        return (null, null, null, error.As(io.EOF)!);
    }
    return (names, dirents, infos, error.As(null!)!);

}

private static FileMode dtToType(byte typ) {

    if (typ == syscall.DT_BLK) 
        return ModeDevice;
    else if (typ == syscall.DT_CHR) 
        return ModeDevice | ModeCharDevice;
    else if (typ == syscall.DT_DIR) 
        return ModeDir;
    else if (typ == syscall.DT_FIFO) 
        return ModeNamedPipe;
    else if (typ == syscall.DT_LNK) 
        return ModeSymlink;
    else if (typ == syscall.DT_REG) 
        return 0;
    else if (typ == syscall.DT_SOCK) 
        return ModeSocket;
        return ~FileMode(0);

}

// Implemented in syscall/syscall_darwin.go.

//go:linkname closedir syscall.closedir
private static error closedir(System.UIntPtr dir);

//go:linkname readdir_r syscall.readdir_r
private static syscall.Errno readdir_r(System.UIntPtr dir, ptr<syscall.Dirent> entry, ptr<ptr<syscall.Dirent>> result);

} // end os_package
