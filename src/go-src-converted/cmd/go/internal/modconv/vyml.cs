// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:35 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\vyml.go
namespace go.cmd.go.@internal;

using strings = strings_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;

public static partial class modconv_package {

public static (ptr<modfile.File>, error) ParseVendorYML(@string file, slice<byte> data) {
    ptr<modfile.File> _p0 = default!;
    error _p0 = default!;

    ptr<modfile.File> mf = @new<modfile.File>();
    var vendors = false;
    @string path = "";
    foreach (var (_, line) in strings.Split(string(data), "\n")) {
        if (line == "") {
            continue;
        }
        if (strings.HasPrefix(line, "vendors:")) {
            vendors = true;
        }
        else if (line[0] != '-' && line[0] != ' ' && line[0] != '\t') {
            vendors = false;
        }
        if (!vendors) {
            continue;
        }
        if (strings.HasPrefix(line, "- path:")) {
            path = strings.TrimSpace(line[(int)len("- path:")..]);
        }
        if (strings.HasPrefix(line, "  rev:")) {
            var rev = strings.TrimSpace(line[(int)len("  rev:")..]);
            if (path != "" && rev != "") {
                mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:path,Version:rev})));
            }
        }
    }    return (_addr_mf!, error.As(null!)!);
}

} // end modconv_package
