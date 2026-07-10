// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package obscuretestdata contains functionality used by tests to more easily
// work with testdata that must be obscured primarily due to
// golang.org/issue/34986.
namespace go.@internal;

using base64 = encoding.base64_package;
using io = io_package;
using os = os_package;
using encoding;

partial class obscuretestdata_package {

// Rot13 returns the rot13 encoding or decoding of its input.
public static slice<byte> Rot13(slice<byte> data) {
    var @out = new slice<byte>(len(data));
    copy(@out, data);
    foreach (var (i, c) in @out) {
        switch (ᐧ) {
        case {} when (rune)'A' <= c && c <= (rune)'M' || (rune)'a' <= c && c <= (rune)'m': {
            @out[i] = (byte)(c + 13);
            break;
        }
        case {} when (rune)'N' <= c && c <= (rune)'Z' || (rune)'n' <= c && c <= (rune)'z': {
            @out[i] = (byte)(c - 13);
            break;
        }}

    }
    return @out;
}

// DecodeToTempFile decodes the named file to a temporary location.
// If successful, it returns the path of the decoded file.
// The caller is responsible for ensuring that the temporary file is removed.
public static (@string path, error err) DecodeToTempFile(@string name) {
    @string path = default!;
    error err = default!;
    func((defer, recover) => {
        (var f, err) = os.Open(name);
        if (err != default!) {
            (path, err) = ("", err); return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close());
        (var tmp, err) = os.CreateTemp(""u8, "obscuretestdata-decoded-"u8);
        if (err != default!) {
            (path, err) = ("", err); return;
        }
        {
            var (_, errΔ1) = io.Copy(new os.FileжWriter(tmp), base64.NewDecoder(base64.StdEncoding, new os_FileжReader(f))); if (errΔ1 != default!) {
                tmp.Close();
                os.Remove(tmp.Name());
                (path, err) = ("", errΔ1); return;
            }
        }
        {
            var errΔ2 = tmp.Close(); if (errΔ2 != default!) {
                os.Remove(tmp.Name());
                (path, err) = ("", errΔ2); return;
            }
        }
        (path, err) = (tmp.Name(), default!);
    });
    return (path, err);
}

// ReadFile reads the named file and returns its decoded contents.
public static (slice<byte>, error) ReadFile(@string name) => func<(slice<byte>, error)>((defer, recover) => {
    var (f, err) = os.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    var fʗ1 = f;
    defer(() => fʗ1.Close());
    return io.ReadAll(base64.NewDecoder(base64.StdEncoding, new os_FileжReader(f)));
});

} // end obscuretestdata_package
