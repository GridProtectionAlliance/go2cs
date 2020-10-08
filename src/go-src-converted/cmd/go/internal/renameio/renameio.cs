// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package renameio writes files atomically by renaming temporary files.
// package renameio -- go2cs converted at 2020 October 08 04:34:09 UTC
// import "cmd/go/internal/renameio" ==> using renameio = go.cmd.go.@internal.renameio_package
// Original source: C:\Go\src\cmd\go\internal\renameio\renameio.go
using bytes = go.bytes_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;

using robustio = go.cmd.go.@internal.robustio_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class renameio_package
    {
        private static readonly @string patternSuffix = (@string)".tmp";

        // Pattern returns a glob pattern that matches the unrenamed temporary files
        // created when writing to filename.


        // Pattern returns a glob pattern that matches the unrenamed temporary files
        // created when writing to filename.
        public static @string Pattern(@string filename)
        {
            return filepath.Join(filepath.Dir(filename), filepath.Base(filename) + patternSuffix);
        }

        // WriteFile is like ioutil.WriteFile, but first writes data to an arbitrary
        // file in the same directory as filename, then renames it atomically to the
        // final name.
        //
        // That ensures that the final location, if it exists, is always a complete file.
        public static error WriteFile(@string filename, slice<byte> data, os.FileMode perm)
        {
            error err = default!;

            return error.As(WriteToFile(filename, bytes.NewReader(data), perm))!;
        }

        // WriteToFile is a variant of WriteFile that accepts the data as an io.Reader
        // instead of a slice.
        public static error WriteToFile(@string filename, io.Reader data, os.FileMode perm) => func((defer, _, __) =>
        {
            error err = default!;

            var (f, err) = tempFile(filepath.Dir(filename), filepath.Base(filename), perm);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(() =>
            { 
                // Only call os.Remove on f.Name() if we failed to rename it: otherwise,
                // some other process may have created a new file with the same name after
                // that.
                if (err != null)
                {
                    f.Close();
                    os.Remove(f.Name());
                }

            }());

            {
                var err__prev1 = err;

                var (_, err) = io.Copy(f, data);

                if (err != null)
                {
                    return error.As(err)!;
                } 
                // Sync the file before renaming it: otherwise, after a crash the reader may
                // observe a 0-length file instead of the actual contents.
                // See https://golang.org/issue/22397#issuecomment-380831736.

                err = err__prev1;

            } 
            // Sync the file before renaming it: otherwise, after a crash the reader may
            // observe a 0-length file instead of the actual contents.
            // See https://golang.org/issue/22397#issuecomment-380831736.
            {
                var err__prev1 = err;

                var err = f.Sync();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = f.Close();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            return error.As(robustio.Rename(f.Name(), filename))!;

        });

        // ReadFile is like ioutil.ReadFile, but on Windows retries spurious errors that
        // may occur if the file is concurrently replaced.
        //
        // Errors are classified heuristically and retries are bounded, so even this
        // function may occasionally return a spurious error on Windows.
        // If so, the error will likely wrap one of:
        //     - syscall.ERROR_ACCESS_DENIED
        //     - syscall.ERROR_FILE_NOT_FOUND
        //     - internal/syscall/windows.ERROR_SHARING_VIOLATION
        public static (slice<byte>, error) ReadFile(@string filename)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            return robustio.ReadFile(filename);
        }

        // tempFile creates a new temporary file with given permission bits.
        private static (ptr<os.File>, error) tempFile(@string dir, @string prefix, os.FileMode perm)
        {
            ptr<os.File> f = default!;
            error err = default!;

            for (long i = 0L; i < 10000L; i++)
            {
                var name = filepath.Join(dir, prefix + strconv.Itoa(rand.Intn(1000000000L)) + patternSuffix);
                f, err = os.OpenFile(name, os.O_RDWR | os.O_CREATE | os.O_EXCL, perm);
                if (os.IsExist(err))
                {
                    continue;
                }

                break;

            }

            return ;

        }
    }
}}}}
