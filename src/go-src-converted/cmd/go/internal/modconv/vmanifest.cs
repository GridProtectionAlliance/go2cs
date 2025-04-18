// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:34 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\vmanifest.go
namespace go.cmd.go.@internal;

using json = encoding.json_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;

public static partial class modconv_package {

public static (ptr<modfile.File>, error) ParseVendorManifest(@string file, slice<byte> data) {
    ptr<modfile.File> _p0 = default!;
    error _p0 = default!;

    ref var cfg = ref heap(out ptr<var> _addr_cfg);
    {
        var err = json.Unmarshal(data, _addr_cfg);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    ptr<modfile.File> mf = @new<modfile.File>();
    foreach (var (_, d) in cfg.Dependencies) {
        mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:d.ImportPath,Version:d.Revision})));
    }    return (_addr_mf!, error.As(null!)!);
}

} // end modconv_package
