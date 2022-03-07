// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package obscuretestdata contains functionality used by tests to more easily
// work with testdata that must be obscured primarily due to
// golang.org/issue/34986.
// package obscuretestdata -- go2cs converted at 2022 March 06 23:36:28 UTC
// import "internal/obscuretestdata" ==> using obscuretestdata = go.@internal.obscuretestdata_package
// Original source: C:\Program Files\Go\src\internal\obscuretestdata\obscuretestdata.go
using base64 = go.encoding.base64_package;
using io = go.io_package;
using os = go.os_package;

namespace go.@internal;

public static partial class obscuretestdata_package {

    // DecodeToTempFile decodes the named file to a temporary location.
    // If successful, it returns the path of the decoded file.
    // The caller is responsible for ensuring that the temporary file is removed.
public static (@string, error) DecodeToTempFile(@string name) => func((defer, _, _) => {
    @string path = default;
    error err = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(f.Close());

    var (tmp, err) = os.CreateTemp("", "obscuretestdata-decoded-");
    if (err != null) {
        return ("", error.As(err)!);
    }
    {
        var (_, err) = io.Copy(tmp, base64.NewDecoder(base64.StdEncoding, f));

        if (err != null) {
            tmp.Close();
            os.Remove(tmp.Name());
            return ("", error.As(err)!);
        }
    }

    {
        var err = tmp.Close();

        if (err != null) {
            os.Remove(tmp.Name());
            return ("", error.As(err)!);
        }
    }

    return (tmp.Name(), error.As(null!)!);

});

// ReadFile reads the named file and returns its decoded contents.
public static (slice<byte>, error) ReadFile(@string name) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(f.Close());
    return io.ReadAll(base64.NewDecoder(base64.StdEncoding, f));

});

} // end obscuretestdata_package
