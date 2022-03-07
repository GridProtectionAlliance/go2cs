// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package diff implements a Diff function that compare two inputs
// using the 'diff' tool.
// package diff -- go2cs converted at 2022 March 06 23:15:48 UTC
// import "cmd/internal/diff" ==> using diff = go.cmd.@internal.diff_package
// Original source: C:\Program Files\Go\src\cmd\internal\diff\diff.go
using bytes = go.bytes_package;
using exec = go.@internal.execabs_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using runtime = go.runtime_package;

namespace go.cmd.@internal;

public static partial class diff_package {

    // Returns diff of two arrays of bytes in diff tool format.
public static (slice<byte>, error) Diff(@string prefix, slice<byte> b1, slice<byte> b2) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f1, err) = writeTempFile(prefix, b1);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(os.Remove(f1));

    var (f2, err) = writeTempFile(prefix, b2);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(os.Remove(f2));

    @string cmd = "diff";
    if (runtime.GOOS == "plan9") {
        cmd = "/bin/ape/diff";
    }
    var (data, err) = exec.Command(cmd, "-u", f1, f2).CombinedOutput();
    if (len(data) > 0) { 
        // diff exits with a non-zero status when the files don't match.
        // Ignore that failure as long as we get output.
        err = null;

    }
    if (len(data) > 0 && !bytes.HasPrefix(data, (slice<byte>)"--- ")) {
        var i = bytes.Index(data, (slice<byte>)"\n--- ");
        if (i >= 0 && i < 80 * 10 && bytes.Contains(data[..(int)i], (slice<byte>)"://cygwin.com/")) {
            data = data[(int)i + 1..];
        }
    }
    return (data, error.As(err)!);

});

private static (@string, error) writeTempFile(@string prefix, slice<byte> data) {
    @string _p0 = default;
    error _p0 = default!;

    var (file, err) = ioutil.TempFile("", prefix);
    if (err != null) {
        return ("", error.As(err)!);
    }
    _, err = file.Write(data);
    {
        var err1 = file.Close();

        if (err == null) {
            err = err1;
        }
    }

    if (err != null) {
        os.Remove(file.Name());
        return ("", error.As(err)!);
    }
    return (file.Name(), error.As(null!)!);

}

} // end diff_package
