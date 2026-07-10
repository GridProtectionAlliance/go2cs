// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;
using go.io;
using go.sync;

partial class os_package {

// Auxiliary information if the File describes a directory
[GoType] partial struct dirInfo {
    internal Δsync.Mutex mu;
    // buf is a slice pointer so the slice header
    // does not escape to the heap when returning
    // buf to dirBufPool.
    internal ж<slice<byte>> buf; // buffer for directory I/O
    internal nint bufp;    // location of next record in buf
    internal syscallꓸHandle h;
    internal uint32 vol;
    internal uint32 @class; // type of entries in buf
    internal @string path; // absolute directory path, empty if the file system supports FILE_ID_BOTH_DIR_INFO
}

internal static readonly UntypedInt dirBufSize = /* 64 * 1024 */ 65536; // 64kB

// The buffer must be at least a block long.
internal static ж<Δsync.Pool> ᏑdirBufPool = new(new Δsync.Pool(
    New: () => {
        var buf = new slice<byte>(dirBufSize);
        return Ꮡ(buf);
    }
));
internal static ref Δsync.Pool dirBufPool => ref ᏑdirBufPool.Value;

[GoRecv] internal static void close(this ref dirInfo d) {
    d.h = 0;
    if (d.buf != nil) {
        ᏑdirBufPool.Put(d.buf);
        d.buf = default!;
    }
}

// allowReadDirFileID indicates whether File.readdir should try to use FILE_ID_BOTH_DIR_INFO
// if the underlying file system supports it.
// Useful for testing purposes.
internal static bool allowReadDirFileID = true;

internal static void init(this ж<dirInfo> Ꮡd, syscallꓸHandle h) {
    ref var d = ref Ꮡd.Value;

    d.h = h;
    d.@class = windows.FileFullDirectoryRestartInfo;
    // The previous settings are enough to read the directory entries.
    // The following code is only needed to support os.SameFile.
    // It is safe to query d.vol once and reuse the value.
    // Hard links are not allowed to reference files in other volumes.
    // Junctions and symbolic links can reference files and directories in other volumes,
    // but the reparse point should still live in the parent volume.
    ref var flags = ref heap(new uint32(), out var Ꮡflags);
    var err = windows.GetVolumeInformationByHandle(h, nil, 0, Ꮡd.of(dirInfo.Ꮡvol), nil, Ꮡflags, nil, 0);
    if (err != default!) {
        d.vol = 0;
        // Set to zero in case Windows writes garbage to it.
        // If we can't get the volume information, we can't use os.SameFile,
        // but we can still read the directory entries.
        return;
    }
    if ((uint32)(flags & (uint32)windows.FILE_SUPPORTS_OBJECT_IDS) == 0) {
        // The file system does not support object IDs, no need to continue.
        return;
    }
    if (allowReadDirFileID && (uint32)(flags & (uint32)windows.FILE_SUPPORTS_OPEN_BY_FILE_ID) != 0){
        // Use FileIdBothDirectoryRestartInfo if available as it returns the file ID
        // without the need to open the file.
        d.@class = windows.FileIdBothDirectoryRestartInfo;
    } else {
        // If FileIdBothDirectoryRestartInfo is not available but objects IDs are supported,
        // get the directory path so that os.SameFile can use it to open the file
        // and retrieve the file ID.
        (d.path, _) = windows.FinalPath(h, windows.FILE_NAME_OPENED);
    }
}

internal static (slice<@string> names, slice<DirEntry> dirents, slice<FileInfo> infos, error err) readdir(this ж<File> Ꮡfile, nint n, readdirMode mode) {
    slice<@string> names = default!;
    slice<DirEntry> dirents = default!;
    slice<FileInfo> infos = default!;
    error err = default!;
    func((defer, recover) => {
    ref var @file = ref Ꮡfile.Value;

        // If this file has no dirInfo, create one.
        ж<dirInfo> d = default!;
        while (ᐧ) {
            d = Ꮡfile.of(File.Ꮡdirinfo).Load();
            if (d != nil) {
                break;
            }
            d = @new<dirInfo>();
            d.init(@file.pfd.Sysfd);
            if (Ꮡfile.of(File.Ꮡdirinfo).CompareAndSwap(nil, d)) {
                break;
            }
            // We lost the race: try again.
            d.close();
        }
        d.of(dirInfo.Ꮡmu).Lock();
        var dʗ1 = d;
        defer(dʗ1.of(dirInfo.Ꮡmu).Unlock);
        if ((~d).buf == nil) {
            d.Value.buf = ᏑdirBufPool.Get()._<ж<slice<byte>>>();
        }
        var wantAll = n <= 0;
        if (wantAll) {
            n = -1;
        }
        while (n != 0) {
            // Refill the buffer if necessary
            if ((~d).bufp == 0) {
                err = windows.GetFileInformationByHandleEx(@file.pfd.Sysfd, (~d).@class, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(((~d).buf.ValueSlot), 0))), (uint32)len((~d).buf.ValueSlot));
                Δruntime.KeepAlive(@file);
                if (err != default!) {
                    if (AreEqual(err, syscall.ERROR_NO_MORE_FILES)) {
                        // Optimization: we can return the buffer to the pool, there is nothing else to read.
                        ᏑdirBufPool.Put((~d).buf);
                        d.Value.buf = default!;
                        break;
                    }
                    if (AreEqual(err, syscall.ERROR_FILE_NOT_FOUND) && ((~d).@class == windows.FileIdBothDirectoryRestartInfo || (~d).@class == windows.FileFullDirectoryRestartInfo)) {
                        // GetFileInformationByHandleEx doesn't document the return error codes when the info class is FileIdBothDirectoryRestartInfo,
                        // but MS-FSA 2.1.5.6.3 [1] specifies that the underlying file system driver should return STATUS_NO_SUCH_FILE when
                        // reading an empty root directory, which is mapped to ERROR_FILE_NOT_FOUND by Windows.
                        // Note that some file system drivers may never return this error code, as the spec allows to return the "." and ".."
                        // entries in such cases, making the directory appear non-empty.
                        // The chances of false positive are very low, as we know that the directory exists, else GetVolumeInformationByHandle
                        // would have failed, and that the handle is still valid, as we haven't closed it.
                        // See go.dev/issue/61159.
                        // [1] https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fsa/fa8194e0-53ec-413b-8315-e8fa85396fd8
                        break;
                    }
                    {
                        var (s, _) = Ꮡfile.Stat(); if (s != default! && !s.IsDir()){
                            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: @file.name, Err: syscall.ENOTDIR)));
                        } else {
                            err = new fs.PathErrorжerror(Ꮡ(new PathError(Op: "GetFileInformationByHandleEx"u8, Path: @file.name, Err: err)));
                        }
                    }
                    return;
                }
                if ((~d).@class == windows.FileIdBothDirectoryRestartInfo){
                    d.Value.@class = windows.FileIdBothDirectoryInfo;
                } else 
                if ((~d).@class == windows.FileFullDirectoryRestartInfo) {
                    d.Value.@class = windows.FileFullDirectoryInfo;
                }
            }
            // Drain the buffer
            bool islast = default!;
            while (n != 0 && !islast) {
                uint32 nextEntryOffset = default!;
                slice<uint16> nameslice = default!;
                @unsafe.Pointer entry = new @unsafe.Pointer(Ꮡ(((~d).buf.ValueSlot), (~d).bufp));
                if ((~d).@class == windows.FileIdBothDirectoryInfo){
                    var info = (ж<windows.FILE_ID_BOTH_DIR_INFO>)(uintptr)(entry);
                    nextEntryOffset = info.Value.NextEntryOffset;
                    nameslice = @unsafe.Slice(info.at(windows.FILE_ID_BOTH_DIR_INFO.ᏑFileName, 0), (~info).FileNameLength / 2);
                } else {
                    var info = (ж<windows.FILE_FULL_DIR_INFO>)(uintptr)(entry);
                    nextEntryOffset = info.Value.NextEntryOffset;
                    nameslice = @unsafe.Slice(info.at(windows.FILE_FULL_DIR_INFO.ᏑFileName, 0), (~info).FileNameLength / 2);
                }
                d.Value.bufp += (nint)nextEntryOffset;
                islast = nextEntryOffset == 0;
                if (islast) {
                    d.Value.bufp = 0;
                }
                if ((len(nameslice) == 1 && nameslice[0] == (rune)'.') || (len(nameslice) == 2 && nameslice[0] == (rune)'.' && nameslice[1] == (rune)'.')) {
                    // Ignore "." and ".." and avoid allocating a string for them.
                    continue;
                }
                @string name = syscall.UTF16ToString(nameslice);
                if (mode == readdirName){
                    names = append(names, name);
                } else {
                    ж<fileStat> f = default!;
                    if ((~d).@class == windows.FileIdBothDirectoryInfo){
                        f = newFileStatFromFileIDBothDirInfo((ж<windows.FILE_ID_BOTH_DIR_INFO>)(uintptr)(entry));
                    } else {
                        f = newFileStatFromFileFullDirInfo((ж<windows.FILE_FULL_DIR_INFO>)(uintptr)(entry));
                        if ((~d).path != ""u8) {
                            // Defer appending the entry name to the parent directory path until
                            // it is really needed, to avoid allocating a string that may not be used.
                            // It is currently only used in os.SameFile.
                            f.Value.appendNameToPath = true;
                            f.Value.path = d.Value.path;
                        }
                    }
                    f.Value.name = name;
                    f.Value.vol = d.Value.vol;
                    if (mode == readdirDirEntry){
                        dirents = append(dirents, (DirEntry)(new dirEntry(f)));
                    } else {
                        infos = append(infos, (FileInfo)(new fileStatжFileInfo(f)));
                    }
                }
                n--;
            }
        }
        if (!wantAll && len(names) + len(dirents) + len(infos) == 0) {
            (names, dirents, infos, err) = (default!, default!, default!, Δio.EOF); return;
        }
        (names, dirents, infos, err) = (names, dirents, infos, default!);
    });
    return (names, dirents, infos, err);
}

[GoType] partial struct dirEntry {
    internal ж<fileStat> fs;
}

internal static @string Name(this dirEntry de) {
    return de.fs.Name();
}

internal static bool IsDir(this dirEntry de) {
    return de.fs.IsDir();
}

internal static FileMode Type(this dirEntry de) {
    return de.fs.Mode().Type();
}

internal static (FileInfo, error) Info(this dirEntry de) {
    return (new fileStatжFileInfo(de.fs), default!);
}

internal static @string String(this dirEntry de) {
    return fs.FormatDirEntry(de);
}

} // end os_package
