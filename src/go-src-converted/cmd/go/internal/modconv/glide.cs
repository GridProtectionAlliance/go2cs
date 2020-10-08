// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2020 October 08 04:35:29 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Go\src\cmd\go\internal\modconv\glide.go
using strings = go.strings_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modconv_package
    {
        public static (ptr<modfile.File>, error) ParseGlideLock(@string file, slice<byte> data)
        {
            ptr<modfile.File> _p0 = default!;
            error _p0 = default!;

            ptr<modfile.File> mf = @new<modfile.File>();
            var imports = false;
            @string name = "";
            foreach (var (_, line) in strings.Split(string(data), "\n"))
            {
                if (line == "")
                {
                    continue;
                }
                if (strings.HasPrefix(line, "imports:"))
                {
                    imports = true;
                }
                else if (line[0L] != '-' && line[0L] != ' ' && line[0L] != '\t')
                {
                    imports = false;
                }
                if (!imports)
                {
                    continue;
                }
                if (strings.HasPrefix(line, "- name:"))
                {
                    name = strings.TrimSpace(line[len("- name:")..]);
                }
                if (strings.HasPrefix(line, "  version:"))
                {
                    var version = strings.TrimSpace(line[len("  version:")..]);
                    if (name != "" && version != "")
                    {
                        mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:name,Version:version})));
                    }
                }
            }            return (_addr_mf!, error.As(null!)!);

        }
    }
}}}}
