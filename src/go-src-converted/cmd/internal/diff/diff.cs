// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package diff implements a Diff function that compare two inputs
// using the 'diff' tool.
// package diff -- go2cs converted at 2020 October 08 04:33:18 UTC
// import "cmd/internal/diff" ==> using diff = go.cmd.@internal.diff_package
// Original source: C:\Go\src\cmd\internal\diff\diff.go
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class diff_package
    {
        // Returns diff of two arrays of bytes in diff tool format.
        public static (slice<byte>, error) Diff(@string prefix, slice<byte> b1, slice<byte> b2) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (f1, err) = writeTempFile(prefix, b1);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            defer(os.Remove(f1));

            var (f2, err) = writeTempFile(prefix, b2);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            defer(os.Remove(f2));

            @string cmd = "diff";
            if (runtime.GOOS == "plan9")
            {
                cmd = "/bin/ape/diff";
            }
            var (data, err) = exec.Command(cmd, "-u", f1, f2).CombinedOutput();
            if (len(data) > 0L)
            { 
                // diff exits with a non-zero status when the files don't match.
                // Ignore that failure as long as we get output.
                err = null;

            }
            return (data, error.As(err)!);

        });

        private static (@string, error) writeTempFile(@string prefix, slice<byte> data)
        {
            @string _p0 = default;
            error _p0 = default!;

            var (file, err) = ioutil.TempFile("", prefix);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            _, err = file.Write(data);
            {
                var err1 = file.Close();

                if (err == null)
                {
                    err = err1;
                }

            }

            if (err != null)
            {
                os.Remove(file.Name());
                return ("", error.As(err)!);
            }

            return (file.Name(), error.As(null!)!);

        }
    }
}}}
