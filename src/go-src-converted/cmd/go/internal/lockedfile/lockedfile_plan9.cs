// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9
// +build plan9

// package lockedfile -- go2cs converted at 2022 March 06 23:17:01 UTC
// import "cmd/go/internal/lockedfile" ==> using lockedfile = go.cmd.go.@internal.lockedfile_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\lockedfile_plan9.go
using fs = go.io.fs_package;
using rand = go.math.rand_package;
using os = go.os_package;
using strings = go.strings_package;
using time = go.time_package;

namespace go.cmd.go.@internal;

public static partial class lockedfile_package {

    // Opening an exclusive-use file returns an error.
    // The expected error strings are:
    //
    //  - "open/create -- file is locked" (cwfs, kfs)
    //  - "exclusive lock" (fossil)
    //  - "exclusive use file already open" (ramfs)
private static array<@string> lockedErrStrings = new array<@string>(new @string[] { "file is locked", "exclusive lock", "exclusive use file already open" });

// Even though plan9 doesn't support the Lock/RLock/Unlock functions to
// manipulate already-open files, IsLocked is still meaningful: os.OpenFile
// itself may return errors that indicate that a file with the ModeExclusive bit
// set is already open.
private static bool isLocked(error err) {
    var s = err.Error();

    foreach (var (_, frag) in lockedErrStrings) {
        if (strings.Contains(s, frag)) {
            return true;
        }
    }    return false;

}

private static (ptr<os.File>, error) openFile(@string name, nint flag, fs.FileMode perm) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;
 
    // Plan 9 uses a mode bit instead of explicit lock/unlock syscalls.
    //
    // Per http://man.cat-v.org/plan_9/5/stat: “Exclusive use files may be open
    // for I/O by only one fid at a time across all clients of the server. If a
    // second open is attempted, it draws an error.”
    //
    // So we can try to open a locked file, but if it fails we're on our own to
    // figure out when it becomes available. We'll use exponential backoff with
    // some jitter and an arbitrary limit of 500ms.

    // If the file was unpacked or created by some other program, it might not
    // have the ModeExclusive bit set. Set it before we call OpenFile, so that we
    // can be confident that a successful OpenFile implies exclusive use.
    {
        var (fi, err) = os.Stat(name);

        if (err == null) {
            if (fi.Mode() & fs.ModeExclusive == 0) {
                {
                    var err = os.Chmod(name, fi.Mode() | fs.ModeExclusive);

                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }

                }

            }

        }
        else if (!os.IsNotExist(err)) {
            return (_addr_null!, error.As(err)!);
        }

    }


    nint nextSleep = 1 * time.Millisecond;
    const nint maxSleep = 500 * time.Millisecond;

    while (true) {
        var (f, err) = os.OpenFile(name, flag, perm | fs.ModeExclusive);
        if (err == null) {
            return (_addr_f!, error.As(null!)!);
        }
        if (!isLocked(err)) {
            return (_addr_null!, error.As(err)!);
        }
        time.Sleep(nextSleep);

        nextSleep += nextSleep;
        if (nextSleep > maxSleep) {
            nextSleep = maxSleep;
        }
        nextSleep += time.Duration((0.1F * rand.Float64() - 0.05F) * float64(nextSleep));

    }

}

private static error closeFile(ptr<os.File> _addr_f) {
    ref os.File f = ref _addr_f.val;

    return error.As(f.Close())!;
}

} // end lockedfile_package
