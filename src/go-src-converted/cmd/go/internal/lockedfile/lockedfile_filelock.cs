// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9

// package lockedfile -- go2cs converted at 2020 October 08 04:34:18 UTC
// import "cmd/go/internal/lockedfile" ==> using lockedfile = go.cmd.go.@internal.lockedfile_package
// Original source: C:\Go\src\cmd\go\internal\lockedfile\lockedfile_filelock.go
using os = go.os_package;

using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class lockedfile_package
    {
        private static (ptr<os.File>, error) openFile(@string name, long flag, os.FileMode perm)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;
 
            // On BSD systems, we could add the O_SHLOCK or O_EXLOCK flag to the OpenFile
            // call instead of locking separately, but we have to support separate locking
            // calls for Linux and Windows anyway, so it's simpler to use that approach
            // consistently.

            var (f, err) = os.OpenFile(name, flag & ~os.O_TRUNC, perm);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            if (flag & (os.O_RDONLY | os.O_WRONLY | os.O_RDWR) == os.O_WRONLY || flag & (os.O_RDONLY | os.O_WRONLY | os.O_RDWR) == os.O_RDWR) 
                err = filelock.Lock(f);
            else 
                err = filelock.RLock(f);
                        if (err != null)
            {
                f.Close();
                return (_addr_null!, error.As(err)!);
            }
            if (flag & os.O_TRUNC == os.O_TRUNC)
            {
                {
                    var err = f.Truncate(0L);

                    if (err != null)
                    { 
                        // The documentation for os.O_TRUNC says “if possible, truncate file when
                        // opened”, but doesn't define “possible” (golang.org/issue/28699).
                        // We'll treat regular files (and symlinks to regular files) as “possible”
                        // and ignore errors for the rest.
                        {
                            var (fi, statErr) = f.Stat();

                            if (statErr != null || fi.Mode().IsRegular())
                            {
                                filelock.Unlock(f);
                                f.Close();
                                return (_addr_null!, error.As(err)!);
                            }
                        }

                    }
                }

            }
            return (_addr_f!, error.As(null!)!);

        }

        private static error closeFile(ptr<os.File> _addr_f)
        {
            ref os.File f = ref _addr_f.val;
 
            // Since locking syscalls operate on file descriptors, we must unlock the file
            // while the descriptor is still valid — that is, before the file is closed —
            // and avoid unlocking files that are already closed.
            var err = filelock.Unlock(f);

            {
                var closeErr = f.Close();

                if (err == null)
                {
                    err = closeErr;
                }

            }

            return error.As(err)!;

        }
    }
}}}}
