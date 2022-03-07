// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lockedfile creates and manipulates files whose contents should only
// change atomically.
// package lockedfile -- go2cs converted at 2022 March 06 23:16:36 UTC
// import "cmd/go/internal/lockedfile" ==> using lockedfile = go.cmd.go.@internal.lockedfile_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\lockedfile.go
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using runtime = go.runtime_package;
using System;


namespace go.cmd.go.@internal;

public static partial class lockedfile_package {

    // A File is a locked *os.File.
    //
    // Closing the file releases the lock.
    //
    // If the program exits while a file is locked, the operating system releases
    // the lock but may not do so promptly: callers must ensure that all locked
    // files are closed before exiting.
public partial struct File {
    public ref osFile osFile => ref osFile_val;
    public bool closed;
}

// osFile embeds a *os.File while keeping the pointer itself unexported.
// (When we close a File, it must be the same file descriptor that we opened!)
private partial struct osFile {
    public ref ptr<os.File> File> => ref File>_ptr;
}

// OpenFile is like os.OpenFile, but returns a locked file.
// If flag includes os.O_WRONLY or os.O_RDWR, the file is write-locked;
// otherwise, it is read-locked.
public static (ptr<File>, error) OpenFile(@string name, nint flag, fs.FileMode perm) => func((_, panic, _) => {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    ptr<File> f = @new<File>();    error err = default!;
    f.osFile.File, err = openFile(name, flag, perm);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    runtime.SetFinalizer(f, f => {
        panic(fmt.Sprintf("lockedfile.File %s became unreachable without a call to Close", f.Name()));
    });

    return (_addr_f!, error.As(null!)!);

});

// Open is like os.Open, but returns a read-locked file.
public static (ptr<File>, error) Open(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(name, os.O_RDONLY, 0)!;
}

// Create is like os.Create, but returns a write-locked file.
public static (ptr<File>, error) Create(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_TRUNC, 0666)!;
}

// Edit creates the named file with mode 0666 (before umask),
// but does not truncate existing contents.
//
// If Edit succeeds, methods on the returned File can be used for I/O.
// The associated file descriptor has mode O_RDWR and the file is write-locked.
public static (ptr<File>, error) Edit(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(name, os.O_RDWR | os.O_CREATE, 0666)!;
}

// Close unlocks and closes the underlying file.
//
// Close may be called multiple times; all calls after the first will return a
// non-nil error.
private static error Close(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    if (f.closed) {
        return error.As(addr(new fs.PathError(Op:"close",Path:f.Name(),Err:fs.ErrClosed,))!)!;
    }
    f.closed = true;

    var err = closeFile(f.osFile.File);
    runtime.SetFinalizer(f, null);
    return error.As(err)!;

}

// Read opens the named file with a read-lock and returns its contents.
public static (slice<byte>, error) Read(@string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f, err) = Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(f.Close());

    return io.ReadAll(f);

});

// Write opens the named file (creating it with the given permissions if needed),
// then write-locks it and overwrites it with the given content.
public static error Write(@string name, io.Reader content, fs.FileMode perm) {
    error err = default!;

    var (f, err) = OpenFile(name, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, perm);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = io.Copy(f, content);
    {
        var closeErr = f.Close();

        if (err == null) {
            err = closeErr;
        }
    }

    return error.As(err)!;

}

// Transform invokes t with the result of reading the named file, with its lock
// still held.
//
// If t returns a nil error, Transform then writes the returned contents back to
// the file, making a best effort to preserve existing contents on error.
//
// t must not modify the slice passed to it.
public static error Transform(@string name, Func<slice<byte>, (slice<byte>, error)> t) => func((defer, _, _) => {
    error err = default!;

    var (f, err) = Edit(name);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());

    var (old, err) = io.ReadAll(f);
    if (err != null) {
        return error.As(err)!;
    }
    var (new, err) = t(old);
    if (err != null) {
        return error.As(err)!;
    }
    if (len(new) > len(old)) { 
        // The overall file size is increasing, so write the tail first: if we're
        // about to run out of space on the disk, we would rather detect that
        // failure before we have overwritten the original contents.
        {
            var (_, err) = f.WriteAt(new[(int)len(old)..], int64(len(old)));

            if (err != null) { 
                // Make a best effort to remove the incomplete tail.
                f.Truncate(int64(len(old)));
                return error.As(err)!;

            }

        }

    }
    defer(() => {
        if (err != null) {
            {
                (_, err) = f.WriteAt(old, 0);

                if (err == null) {
                    f.Truncate(int64(len(old)));
                }

            }

        }
    }());

    if (len(new) >= len(old)) {
        {
            (_, err) = f.WriteAt(new[..(int)len(old)], 0);

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    else
 {
        {
            (_, err) = f.WriteAt(new, 0);

            if (err != null) {
                return error.As(err)!;
            } 
            // The overall file size is decreasing, so shrink the file to its final size
            // after writing. We do this after writing (instead of before) so that if
            // the write fails, enough filesystem space will likely still be reserved
            // to contain the previous contents.

        } 
        // The overall file size is decreasing, so shrink the file to its final size
        // after writing. We do this after writing (instead of before) so that if
        // the write fails, enough filesystem space will likely still be reserved
        // to contain the previous contents.
        {
            var err = f.Truncate(int64(len(new)));

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    return error.As(null!)!;

});

} // end lockedfile_package
