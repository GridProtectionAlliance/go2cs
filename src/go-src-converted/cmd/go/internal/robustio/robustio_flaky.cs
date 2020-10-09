// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows darwin

// package robustio -- go2cs converted at 2020 October 09 05:45:43 UTC
// import "cmd/go/internal/robustio" ==> using robustio = go.cmd.go.@internal.robustio_package
// Original source: C:\Go\src\cmd\go\internal\robustio\robustio_flaky.go
using errors = go.errors_package;
using ioutil = go.io.ioutil_package;
using rand = go.math.rand_package;
using os = go.os_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class robustio_package
    {
        private static readonly long arbitraryTimeout = (long)2000L * time.Millisecond;

        // retry retries ephemeral errors from f up to an arbitrary timeout
        // to work around filesystem flakiness on Windows and Darwin.


        // retry retries ephemeral errors from f up to an arbitrary timeout
        // to work around filesystem flakiness on Windows and Darwin.
        private static error retry(Func<(error, bool)> f)
        {
            error bestErr = default!;            syscall.Errno lowestErrno = default;            time.Time start = default;            time.Duration nextSleep = 1L * time.Millisecond;
            while (true)
            {
                var (err, mayRetry) = f();
                if (err == null || !mayRetry)
                {
                    return error.As(err)!;
                }

                ref syscall.Errno errno = ref heap(out ptr<syscall.Errno> _addr_errno);
                if (errors.As(err, _addr_errno) && (lowestErrno == 0L || errno < lowestErrno))
                {
                    bestErr = error.As(err)!;
                    lowestErrno = errno;
                }
                else if (bestErr == null)
                {
                    bestErr = error.As(err)!;
                }

                if (start.IsZero())
                {
                    start = time.Now();
                }                {
                    var d = time.Since(start) + nextSleep;


                    else if (d >= arbitraryTimeout)
                    {
                        break;
                    }

                }

                time.Sleep(nextSleep);
                nextSleep += time.Duration(rand.Int63n(int64(nextSleep)));

            }


            return error.As(bestErr)!;

        }

        // rename is like os.Rename, but retries ephemeral errors.
        //
        // On Windows it wraps os.Rename, which (as of 2019-06-04) uses MoveFileEx with
        // MOVEFILE_REPLACE_EXISTING.
        //
        // Windows also provides a different system call, ReplaceFile,
        // that provides similar semantics, but perhaps preserves more metadata. (The
        // documentation on the differences between the two is very sparse.)
        //
        // Empirical error rates with MoveFileEx are lower under modest concurrency, so
        // for now we're sticking with what the os package already provides.
        private static error rename(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(retry(() =>
            {
                err = os.Rename(oldpath, newpath);
                return (error.As(err)!, isEphemeralError(err));
            }))!;

        }

        // readFile is like ioutil.ReadFile, but retries ephemeral errors.
        private static (slice<byte>, error) readFile(@string filename)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            slice<byte> b = default;
            var err = retry(() =>
            {
                b, err = ioutil.ReadFile(filename); 

                // Unlike in rename, we do not retry errFileNotFound here: it can occur
                // as a spurious error, but the file may also genuinely not exist, so the
                // increase in robustness is probably not worth the extra latency.
                return (err, error.As(isEphemeralError(err) && !errors.Is(err, errFileNotFound))!);

            });
            return (b, error.As(err)!);

        }

        private static error removeAll(@string path)
        {
            return error.As(retry(() =>
            {
                err = os.RemoveAll(path);
                return (error.As(err)!, isEphemeralError(err));
            }))!;

        }
    }
}}}}
