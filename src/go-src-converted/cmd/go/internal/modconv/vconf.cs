// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:34 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\vconf.go
namespace go.cmd.go.@internal;

using strings = strings_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;

public static partial class modconv_package {

public static (ptr<modfile.File>, error) ParseVendorConf(@string file, slice<byte> data) {
    ptr<modfile.File> _p0 = default!;
    error _p0 = default!;

    ptr<modfile.File> mf = @new<modfile.File>();
    foreach (var (_, line) in strings.Split(string(data), "\n")) {
        {
            var i = strings.Index(line, "#");

            if (i >= 0) {
                line = line[..(int)i];
            }
        }
        var f = strings.Fields(line);
        if (len(f) >= 2) {
            mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:f[0],Version:f[1]})));
        }
    }    return (_addr_mf!, error.As(null!)!);
}

} // end modconv_package
