// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:50 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\stat_plan9.go
using syscall = go.syscall_package;
using time = go.time_package;

namespace go;

public static partial class os_package {

private static readonly nint bitSize16 = 2;



private static ptr<fileStat> fileInfoFromStat(ptr<syscall.Dir> _addr_d) {
    ref syscall.Dir d = ref _addr_d.val;

    ptr<fileStat> fs = addr(new fileStat(name:d.Name,size:d.Length,modTime:time.Unix(int64(d.Mtime),0),sys:d,));
    fs.mode = FileMode(d.Mode & 0777);
    if (d.Mode & syscall.DMDIR != 0) {
        fs.mode |= ModeDir;
    }
    if (d.Mode & syscall.DMAPPEND != 0) {
        fs.mode |= ModeAppend;
    }
    if (d.Mode & syscall.DMEXCL != 0) {
        fs.mode |= ModeExclusive;
    }
    if (d.Mode & syscall.DMTMP != 0) {
        fs.mode |= ModeTemporary;
    }
    if (d.Type != 'M') {
        fs.mode |= ModeDevice;
    }
    if (d.Type == 'c') {
        fs.mode |= ModeCharDevice;
    }
    return _addr_fs!;

}

// arg is an open *File or a path string.
private static (ptr<syscall.Dir>, error) dirstat(object arg) => func((_, panic, _) => {
    ptr<syscall.Dir> _p0 = default!;
    error _p0 = default!;

    @string name = default;
    error err = default!;

    var size = syscall.STATFIXLEN + 16 * 4;

    for (nint i = 0; i < 2; i++) {
        var buf = make_slice<byte>(bitSize16 + size);

        nint n = default;
        switch (arg.type()) {
            case ptr<File> a:
                name = a.name;
                n, err = syscall.Fstat(a.fd, buf);
                break;
            case @string a:
                name = a;
                n, err = syscall.Stat(a, buf);
                break;
            default:
            {
                var a = arg.type();
                panic("phase error in dirstat");
                break;
            }

        }

        if (n < bitSize16) {
            return (_addr_null!, error.As(addr(new PathError(Op:"stat",Path:name,Err:err))!)!);
        }
        size = int(uint16(buf[0]) | uint16(buf[1]) << 8); 

        // If the stat message is larger than our buffer we will
        // go around the loop and allocate one that is big enough.
        if (size <= n) {
            var (d, err) = syscall.UnmarshalDir(buf[..(int)n]);
            if (err != null) {
                return (_addr_null!, error.As(addr(new PathError(Op:"stat",Path:name,Err:err))!)!);
            }
            return (_addr_d!, error.As(null!)!);
        }
    }

    if (err == null) {
        err = error.As(syscall.ErrBadStat)!;
    }
    return (_addr_null!, error.As(addr(new PathError(Op:"stat",Path:name,Err:err))!)!);

});

// statNolog implements Stat for Plan 9.
private static (FileInfo, error) statNolog(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    var (d, err) = dirstat(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (fileInfoFromStat(_addr_d), error.As(null!)!);

}

// lstatNolog implements Lstat for Plan 9.
private static (FileInfo, error) lstatNolog(@string name) {
    FileInfo _p0 = default;
    error _p0 = default!;

    return statNolog(name);
}

// For testing.
private static time.Time atime(FileInfo fi) {
    return time.Unix(int64(fi.Sys()._<ptr<syscall.Dir>>().Atime), 0);
}

} // end os_package
