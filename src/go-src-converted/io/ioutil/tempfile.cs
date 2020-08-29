// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ioutil -- go2cs converted at 2020 August 29 08:22:23 UTC
// import "io/ioutil" ==> using ioutil = go.io.ioutil_package
// Original source: C:\Go\src\io\ioutil\tempfile.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace io
{
    public static partial class ioutil_package
    {
        // Random number state.
        // We generate random temporary file names so that there's a good
        // chance the file doesn't exist yet - keeps the number of tries in
        // TempFile to a minimum.
        private static uint rand = default;
        private static sync.Mutex randmu = default;

        private static uint reseed()
        {
            return uint32(time.Now().UnixNano() + int64(os.Getpid()));
        }

        private static @string nextSuffix()
        {
            randmu.Lock();
            var r = rand;
            if (r == 0L)
            {
                r = reseed();
            }
            r = r * 1664525L + 1013904223L; // constants from Numerical Recipes
            rand = r;
            randmu.Unlock();
            return strconv.Itoa(int(1e9F + r % 1e9F))[1L..];
        }

        // TempFile creates a new temporary file in the directory dir
        // with a name beginning with prefix, opens the file for reading
        // and writing, and returns the resulting *os.File.
        // If dir is the empty string, TempFile uses the default directory
        // for temporary files (see os.TempDir).
        // Multiple programs calling TempFile simultaneously
        // will not choose the same file. The caller can use f.Name()
        // to find the pathname of the file. It is the caller's responsibility
        // to remove the file when no longer needed.
        public static (ref os.File, error) TempFile(@string dir, @string prefix)
        {
            if (dir == "")
            {
                dir = os.TempDir();
            }
            long nconflict = 0L;
            for (long i = 0L; i < 10000L; i++)
            {
                var name = filepath.Join(dir, prefix + nextSuffix());
                f, err = os.OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_EXCL, 0600L);
                if (os.IsExist(err))
                {
                    nconflict++;

                    if (nconflict > 10L)
                    {
                        randmu.Lock();
                        rand = reseed();
                        randmu.Unlock();
                    }
                    continue;
                }
                break;
            }

            return;
        }

        // TempDir creates a new temporary directory in the directory dir
        // with a name beginning with prefix and returns the path of the
        // new directory. If dir is the empty string, TempDir uses the
        // default directory for temporary files (see os.TempDir).
        // Multiple programs calling TempDir simultaneously
        // will not choose the same directory. It is the caller's responsibility
        // to remove the directory when no longer needed.
        public static (@string, error) TempDir(@string dir, @string prefix)
        {
            if (dir == "")
            {
                dir = os.TempDir();
            }
            long nconflict = 0L;
            for (long i = 0L; i < 10000L; i++)
            {
                var @try = filepath.Join(dir, prefix + nextSuffix());
                err = os.Mkdir(try, 0700L);
                if (os.IsExist(err))
                {
                    nconflict++;

                    if (nconflict > 10L)
                    {
                        randmu.Lock();
                        rand = reseed();
                        randmu.Unlock();
                    }
                    continue;
                }
                if (os.IsNotExist(err))
                {
                    {
                        var (_, err) = os.Stat(dir);

                        if (os.IsNotExist(err))
                        {
                            return ("", err);
                        }

                    }
                }
                if (err == null)
                {
                    name = try;
                }
                break;
            }

            return;
        }
    }
}}
