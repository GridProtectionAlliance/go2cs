// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package syscall -- go2cs converted at 2022 March 06 22:26:37 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\fs_js.go
using errors = go.errors_package;
using sync = go.sync_package;
using js = go.syscall.js_package;
using System;


namespace go;

public static partial class syscall_package {

    // Provided by package runtime.
private static (long, int) now();

private static var jsProcess = js.Global().Get("process");
private static var jsFS = js.Global().Get("fs");
private static var constants = jsFS.Get("constants");

private static var uint8Array = js.Global().Get("Uint8Array");

private static var nodeWRONLY = constants.Get("O_WRONLY").Int();private static var nodeRDWR = constants.Get("O_RDWR").Int();private static var nodeCREATE = constants.Get("O_CREAT").Int();private static var nodeTRUNC = constants.Get("O_TRUNC").Int();private static var nodeAPPEND = constants.Get("O_APPEND").Int();private static var nodeEXCL = constants.Get("O_EXCL").Int();

private partial struct jsFile {
    public @string path;
    public slice<@string> entries;
    public nint dirIdx; // entries[:dirIdx] have already been returned in ReadDirent
    public long pos;
    public bool seeked;
}

private static sync.Mutex filesMu = default;
private static map files = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, ptr<jsFile>>{0:{},1:{},2:{},};

private static (ptr<jsFile>, error) fdToFile(nint fd) {
    ptr<jsFile> _p0 = default!;
    error _p0 = default!;

    filesMu.Lock();
    var (f, ok) = files[fd];
    filesMu.Unlock();
    if (!ok) {>>MARKER:FUNCTION_now_BLOCK_PREFIX<<
        return (_addr_null!, error.As(EBADF)!);
    }
    return (_addr_f!, error.As(null!)!);

}

public static (nint, error) Open(@string path, nint openmode, uint perm) {
    nint _p0 = default;
    error _p0 = default!;

    {
        var err = checkPath(path);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }


    nint flags = 0;
    if (openmode & O_WRONLY != 0) {
        flags |= nodeWRONLY;
    }
    if (openmode & O_RDWR != 0) {
        flags |= nodeRDWR;
    }
    if (openmode & O_CREATE != 0) {
        flags |= nodeCREATE;
    }
    if (openmode & O_TRUNC != 0) {
        flags |= nodeTRUNC;
    }
    if (openmode & O_APPEND != 0) {
        flags |= nodeAPPEND;
    }
    if (openmode & O_EXCL != 0) {
        flags |= nodeEXCL;
    }
    if (openmode & O_SYNC != 0) {
        return (0, error.As(errors.New("syscall.Open: O_SYNC is not supported by js/wasm"))!);
    }
    var (jsFD, err) = fsCall("open", path, flags, perm);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var fd = jsFD.Int();

    slice<@string> entries = default;
    {
        var (stat, err) = fsCall("fstat", fd);

        if (err == null && stat.Call("isDirectory").Bool()) {
            var (dir, err) = fsCall("readdir", path);
            if (err != null) {
                return (0, error.As(err)!);
            }
            entries = make_slice<@string>(dir.Length());
            foreach (var (i) in entries) {
                entries[i] = dir.Index(i).String();
            }
        }
    }


    if (path[0] != '/') {
        var cwd = jsProcess.Call("cwd").String();
        path = cwd + "/" + path;
    }
    ptr<jsFile> f = addr(new jsFile(path:path,entries:entries,));
    filesMu.Lock();
    files[fd] = f;
    filesMu.Unlock();
    return (fd, error.As(null!)!);

}

public static error Close(nint fd) {
    filesMu.Lock();
    delete(files, fd);
    filesMu.Unlock();
    var (_, err) = fsCall("close", fd);
    return error.As(err)!;
}

public static void CloseOnExec(nint fd) { 
    // nothing to do - no exec
}

public static error Mkdir(@string path, uint perm) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("mkdir", path, perm);
    return error.As(err)!;

}

public static (nint, error) ReadDirent(nint fd, slice<byte> buf) {
    nint _p0 = default;
    error _p0 = default!;

    var (f, err) = fdToFile(fd);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (f.entries == null) {
        return (0, error.As(EINVAL)!);
    }
    nint n = 0;
    while (f.dirIdx < len(f.entries)) {
        var entry = f.entries[f.dirIdx];
        nint l = 2 + len(entry);
        if (l > len(buf)) {
            break;
        }
        buf[0] = byte(l);
        buf[1] = byte(l >> 8);
        copy(buf[(int)2..], entry);
        buf = buf[(int)l..];
        n += l;
        f.dirIdx++;

    }

    return (n, error.As(null!)!);

}

private static void setStat(ptr<Stat_t> _addr_st, js.Value jsSt) {
    ref Stat_t st = ref _addr_st.val;

    st.Dev = int64(jsSt.Get("dev").Int());
    st.Ino = uint64(jsSt.Get("ino").Int());
    st.Mode = uint32(jsSt.Get("mode").Int());
    st.Nlink = uint32(jsSt.Get("nlink").Int());
    st.Uid = uint32(jsSt.Get("uid").Int());
    st.Gid = uint32(jsSt.Get("gid").Int());
    st.Rdev = int64(jsSt.Get("rdev").Int());
    st.Size = int64(jsSt.Get("size").Int());
    st.Blksize = int32(jsSt.Get("blksize").Int());
    st.Blocks = int32(jsSt.Get("blocks").Int());
    var atime = int64(jsSt.Get("atimeMs").Int());
    st.Atime = atime / 1000;
    st.AtimeNsec = (atime % 1000) * 1000000;
    var mtime = int64(jsSt.Get("mtimeMs").Int());
    st.Mtime = mtime / 1000;
    st.MtimeNsec = (mtime % 1000) * 1000000;
    var ctime = int64(jsSt.Get("ctimeMs").Int());
    st.Ctime = ctime / 1000;
    st.CtimeNsec = (ctime % 1000) * 1000000;
}

public static error Stat(@string path, ptr<Stat_t> _addr_st) {
    ref Stat_t st = ref _addr_st.val;

    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (jsSt, err) = fsCall("stat", path);
    if (err != null) {
        return error.As(err)!;
    }
    setStat(_addr_st, jsSt);
    return error.As(null!)!;

}

public static error Lstat(@string path, ptr<Stat_t> _addr_st) {
    ref Stat_t st = ref _addr_st.val;

    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (jsSt, err) = fsCall("lstat", path);
    if (err != null) {
        return error.As(err)!;
    }
    setStat(_addr_st, jsSt);
    return error.As(null!)!;

}

public static error Fstat(nint fd, ptr<Stat_t> _addr_st) {
    ref Stat_t st = ref _addr_st.val;

    var (jsSt, err) = fsCall("fstat", fd);
    if (err != null) {
        return error.As(err)!;
    }
    setStat(_addr_st, jsSt);
    return error.As(null!)!;

}

public static error Unlink(@string path) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("unlink", path);
    return error.As(err)!;

}

public static error Rmdir(@string path) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("rmdir", path);
    return error.As(err)!;

}

public static error Chmod(@string path, uint mode) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("chmod", path, mode);
    return error.As(err)!;

}

public static error Fchmod(nint fd, uint mode) {
    var (_, err) = fsCall("fchmod", fd, mode);
    return error.As(err)!;
}

public static error Chown(@string path, nint uid, nint gid) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("chown", path, uint32(uid), uint32(gid));
    return error.As(err)!;

}

public static error Fchown(nint fd, nint uid, nint gid) {
    var (_, err) = fsCall("fchown", fd, uint32(uid), uint32(gid));
    return error.As(err)!;
}

public static error Lchown(@string path, nint uid, nint gid) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    if (jsFS.Get("lchown").IsUndefined()) { 
        // fs.lchown is unavailable on Linux until Node.js 10.6.0
        // TODO(neelance): remove when we require at least this Node.js version
        return error.As(ENOSYS)!;

    }
    var (_, err) = fsCall("lchown", path, uint32(uid), uint32(gid));
    return error.As(err)!;

}

public static error UtimesNano(@string path, slice<Timespec> ts) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    var atime = ts[0].Sec;
    var mtime = ts[1].Sec;
    var (_, err) = fsCall("utimes", path, atime, mtime);
    return error.As(err)!;

}

public static error Rename(@string from, @string to) {
    {
        var err__prev1 = err;

        var err = checkPath(from);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = checkPath(to);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var (_, err) = fsCall("rename", from, to);
    return error.As(err)!;

}

public static error Truncate(@string path, long length) {
    {
        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
    }

    var (_, err) = fsCall("truncate", path, length);
    return error.As(err)!;

}

public static error Ftruncate(nint fd, long length) {
    var (_, err) = fsCall("ftruncate", fd, length);
    return error.As(err)!;
}

public static (nint, error) Getcwd(slice<byte> buf) => func((defer, _, _) => {
    nint n = default;
    error err = default!;

    defer(recoverErr(_addr_err));
    var cwd = jsProcess.Call("cwd").String();
    n = copy(buf, cwd);
    return ;
});

public static error Chdir(@string path) => func((defer, _, _) => {
    error err = default!;

    {
        ref var err = ref heap(checkPath(path), out ptr<var> _addr_err);

        if (err != null) {
            return error.As(err)!;
        }
    }

    defer(recoverErr(_addr_err));
    jsProcess.Call("chdir", path);
    return ;

});

public static error Fchdir(nint fd) {
    var (f, err) = fdToFile(fd);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(Chdir(f.path))!;

}

public static (nint, error) Readlink(@string path, slice<byte> buf) {
    nint n = default;
    error err = default!;

    {
        var err = checkPath(path);

        if (err != null) {
            return (0, error.As(err)!);
        }
    }

    var (dst, err) = fsCall("readlink", path);
    if (err != null) {
        return (0, error.As(err)!);
    }
    n = copy(buf, dst.String());
    return (n, error.As(null!)!);

}

public static error Link(@string path, @string link) {
    {
        var err__prev1 = err;

        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = checkPath(link);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var (_, err) = fsCall("link", path, link);
    return error.As(err)!;

}

public static error Symlink(@string path, @string link) {
    {
        var err__prev1 = err;

        var err = checkPath(path);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = checkPath(link);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var (_, err) = fsCall("symlink", path, link);
    return error.As(err)!;

}

public static error Fsync(nint fd) {
    var (_, err) = fsCall("fsync", fd);
    return error.As(err)!;
}

public static (nint, error) Read(nint fd, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;

    var (f, err) = fdToFile(fd);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (f.seeked) {
        var (n, err) = Pread(fd, b, f.pos);
        f.pos += int64(n);
        return (n, error.As(err)!);
    }
    var buf = uint8Array.New(len(b));
    (n, err) = fsCall("read", fd, buf, 0, len(b), null);
    if (err != null) {
        return (0, error.As(err)!);
    }
    js.CopyBytesToGo(b, buf);

    var n2 = n.Int();
    f.pos += int64(n2);
    return (n2, error.As(err)!);

}

public static (nint, error) Write(nint fd, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;

    var (f, err) = fdToFile(fd);
    if (err != null) {
        return (0, error.As(err)!);
    }
    if (f.seeked) {
        var (n, err) = Pwrite(fd, b, f.pos);
        f.pos += int64(n);
        return (n, error.As(err)!);
    }
    if (faketime && (fd == 1 || fd == 2)) {
        var n = faketimeWrite(fd, b);
        if (n < 0) {
            return (0, error.As(errnoErr(Errno(-n)))!);
        }
        return (n, error.As(null!)!);

    }
    var buf = uint8Array.New(len(b));
    js.CopyBytesToJS(buf, b);
    (n, err) = fsCall("write", fd, buf, 0, len(b), null);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var n2 = n.Int();
    f.pos += int64(n2);
    return (n2, error.As(err)!);

}

public static (nint, error) Pread(nint fd, slice<byte> b, long offset) {
    nint _p0 = default;
    error _p0 = default!;

    var buf = uint8Array.New(len(b));
    var (n, err) = fsCall("read", fd, buf, 0, len(b), offset);
    if (err != null) {
        return (0, error.As(err)!);
    }
    js.CopyBytesToGo(b, buf);
    return (n.Int(), error.As(null!)!);

}

public static (nint, error) Pwrite(nint fd, slice<byte> b, long offset) {
    nint _p0 = default;
    error _p0 = default!;

    var buf = uint8Array.New(len(b));
    js.CopyBytesToJS(buf, b);
    var (n, err) = fsCall("write", fd, buf, 0, len(b), offset);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (n.Int(), error.As(null!)!);

}

public static (long, error) Seek(nint fd, long offset, nint whence) {
    long _p0 = default;
    error _p0 = default!;

    var (f, err) = fdToFile(fd);
    if (err != null) {
        return (0, error.As(err)!);
    }
    long newPos = default;
    switch (whence) {
        case 0: 
            newPos = offset;
            break;
        case 1: 
            newPos = f.pos + offset;
            break;
        case 2: 
            ref Stat_t st = ref heap(out ptr<Stat_t> _addr_st);
            {
                var err = Fstat(fd, _addr_st);

                if (err != null) {
                    return (0, error.As(err)!);
                }

            }

            newPos = st.Size + offset;

            break;
        default: 
            return (0, error.As(errnoErr(EINVAL))!);
            break;
    }

    if (newPos < 0) {
        return (0, error.As(errnoErr(EINVAL))!);
    }
    f.seeked = true;
    f.dirIdx = 0; // Reset directory read position. See issue 35767.
    f.pos = newPos;
    return (newPos, error.As(null!)!);

}

public static (nint, error) Dup(nint fd) {
    nint _p0 = default;
    error _p0 = default!;

    return (0, error.As(ENOSYS)!);
}

public static error Dup2(nint fd, nint newfd) {
    return error.As(ENOSYS)!;
}

public static error Pipe(slice<nint> fd) {
    return error.As(ENOSYS)!;
}

private static (js.Value, error) fsCall(@string name, params object[] args) => func((defer, _, _) => {
    js.Value _p0 = default;
    error _p0 = default!;
    args = args.Clone();

    private partial struct callResult {
        public js.Value val;
        public error err;
    }

    var c = make_channel<callResult>(1);
    var f = js.FuncOf((@this, args) => {
        callResult res = default;

        if (len(args) >= 1) { // on Node.js 8, fs.utimes calls the callback without any arguments
            {
                var jsErr = args[0];

                if (!jsErr.IsNull()) {
                    res.err = mapJSError(jsErr);
                }

            }

        }
        res.val = js.Undefined();
        if (len(args) >= 2) {
            res.val = args[1];
        }
        c.Send(res);
        return null;

    });
    defer(f.Release());
    jsFS.Call(name, append(args, f));
    res = c.Receive();
    return (res.val, error.As(res.err)!);

});

// checkPath checks that the path is not empty and that it contains no null characters.
private static error checkPath(@string path) {
    if (path == "") {
        return error.As(EINVAL)!;
    }
    for (nint i = 0; i < len(path); i++) {
        if (path[i] == '\x00') {
            return error.As(EINVAL)!;
        }
    }
    return error.As(null!)!;

}

private static void recoverErr(ptr<error> _addr_errPtr) => func((_, panic, _) => {
    ref error errPtr = ref _addr_errPtr.val;

    {
        var err = recover();

        if (err != null) {
            js.Error (jsErr, ok) = err._<js.Error>();
            if (!ok) {
                panic(err);
            }
            errPtr = error.As(mapJSError(jsErr.Value))!;
        }
    }

});

// mapJSError maps an error given by Node.js to the appropriate Go error
private static error mapJSError(js.Value jsErr) => func((_, panic, _) => {
    var (errno, ok) = errnoByCode[jsErr.Get("code").String()];
    if (!ok) {
        panic(jsErr);
    }
    return error.As(errnoErr(Errno(errno)))!;

});

} // end syscall_package
