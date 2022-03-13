// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 13 06:31:34 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\glide.go
namespace go.cmd.go.@internal;

using strings = strings_package;

using modfile = golang.org.x.mod.modfile_package;
using module = golang.org.x.mod.module_package;

public static partial class modconv_package {

public static (ptr<modfile.File>, error) ParseGlideLock(@string file, slice<byte> data) {
    ptr<modfile.File> _p0 = default!;
    error _p0 = default!;

    ptr<modfile.File> mf = @new<modfile.File>();
    var imports = false;
    @string name = "";
    foreach (var (_, line) in strings.Split(string(data), "\n")) {
        if (line == "") {
            continue;
        }
        if (strings.HasPrefix(line, "imports:")) {
            imports = true;
        }
        else if (line[0] != '-' && line[0] != ' ' && line[0] != '\t') {
            imports = false;
        }
        if (!imports) {
            continue;
        }
        if (strings.HasPrefix(line, "- name:")) {
            name = strings.TrimSpace(line[(int)len("- name:")..]);
        }
        if (strings.HasPrefix(line, "  version:")) {
            var version = strings.TrimSpace(line[(int)len("  version:")..]);
            if (name != "" && version != "") {
                mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:name,Version:version})));
            }
        }
    }    return (_addr_mf!, error.As(null!)!);
}

} // end modconv_package
