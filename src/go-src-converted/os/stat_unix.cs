// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package os -- go2cs converted at 2022 March 06 22:13:51 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\stat_unix.go
using syscall = go.syscall_package;
using System;


namespace go;

public static partial class os_package {

    // Stat returns the FileInfo structure describing file.
    // If there is an error, it will be of type *PathError.
private static (FileInfo, error) Stat(this ptr<File> _addr_f) {
    FileInfo _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    if (f == null) {
        return (null, error.As(ErrInvalid)!);
    }
    ref fileStat fs = ref heap(out ptr<fileStat> _addr_fs);
    var err = f.pfd.Fstat(_addr_fs.sys);
    if (err != null) {
        return (null, error.As(addr(new PathError(Op:"stat",Path:f.name,Err:err))!)!);
    }
    fillFileStatFromSys(_addr_fs, f.name);
    return (_addr_fs, error.As(null!)!);

}

// statNolog stats a file with no test logging.
private static (FileInfo, error) statNolog(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    ref fileStat fs = ref heap(out ptr<fileStat> _addr_fs);
    var err = ignoringEINTR(() => {
        return syscall.Stat(name, _addr_fs.sys);
    });
    if (err != null) {
        return (null, error.As(addr(new PathError(Op:"stat",Path:name,Err:err))!)!);
    }
    fillFileStatFromSys(_addr_fs, name);
    return (_addr_fs, error.As(null!)!);

}

// lstatNolog lstats a file with no test logging.
private static (FileInfo, error) lstatNolog(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    ref fileStat fs = ref heap(out ptr<fileStat> _addr_fs);
    var err = ignoringEINTR(() => {
        return syscall.Lstat(name, _addr_fs.sys);
    });
    if (err != null) {
        return (null, error.As(addr(new PathError(Op:"lstat",Path:name,Err:err))!)!);
    }
    fillFileStatFromSys(_addr_fs, name);
    return (_addr_fs, error.As(null!)!);

}

} // end os_package
