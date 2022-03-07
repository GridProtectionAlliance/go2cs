// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lockedfile -- go2cs converted at 2022 March 06 23:17:01 UTC
// import "cmd/go/internal/lockedfile" ==> using lockedfile = go.cmd.go.@internal.lockedfile_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\mutex.go
using fmt = go.fmt_package;
using os = go.os_package;
using sync = go.sync_package;
using System;


namespace go.cmd.go.@internal;

public static partial class lockedfile_package {

    // A Mutex provides mutual exclusion within and across processes by locking a
    // well-known file. Such a file generally guards some other part of the
    // filesystem: for example, a Mutex file in a directory might guard access to
    // the entire tree rooted in that directory.
    //
    // Mutex does not implement sync.Locker: unlike a sync.Mutex, a lockedfile.Mutex
    // can fail to lock (e.g. if there is a permission error in the filesystem).
    //
    // Like a sync.Mutex, a Mutex may be included as a field of a larger struct but
    // must not be copied after first use. The Path field must be set before first
    // use and must not be change thereafter.
public partial struct Mutex {
    public @string Path; // The path to the well-known lock file. Must be non-empty.
    public sync.Mutex mu; // A redundant mutex. The race detector doesn't know about file locking, so in tests we may need to lock something that it understands.
}

// MutexAt returns a new Mutex with Path set to the given non-empty path.
public static ptr<Mutex> MutexAt(@string path) => func((_, panic, _) => {
    if (path == "") {
        panic("lockedfile.MutexAt: path must be non-empty");
    }
    return addr(new Mutex(Path:path));

});

private static @string String(this ptr<Mutex> _addr_mu) {
    ref Mutex mu = ref _addr_mu.val;

    return fmt.Sprintf("lockedfile.Mutex(%s)", mu.Path);
}

// Lock attempts to lock the Mutex.
//
// If successful, Lock returns a non-nil unlock function: it is provided as a
// return-value instead of a separate method to remind the caller to check the
// accompanying error. (See https://golang.org/issue/20803.)
private static (Action, error) Lock(this ptr<Mutex> _addr_mu) => func((_, panic, _) => {
    Action unlock = default;
    error err = default!;
    ref Mutex mu = ref _addr_mu.val;

    if (mu.Path == "") {
        panic("lockedfile.Mutex: missing Path during Lock");
    }
    var (f, err) = OpenFile(mu.Path, os.O_RDWR | os.O_CREATE, 0666);
    if (err != null) {
        return (null, error.As(err)!);
    }
    mu.mu.Lock();

    return (() => {
        mu.mu.Unlock();
        f.Close();
    }, error.As(null!)!);

});

} // end lockedfile_package
